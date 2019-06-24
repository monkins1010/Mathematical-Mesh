﻿using System;
using System.Collections.Generic;
using System.Text;
using Goedel.Utilities;
using Goedel.Cryptography;
using Goedel.Cryptography.Dare;
using Goedel.Cryptography.Jose;
using Goedel.Mesh;
using System.IO;

namespace Goedel.Mesh.Client {

    ///<summary>Track the synchronization status of an upload or download operation.</summary>
    public class SyncStatus {
        
        ///<summary>The local store</summary>
        public Store Store;

        ///<summary>The last index at the remote store</summary>
        public long Index;

        ///<summary>The apex digest value at the remote store</summary>
        public string Digest;

        public SyncStatus(Store store) {
            Store = store;
            Index = -1;
            Digest = null;
            }
        }

    public class ContextAccount : Disposable {

        ///<summary>The device profile to which the signature key is bound</summary>
        //public ProfileDevice ProfileDevice { get; }

        ///<summary>The enclosing machine context.</summary>
        public ContextMesh ContextMesh;

        ContextMeshAdmin ContextMeshAdmin => ContextMesh as ContextMeshAdmin;

        ///<summary>The account activation</summary>
        public ActivationAccount ActivationAccount;


        public AssertionAccount AssertionAccount;

        ///<summary>The Machine context.</summary>
        IMeshMachineClient MeshMachine => ContextMesh.MeshMachine;
        KeyCollection KeyCollection => MeshMachine.KeyCollection;

        ///<summary>The cryptographic parameters for reading/writing to account containers</summary>
        CryptoParameters ContainerCryptoParameters;


        public string KeySignatureUDF => KeySignature.UDF;
        public string KeyEncryptionUDF => KeyEncryption.UDF;
        public string KeyAuthenticationUDF => KeyAuthentication.UDF;

        KeyPair KeySignature;
        KeyPair KeyEncryption;
        KeyPair KeyAuthentication;

        MeshService MeshClient;
        string ServiceID;

        SyncStatus SyncStatusDevice;


        public string DirectoryAccount => directoryAccount ??
            Path.Combine(MeshMachine.DirectoryMesh, ActivationAccount.AccountUDF).CacheValue(out directoryAccount);
        string directoryAccount;

        Dictionary<string, SyncStatus> DictionaryStores = new Dictionary<string, SyncStatus>();

        public ContextAccount(
                    ContextMesh contextMesh,
                    ActivationAccount activationAccount,
                    AssertionAccount assertionAccount = null
                    ) {
            // Set up the basic context
            ContextMesh = contextMesh;
            ActivationAccount = activationAccount;

            // Set up the crypto keys so that we can open the application catalog
            KeySignature = activationAccount.KeySignature.GetPrivate(MeshMachine);
            KeyEncryption = activationAccount.KeyEncryption.GetPrivate(MeshMachine);
            KeyAuthentication = activationAccount.KeyAuthentication.GetPrivate(MeshMachine);
            KeyCollection.Add(KeyEncryption);

            ContainerCryptoParameters = new CryptoParameters(keyCollection: KeyCollection, recipient: KeyEncryption);


            if (assertionAccount == null) {
                var CatalogApplication = GetCatalogApplication();
                AssertionAccount = CatalogApplication.GetAssertionAccount(activationAccount.AccountUDF);
                }
            else {
                AssertionAccount = assertionAccount;
                }

            }


        protected MeshService GetMeshClient(string serviceID) => MeshMachine.GetMeshClient(serviceID, KeyAuthentication,
                ActivationAccount.AssertionAccountConnection, ContextMesh.ProfileMesh);




