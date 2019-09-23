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

        ///<summary>The enclosing machine context.</summary>
        public ContextMesh ContextMesh;

        ContextMeshAdmin ContextMeshAdmin => ContextMesh as ContextMeshAdmin;

        public AccountEntry AccountEntry { get; private set; }


        ///<summary>The account activation</summary>
        public ActivationAccount ActivationAccount => AccountEntry.ActivationAccount;


        public ProfileAccount ProfileAccount => AccountEntry.ProfileAccount;

        ConnectionAccount ConnectionAccount => AccountEntry.ConnectionAccount;


        ///<summary>The Machine context.</summary>
        IMeshMachineClient MeshMachine => ContextMesh.MeshMachine;
        KeyCollection KeyCollection => MeshMachine.KeyCollection;

        ///<summary>The cryptographic parameters for reading/writing to account containers</summary>
        CryptoParameters containerCryptoParameters;


        public string KeySignatureUDF => keySignature.UDF;
        public string KeyEncryptionUDF => keyEncryption.UDF;
        public string KeyAuthenticationUDF => keyAuthentication.UDF;

        KeyPair keySignature;
        KeyPair keyEncryption;
        KeyPair keyAuthentication;

        public MeshService MeshClient => meshClient ??GetMeshClient(ServiceID).CacheValue (out meshClient);
        MeshService meshClient;


        public string ServiceID { get; private set; }


        ///<summary>The directory containing the catalogs related to the account.</summary>
        public string DirectoryAccount => directoryAccount ??
            Path.Combine(MeshMachine.DirectoryMesh, ActivationAccount.AccountUDF).CacheValue(out directoryAccount);
        string directoryAccount;

        Dictionary<string, SyncStatus> dictionaryStores = new Dictionary<string, SyncStatus>();


        protected override void Disposing() => spoolInbound?.Dispose();


        public ContextAccount(
                    ContextMesh contextMesh,
                    AccountEntry accountEntry,
                    string serviceID = null
                    ) {
            // Set up the basic context
            ContextMesh = contextMesh;
            AccountEntry = accountEntry;
            ServiceID = serviceID?? AccountEntry.ProfileAccount?.ServiceDefault;

            // Set up the crypto keys so that we can open the application catalog

            keySignature = ActivationAccount.KeySignature.GetPrivate(MeshMachine);

            keyEncryption = ActivationAccount.KeyEncryption.GetPrivate(MeshMachine);
            keyAuthentication = ActivationAccount.KeyAuthentication.GetPrivate(MeshMachine);
            KeyCollection.Add(keyEncryption);

            //ContainerCryptoParameters = new CryptoParameters(keyCollection: KeyCollection, recipient: KeyEncryption);
            containerCryptoParameters = new CryptoParameters(keyCollection: KeyCollection);
            }


        protected MeshService GetMeshClient(string serviceID) => 
                    MeshMachine.GetMeshClient(serviceID, keyAuthentication,
                ConnectionAccount, ContextMesh.ProfileMesh);

        /// <summary>
        /// Add a device to the device catalog.
        /// </summary>
        /// <param name="catalogedDevice"></param>
        public void AddDevice(CatalogedDevice catalogedDevice) {
            var catalog = GetCatalogDevice();
            var transaction = new TransactionServiced(catalog, MeshClient);
            transaction.Update(catalogedDevice);
            transaction.Commit();



            //GetCatalogDevice().Update(catalogedDevice);
            }

        /// <summary>
        /// Add service to the 
        /// </summary>
        /// <param name="serviceID"></param>
        /// <param name="sync"></param>
        public void AddService(
                string serviceID,
                bool sync = true) {
            // Add to assertion
            ProfileAccount.ServiceIDs = ProfileAccount.ServiceIDs ?? new List<string>();
            ProfileAccount.ServiceIDs.Add(serviceID);
            ContextMeshAdmin.Sign(ProfileAccount);
            ServiceID = serviceID;

            var createRequest = new CreateRequest() {
                ServiceID = serviceID,
                SignedAssertionAccount = ProfileAccount.DareEnvelope,
                SignedProfileMesh = ContextMesh.ProfileMesh.DareEnvelope
                };

            // Attempt to register with service in question
            MeshClient.CreateAccount(createRequest, MeshClient.JpcSession);

            // Update all the devices connected to this profile.
            UpdateDevices(ProfileAccount);
            
            if (sync) {
                GetCatalogApplication();
                GetCatalogContact();
                GetCatalogDevice();
                GetCatalogCredential();
                GetCatalogBookmark();
                GetCatalogCalendar();
                GetCatalogNetwork();
                SyncProgressUpload();
                }

            }


        void UpdateDevices(ProfileAccount account) {
            var catalog = GetCatalogDevice();

            var updates = new List<CatalogedDevice>();
            foreach (var device in catalog.AsCatalogEntryDevice) {
                bool updated = false;
                device.Accounts = device.Accounts ?? new List<AccountEntry>();


                foreach (var accountEntry in device.Accounts) {
                    if (accountEntry.AccountUDF == account.UDF) {
                        accountEntry.EnvelopedProfileAccount = account.DareEnvelope;
                        updated = true;
                        }
                    }

                if (!updated) {



                    }
                updates.Add(device);

                }
            foreach (var device in updates) {
                catalog.Update(device);
                if (device.UDF == ContextMesh.CatalogedDevice.UDF) {
                    ContextMesh.UpdateDevice(device);
                    }
                }

            }


        public void SetContactSelf(Contact contact) {
            ContextMeshAdmin.Sign(contact);
            GetCatalogContact().Add(contact, true);
            }


        #region // Convenience accessors for catalogs and stores

        ///<summary>Dictionary used to cache stores to avoid need to re-open them repeatedly.</summary>

        public bool SyncProgress(int maxEnvelopes = -1) => SyncProgressUpload(maxEnvelopes);


        public bool SyncProgressUpload(int maxEnvelopes = -1) {
            bool complete = true;
            var updates = new List<ContainerUpdate>();

            //// Always do the devices first (if we are an admin device)
            //if (SyncStatusDevice != null) {
            //    maxEnvelopes -= AddUpload(updates, SyncStatusDevice, maxEnvelopes);
            //    }

            // upload all the containers here
            foreach (var store in dictionaryStores) {
                maxEnvelopes -= AddUpload(updates, store.Value, maxEnvelopes);
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

            //Console.WriteLine($"Initial sync of {syncStatus.Store.ContainerName}");

            int uploads = 0;
            if (maxEnvelopes == 0) {
                return 0; // no more room left in this request.
                }


            if (syncStatus.Index <= syncStatus.Store.FrameCount) {
                var container = syncStatus.Store.Container;
                var envelopes = new List<DareEnvelope>();
                var containerUpdate = new ContainerUpdate() {
                    Container = syncStatus.Store.ContainerName,
                    Envelopes = envelopes,
                    Digest = container.Digest
                    // put the digest value here
                    };

                var start = 1 + syncStatus.Index;
                long last = (maxEnvelopes < 0) ? syncStatus.Store.FrameCount :
                    Math.Min((start + maxEnvelopes), syncStatus.Store.FrameCount);

                if (start == 0) {
                    envelopes.Add(container.FrameZero);
                    start++;
                    }


                for (var i = start; i < last; i++) {
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
            CatalogDevice.Label,
            CatalogContact.Label,
            CatalogCredential.Label,
            CatalogBookmark.Label,
            CatalogCalendar.Label,
            CatalogNetwork.Label
            };

        ///<summary>Returns the application catalog for the account</summary>
        public CatalogApplication GetCatalogApplication() => GetStore(CatalogApplication.Label) as CatalogApplication;

        ///<summary>Returns the contacts catalog for the account</summary>
        public CatalogDevice GetCatalogDevice() => GetStore(CatalogDevice.Label) as CatalogDevice;

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
        public Message GetPendingMessageConnectionRequest() =>
            GetPendingMessage(AcknowledgeConnection.__Tag);

        /// <summary>
        /// Return the latest unprocessed MessageContactRequest that was received.
        /// </summary>
        /// <returns>The latest unprocessed MessageContactRequest</returns>
        public Message GetPendingMessageContactRequest() =>
            GetPendingMessage(RequestContact.__Tag);

        /// <summary>
        /// Return the latest unprocessed MessageConfirmationRequest that was received.
        /// </summary>
        /// <returns>The latest unprocessed MessageConfirmationRequest</returns>
        public Message GetPendingMessageConfirmationRequest() =>
            GetPendingMessage(RequestConfirmation.__Tag);

        /// <summary>
        /// Return the latest unprocessed MessageConfirmationResponse that was received.
        /// </summary>
        /// <returns>The latest unprocessed MessageConfirmationResponse</returns>
        public Message GetPendingMessageConfirmationResponse() =>
            GetPendingMessage(ResponseConfirmation.__Tag);

        /// <summary>
        /// Search the inbound spool and 
        /// </summary>
        /// <param name="spoolInbound"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public Message GetPendingMessage(string tag) {
            var completed = new Dictionary<string, Message>();

            foreach (var message in GetSpoolInbound().Select(1, true)) {
                var meshMessage = Message.FromJSON(message.GetBodyReader());
                if (!completed.ContainsKey(meshMessage.MessageID)) {
                    if (meshMessage._Tag == tag) {
                        return meshMessage;
                        }
                    switch (meshMessage) {
                        case MessageComplete meshMessageComplete: {
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

        public void InitializeStores() => _ = Directory.CreateDirectory(DirectoryAccount);


        public Store GetStore(string name) {

            if (dictionaryStores.TryGetValue(name, out var syncStore)) {
                return syncStore.Store;
                }
            //Console.WriteLine($"Open store {name} on {MeshMachine.DirectoryMesh}");

            var store = MakeStore(name);
            //if (store is Catalog catalog) {
            //    catalog.TransactDelegate = Transact;
            //    }

            syncStore = new SyncStatus(store);

 
            dictionaryStores.Add(name, syncStore);

            return syncStore.Store;
            }

        Store MakeStore(string name) {
            switch (name) {
                case Spool.SpoolInbound: return new Spool(DirectoryAccount, name, containerCryptoParameters, KeyCollection);
                case Spool.SpoolOutbound: return new Spool(DirectoryAccount, name, containerCryptoParameters, KeyCollection);
                case Spool.SpoolArchive: return new Spool(DirectoryAccount, name, containerCryptoParameters, KeyCollection);

                case CatalogCredential.Label: return new CatalogCredential(DirectoryAccount, name, containerCryptoParameters, KeyCollection);
                case CatalogContact.Label: return new CatalogContact(DirectoryAccount, name, containerCryptoParameters, KeyCollection);
                case CatalogCalendar.Label: return new CatalogCalendar(DirectoryAccount, name, containerCryptoParameters, KeyCollection);
                case CatalogBookmark.Label: return new CatalogBookmark(DirectoryAccount, name, containerCryptoParameters, KeyCollection);
                case CatalogNetwork.Label: return new CatalogNetwork(DirectoryAccount, name, containerCryptoParameters, KeyCollection);
                case CatalogApplication.Label: return new CatalogApplication(DirectoryAccount, name, containerCryptoParameters, KeyCollection);
                case CatalogDevice.Label: return new CatalogDevice(DirectoryAccount, name, containerCryptoParameters, KeyCollection);
                }

            throw new NYI();
            }

        public bool Transact(Catalog catalog, List<CatalogUpdate> updates) {

            if (ServiceID == null) {
                return catalog.Transact(catalog, updates);
                }
            var envelopes = catalog.Prepare(updates);

            var containerUpdate = new ContainerUpdate() {
                Container = catalog.ContainerName,
                Index = (int)catalog.Container.FrameCount,
                Envelopes = envelopes
                };



            var uploadRequest = new UploadRequest() {
                Updates = new List<ContainerUpdate> { containerUpdate }
                };

            var uploadResponse = MeshClient.Upload(uploadRequest);


            catalog.Commit(envelopes);


            return true;
            }



        public MessagePIN GetPIN(int length = 80, int validity = 24 * 60) {
            var pin = UDF.Nonce(length);
            var expires = DateTime.Now.AddMinutes(validity);
            var messageConnectionPIN = new MessagePIN() {
                Account = ActivationAccount.AccountUDF,
                Expires = expires,
                PIN = pin,
                MessageID = UDF.Nonce()
                };

            SendMessageAdmin(messageConnectionPIN);

            return messageConnectionPIN;
            }


        /// <summary>
        /// Synchronize this device to the catalogs at the service. Since the authoritative copy of
        /// the service is held at the service, this means only downloading updates at present.
        /// </summary>
        /// <returns>The number of items synchronized</returns>
        public int Sync() {
            int count = 0;

            var statusRequest = new StatusRequest() {
                };
            var status = MeshClient.Status(statusRequest);

            (status.ContainerStatus == null).AssertFalse();


            var constraintsSelects = new List<ConstraintsSelect>();

            foreach (var container in status.ContainerStatus) {
                var store = GetStore(container.Container);


                //Console.WriteLine($"Container {container.Container}, items {container.Index} [{store.Container.FrameCount}]");

                if (store.Container.FrameCount < container.Index) {
                    var constraintsSelect = new ConstraintsSelect() {
                        Container = container.Container,
                        IndexMin = container.Index + 1,
                        IndexMax = (int)store.Container.FrameCount

                        };
                    constraintsSelects.Add(constraintsSelect);

                    }
                }

            if (constraintsSelects.Count == 0) {
                return 0;
                }

            var downloadRequest = new DownloadRequest() {

                Select = constraintsSelects
                };

            var download = MeshClient.Download(downloadRequest);

            foreach (var update in download.Updates) {
                var store = GetStore(update.Container);

                foreach (var entry in update.Envelopes) {
                    count++;
                    store.AppendDirect(entry);
                    }

                }


            // TBS: If we have synchronized the catalogs, upload cached offline updates here.

            return count;
            }

        public StatusResponse Status() {
            var statusRequest = new StatusRequest() {
                };
            return MeshClient.Status(statusRequest);
            }

        
        
        /// <summary>
        /// Accept a connection request.
        /// </summary>
        /// <param name="request">The request to accept.</param>
        void Accept(AcknowledgeConnection request) {
            // Connect the device to the Mesh
            var device = ContextMeshAdmin.CreateCataloguedDevice(
                            request.MessageConnectionRequest.ProfileDevice);

            // Connect the device to the Account
            ProfileAccount.ConnectDevice(MeshMachine, device, null);

            // Update the local and remote catalog.
            AddDevice(device);
            }



        //-------------------------------

        public void Download(long maxItems = -1) {
            }


        void Download() {
            }







        public void Process(Message meshMessage, bool accept = true, bool respond = true) {

            switch (meshMessage) {
                case AcknowledgeConnection connection: {
                    Accept(connection);
                    break;
                    }


                default: throw new NYI();
                }
            }


        public void ContactRequest(string serviceID) => throw new NYI();

        public void ConfirmationRequest(string serviceID, string messageText) => throw new NYI();

        void Connect() {
            if (MeshClient != null) {
                return;
                }

            ProfileAccount.ServiceIDs.AssertNotNull();
            (ProfileAccount.ServiceIDs.Count > 0).AssertTrue();

            ServiceID = ProfileAccount.ServiceIDs[0];

            meshClient = GetMeshClient(ServiceID);
            }


        public void SendMessage(Message MeshMessage) => Connect();

        /// <summary>
        /// Send a message signed using the mesh administration key.
        /// </summary>
        /// <param name="MeshMessage"></param>
        public void SendMessageAdmin(Message MeshMessage) {
            Connect();

            var message = DareEnvelope.Encode(MeshMessage.GetBytes());

            var postRequest = new PostRequest() {
                Self = new List<DareEnvelope>() { message }
                };


            MeshClient.Post(postRequest);
            }



        public CatalogedGroup CreateGroup(string groupName) => throw new NYI();
        public CatalogMember GetCatalogGroup(string groupName) => throw new NYI();
        }

    }