        public void AddService(
                string serviceID,
                bool sync = true) {
            // Add to assertion
            AssertionAccount.ServiceIDs = AssertionAccount.ServiceIDs ?? new List<string>();
            AssertionAccount.ServiceIDs.Add(serviceID);
            ContextMeshAdmin.Sign(AssertionAccount);

            var createRequest = new CreateRequest() {
                ServiceID = serviceID,
                SignedAssertionAccount = AssertionAccount.DareEnvelope,
                SignedProfileMesh = ContextMesh.ProfileMesh.DareEnvelope
                };

            // attempt to register with service in question

            MeshClient = GetMeshClient(serviceID);
            MeshClient.CreateAccount(createRequest, MeshClient.JpcSession);
            MeshClient.JpcSession.Authenticated = true;

            // Update the account assertion. This lives in CatalogApplication.
            AssertionAccount.ServiceIDs = AssertionAccount.ServiceIDs ?? new List<string>();
            AssertionAccount.ServiceIDs.Add(serviceID);
            GetCatalogApplication().Update(AssertionAccount);

            ServiceID = serviceID;

            SyncStatusDevice = new SyncStatus(ContextMeshAdmin.GetCatalogDevice()) {
                Index = -1
                };

            if (sync) {
                SyncProgressUpload();
                }


            //return service;
            }


        public void SetContactSelf(Contact contact) {
            ContextMeshAdmin.Sign(contact);
            GetCatalogContact().Add(contact, true);
            }


        #region // Convenience accessors for catalogs and stores

        ///<summary>Dictionary used to cache stores to avoid need to re-open them repeatedly.</summary>

        public bool SyncProgress(int maxEnvelopes = -1) {
            return SyncProgressUpload(maxEnvelopes);

            }


        public bool SyncProgressUpload(int maxEnvelopes = -1) {
            bool complete = true;
            var updates = new List<ContainerUpdate>();

            // Always do the devices first (if we are an admin device)
            if (SyncStatusDevice != null) {
                maxEnvelopes -= AddUpload(updates, SyncStatusDevice, maxEnvelopes);
                }

            if (updates.Count > 0) {
                var uploadRequest = new UploadRequest() {
                    Updates = updates
                    };
                MeshClient.Upload(uploadRequest);
                }

            return complete;
            }


        int AddUpload(List<ContainerUpdate> containerUpdates, SyncStatus syncStatus, int maxEnvelopes = -1) {
            int uploads = 0;
            if (maxEnvelopes == 0) {
                return 0; // no more room left in this request.
                }


            if (syncStatus.Index <= syncStatus.Store.FrameCount) {
                var container = syncStatus.Store.Container;
                var envelopes = new List<DareEnvelope>();
                var containerUpdate = new ContainerUpdate() {
                    Container = syncStatus.Store.ContainerName,
                    Envelopes = envelopes
                    };

                var start = 1+syncStatus.Index;
                long last = (maxEnvelopes < 0) ? syncStatus.Store.FrameCount :
                    Math.Min((start + maxEnvelopes), syncStatus.Store.FrameCount);

                if (start == 0) {
                    envelopes.Add(container.FrameZero);
                    start++;
                    }

                
                for (var i = start; i< last; i++) {
                    container.MoveToIndex(i);
                    envelopes.Add(container.ReadDirect());
                    }


                containerUpdates.Add(containerUpdate);
                }


            return uploads;

            }



        public List<string> Stores = new List<string> {
            Spool.SpoolOutbound,
            Spool.SpoolInbound,
            Spool.SpoolArchive,
            CatalogApplication.Label,
            CatalogContact.Label,
            CatalogCredential.Label,
            CatalogBookmark.Label,
            CatalogCalendar.Label,
            CatalogNetwork.Label
            };

        ///<summary>Returns the application catalog for the account</summary>
        public CatalogApplication GetCatalogApplication() => GetStore(CatalogApplication.Label) as CatalogApplication;


        ///<summary>Returns the contacts catalog for the account</summary>
        public CatalogContact GetCatalogContact() => GetStore(CatalogContact.Label) as CatalogContact;

        ///<summary>Returns the credential catalog for the account</summary>
        public CatalogCredential GetCatalogCredential() => GetStore(CatalogCredential.Label) as CatalogCredential;

        ///<summary>Returns the bookmark catalog for the account</summary>
        public CatalogBookmark GetCatalogBookmark() => GetStore(CatalogBookmark.Label) as CatalogBookmark;

        ///<summary>Returns the calendar catalog for the account</summary>
        public CatalogCalendar GetCatalogCalendar() => GetStore(CatalogCalendar.Label) as CatalogCalendar;

        ///<summary>Returns the network catalog for the account</summary>
        public CatalogNetwork GetCatalogNetwork() => GetStore(CatalogNetwork.Label) as CatalogNetwork;



        ///<summary>Returns the inbound spool for the account</summary>
        public Spool GetSpoolInbound() => spoolInbound ?? (GetStore(Spool.SpoolInbound) as Spool).CacheValue(out spoolInbound);
        Spool spoolInbound;


        ///<summary>Returns the outbound spool catalog for the account</summary>
        public Spool GetSpoolOutbound() => GetStore(Spool.SpoolOutbound) as Spool;

        /// <summary>
        /// Return the latest unprocessed MessageConnectionRequest that was received.
        /// </summary>
        /// <returns>The latest unprocessed MessageConnectionRequest</returns>
        public MeshMessage GetPendingMessageConnectionRequest() =>
            GetPendingMessage(MessageConnectionResponse.__Tag);

        /// <summary>
        /// Return the latest unprocessed MessageContactRequest that was received.
        /// </summary>
        /// <returns>The latest unprocessed MessageContactRequest</returns>
        public MeshMessage GetPendingMessageContactRequest() =>
            GetPendingMessage(MessageContactRequest.__Tag);

        /// <summary>
        /// Return the latest unprocessed MessageConfirmationRequest that was received.
        /// </summary>
        /// <returns>The latest unprocessed MessageConfirmationRequest</returns>
        public MeshMessage GetPendingMessageConfirmationRequest() =>
            GetPendingMessage(MessageConfirmationRequest.__Tag);

        /// <summary>
        /// Return the latest unprocessed MessageConfirmationResponse that was received.
        /// </summary>
        /// <returns>The latest unprocessed MessageConfirmationResponse</returns>
        public MeshMessage GetPendingMessageConfirmationResponse() =>
            GetPendingMessage(MessageConfirmationResponse.__Tag);

        /// <summary>
        /// Search the inbound spool and 
        /// </summary>
        /// <param name="spoolInbound"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public MeshMessage GetPendingMessage(string tag) {
            var completed = new Dictionary<string, MeshMessage>();

            foreach (var message in spoolInbound.Select(1, true)) {
                var meshMessage = MeshMessage.FromJSON(message.GetBodyReader());
                if (!completed.ContainsKey(meshMessage.MessageID)) {
                    if (meshMessage._Tag == tag) {
                        return meshMessage;
                        }
                    switch (meshMessage) {
                        case MeshMessageComplete meshMessageComplete: {
                            foreach (var reference in meshMessageComplete.References) {
                                completed.Add(reference.MessageID, meshMessageComplete);
                                // Hack: This should make actual use of the relationship
                                //   (Accept, Reject, Read)
                                }
                            break;
                            }
                        }
                    }
                }
            return null;
            }

        #endregion


        public Store GetStore(string name) {

            if (DictionaryStores.TryGetValue(name, out var syncStore)) {
                return syncStore.Store;
                }
            //Console.WriteLine($"Open store {name} on {MeshMachine.DirectoryMesh} {devicecount}");

            syncStore = new SyncStatus( MakeStore(name));
            DictionaryStores.Add(name, syncStore);

            return syncStore.Store;
            }

        Store MakeStore(string name) {
            switch (name) {
                case Spool.SpoolInbound: return new Spool(DirectoryAccount, name, ContainerCryptoParameters, KeyCollection);
                case Spool.SpoolOutbound: return new Spool(DirectoryAccount, name, ContainerCryptoParameters, KeyCollection);
                case Spool.SpoolArchive: return new Spool(DirectoryAccount, name, ContainerCryptoParameters, KeyCollection);

                case CatalogCredential.Label: return new CatalogCredential(DirectoryAccount, name, ContainerCryptoParameters, KeyCollection);
                case CatalogContact.Label: return new CatalogContact(DirectoryAccount, name, ContainerCryptoParameters, KeyCollection);
                case CatalogCalendar.Label: return new CatalogCalendar(DirectoryAccount, name, ContainerCryptoParameters, KeyCollection);
                case CatalogBookmark.Label: return new CatalogBookmark(DirectoryAccount, name, ContainerCryptoParameters, KeyCollection);
                case CatalogNetwork.Label: return new CatalogNetwork(DirectoryAccount, name, ContainerCryptoParameters, KeyCollection);
                case CatalogApplication.Label: return new CatalogApplication(DirectoryAccount, name, ContainerCryptoParameters, KeyCollection);
                }

            throw new NYI();
            }


        /////////////////////


        public string GetPIN(int length = 80, int validity = 24 * 60) {
            var pin = UDF.Nonce(length);
            var expires = DateTime.Now.AddMinutes(validity);
            var messageConnectionPIN = new MessageConnectionPIN() {
                Account = ActivationAccount.AccountUDF,
                Expires = expires,
                PIN = pin
                };

            SendMessageAdmin(messageConnectionPIN);

            return pin;
            }


        public void Download(long maxItems = -1) {
            }


        void Download() {
            }


        public void Sync(bool all = true) {

            //var statusRequest = new StatusRequest() {
            //    };
            //var status = MeshService.Status(statusRequest);

            //// for each container
            //if (all) {
            //    Download();
            //    }

            throw new NYI();
            }


        public void Process(MeshMessage meshMessage, bool accept = true, bool respond = true) {
            throw new NYI();
            }


        public void ContactRequest(string serviceID) {
            throw new NYI();
            }

        public void ConfirmationRequest(string serviceID, string messageText) {
            throw new NYI();
            }

        void Connect() {
            if (MeshClient != null) {
                return;
                }

            AssertionAccount.ServiceIDs.AssertNotNull();
            (AssertionAccount.ServiceIDs.Count > 0).AssertTrue();

            ServiceID = AssertionAccount.ServiceIDs[0];

            MeshClient = GetMeshClient(ServiceID);
            }


        public void SendMessage(MeshMessage MeshMessage) {
            Connect();

            }

        /// <summary>
        /// Send a message signed using the mesh administration key.
        /// </summary>
        /// <param name="MeshMessage"></param>
        public void SendMessageAdmin(MeshMessage MeshMessage) {
            Connect();

            var message = DareEnvelope.Encode(MeshMessage.GetBytes());

            var postRequest = new PostRequest() {
                Self = new List<DareEnvelope>() { message }
                };


            MeshClient.Post(postRequest);
            }


        }


    //public class ContextAccountService : ContextAccount {
    //    MeshService MeshService;
    //    string ServiceID;



    //    public ContextAccountService(ContextMesh contextMesh, ActivationAccount activationAccount) :
    //            base(contextMesh, activationAccount, null) {
    //        var CatalogApplication = GetCatalogApplication();
    //        // here pull the account assertion from the catalog
    //        AssertionAccount = CatalogApplication.GetAssertionAccount(activationAccount.AccountUDF);
    //        // now work out which service to use.


    //        }

    //    public ContextAccountService(ContextAccount contextAccount, MeshService meshService) :
    //                base(contextAccount) {
    //        MeshService = meshService;
    //        }




    //    public void Upload(long maxItems=-1) {

    //        //var updates = new List<ContainerUpdate>();


    //        //var uploadRequest = new UploadRequest() {
    //        //    Updates = updates
    //        //    };

    //        //int items = 0;
    //        //// Compile the set of updates
    //        //foreach (var storeEntry in DictionaryStores) {


    //        //    }

    //        //if (items == 0) {  // No work to be done!
    //        //    return;
    //        //    }

    //        //var uploadResponse = MeshService.Upload(uploadRequest);

    //        //// Now check off all the ones that have been updated.
    //        //foreach (var responseEntry in uploadResponse.Entries) {


    //        //    }


    //        }




    //    }
    }
