﻿using Goedel.Cryptography;
using Goedel.Cryptography.Dare;
using Goedel.Utilities;

using System;
using System.Collections.Generic;

namespace Goedel.Mesh.Client {

    ///<summary>Track the synchronization status of an upload or download operation.</summary>
    public class SyncStatus {

        ///<summary>The local store</summary>
        public Store Store;

        ///<summary>The last index at the remote store</summary>
        public long Index;

        ///<summary>The apex digest value at the remote store</summary>
        public string Digest;

        /// <summary>
        /// Report the synchronization status of a Mesh store.
        /// </summary>
        /// <param name="store">The store reported on.</param>
        public SyncStatus(Store store) {
            Store = store;
            Index = -1;
            Digest = null;
            }
        }

    /// <summary>
    /// Base class from which Contexts for Accounts and Groups are derrived. These are
    /// separate contexts but share functions and thus code.
    /// </summary>
    public abstract class ContextAccountEntry : Disposable, IKeyLocate {

        #region // Properties
        ///<summary>The enclosing mesh context.</summary>
        public abstract ContextMesh ContextMesh { get; }

        ///<summary>The enclosing mesh context as an administrative context (if rights granted.</summary>
        protected ContextMeshAdmin ContextMeshAdmin => ContextMesh as ContextMeshAdmin;

        ///<summary>The Machine context.</summary>
        protected IMeshMachineClient MeshMachine => ContextMesh.MeshMachine;

        ///<summary>The key collection for use with the context.</summary>
        protected KeyCollection KeyCollection => ContextMesh.KeyCollection;


        public abstract Connection Connection { get; }



        ///<summary>The member's device signature key</summary>
        protected KeyPair KeySignature { get; set; }

        ///<summary>The group encryption key </summary>
        protected KeyPair KeyEncryption { get; set; }

        ///<summary>The authentication key used by this client to connect to the group</summary>

        protected KeyPair KeyAuthentication { get; set; }


        ///<summary>Convenience accessor for the encryption key fingerprint.</summary>
        public string KeyEncryptionUDF => KeyEncryption.KeyIdentifier;
        ///<summary>Convenience accessor for the signature key fingerprint.</summary>
        public string KeySignatureUDF => KeySignature.KeyIdentifier;
        ///<summary>Convenience accessor for the authentication key fingerprint.</summary>
        public string KeyAuthenticationUDF => KeyAuthentication.KeyIdentifier;


        ///<summary>The directory containing the catalogs related to the account.</summary>
        public virtual string StoresDirectory { get; set; }

        ///<summary>Dictionary locating the stores connected to the context.</summary>
        protected Dictionary<string, SyncStatus> DictionaryStores = new Dictionary<string, SyncStatus>();

        ///<summary>The cryptographic parameters for reading/writing to account containers</summary>
        protected CryptoParameters ContainerCryptoParameters;


        ///<summary>Returns the MeshClient and caches the result for future use.</summary>
        public MeshService MeshClient => meshClient ?? GetMeshClient(AccountAddress).CacheValue(out meshClient);
        MeshService meshClient;

        ///<summary>The Account Address</summary>
        public virtual string AccountAddress { get; protected set; }


        ///<summary>Returns the network catalog for the account</summary>
        public CatalogCapability GetCatalogCapability() => GetStore(CatalogCapability.Label) as CatalogCapability;
        
        ///<summary>Returns the local spool  for the account</summary>
        public SpoolLocal GetSpoolLocal() => GetStore(SpoolLocal.Label) as SpoolLocal;
        #endregion

        #region // PIN code generation and use
        /// <summary>
        /// Create a PIN value of length <paramref name="length"/> bits valid for 
        /// <paramref name="validity"/> minutes.
        /// </summary>
        /// <param name="length">The size of the PIN value to create in bits.</param>
        /// <param name="validity">The validity interval in minutes from the current 
        /// date and time.</param>
        /// <param name="action">The action to which this pin is bound.</param>
        /// <param name="automatic">If true, presentation of the pin code is sufficient
        /// to authenticate and authorize the action.</param>
        /// <returns>A <see cref="MessagePIN"/> instance describing the created parameters.</returns>
        public MessagePIN GetPIN(string action, bool automatic = true, 
                            int length = 80, long validity = Constants.DayInTicks) {
            var pin = UDF.SymmetricKey(length);
            var expires = DateTime.Now.AddTicks(validity);

            return RegisterPIN(pin, automatic, expires, AccountAddress, action);
            }

        /// <summary>
        /// Register the pin code <paramref name="pin"/> to the account <paramref name="accountAddress"/>
        /// bound to the action <paramref name="action"/>.
        /// </summary>
        /// <param name="pin">The PIN code</param>
        /// <param name="automatic">If true, proof of knowledge of the pin is sufficient authorization.</param>
        /// <param name="expires">Expiry time.</param>
        /// <param name="accountAddress">The account to which the pin is bound.</param>
        /// <param name="action">The action to which the pin is bound.</param>
        /// <returns>The message registered on the Admin spool.</returns>
        protected MessagePIN RegisterPIN(string pin, bool automatic, DateTime? expires, string accountAddress, string action) {
            var messageConnectionPIN = new MessagePIN(pin, automatic, expires, accountAddress, action);

            SendMessageAdmin(messageConnectionPIN);
            return messageConnectionPIN;
            }

        /// <summary>
        /// Fetch the <see cref="MessagePIN"/> with the identifier <paramref name="PinUDF"/>.
        /// </summary>
        /// <param name="PinUDF">The identifier of the PIN</param>
        /// <returns>The message (if found), otherwise null.</returns>
        public MessagePIN GetMessagePIN(string PinUDF) {
            var pinCreate = GetSpoolLocal().CheckPIN(PinUDF);

            // check PIN
            if (pinCreate == null || pinCreate.Closed) {
                "Should collect up errors for optional reporting".TaskValidate();
                "Should check on expiry".TaskValidate();
                throw new NYI();
                //return InvalidPIN();
                }

            return pinCreate.Message as MessagePIN;
            }

        #endregion

        #region // Account operations sync, send message

        /// <summary>
        /// Returns a Mesh service client for <paramref name="accountAddress"/>.
        /// </summary>
        /// <param name="accountAddress">The account service identifier.</param>
        /// <returns>The Mesh service client</returns>
        protected MeshService GetMeshClient(string accountAddress) =>
                    MeshMachine.GetMeshClient(accountAddress, KeyAuthentication,
                            Connection, ContextMesh.ProfileMesh);


        /// <summary>
        /// Obtain the status of the remote store.
        /// </summary>
        /// <returns></returns>
        public StatusResponse Status() {
            var statusRequest = new StatusRequest() {
                };
            return MeshClient.Status(statusRequest);
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
                var constraintsSelect = GetStoreStatus(container);
                if (constraintsSelect != null) {
                    constraintsSelects.Add(constraintsSelect);
                    }
                }

            if (constraintsSelects.Count == 0) {
                return 0;
                }

            var downloadRequest = new DownloadRequest() {

                Select = constraintsSelects
                };

            // what is it with the ranges here? make sure they are all correct.
            // Then check that the remote versions are correct.

            var download = MeshClient.Download(downloadRequest);

            foreach (var update in download.Updates) {
                count += UpdateStore(update);
                }

            // At this point we want to look at all the pending messages and see if there
            // are any PIN authenticated auto-executing messages.
            // TBS: If we have synchronized the catalogs, upload cached offline updates here.

            return count;
            }

        /// <summary>
        /// Send <paramref name="meshMessage"/> to <paramref name="recipient"/>.
        /// </summary>
        /// <param name="meshMessage">The message to send.</param>
        /// <param name="recipient">The recipient service ID.</param>
        public void SendMessage(Message meshMessage, string recipient) =>
            SendMessage(meshMessage, new List<string> { recipient });


        /// <summary>
        /// Post the message <paramref name="meshMessage"/> to the service. If <paramref name="recipients"/>
        /// is not null, the message is to be posted to the outbound spool to be forwarded to the
        /// appropriate Mesh Service. Otherwise, the message is posted to the local spool for local
        /// collection.
        /// </summary>
        /// <param name="meshMessage">The message to post</param>
        /// <param name="recipients">The recipients the message is to be sent to. If null, the
        /// message is for local pickup.</param>
        public void SendMessage(
                    Message meshMessage,
                    List<string> recipients = null) {
            Connect();

            meshMessage.Sender = AccountAddress;


            var envelope = meshMessage.Encode();

            var postRequest = new PostRequest() {
                Accounts = recipients,
                Message = new List<DareEnvelope>() { envelope }
                };


            MeshClient.Post(postRequest);
            }

        /// <summary>
        /// Send a message signed using the mesh administration key.
        /// </summary>
        /// <param name="meshMessage"></param>
        public void SendMessageAdmin(Message meshMessage) {
            Connect();

            var message = meshMessage.Encode(KeySignature);

            var postRequest = new PostRequest() {
                Self = new List<DareEnvelope>() { message }
                };


            MeshClient.Post(postRequest);
            }

        void Connect() {
            if (MeshClient != null) {
                return;
                }


            AccountAddress = GetAccountAddress();

            meshClient = GetMeshClient(AccountAddress);
            }


        public abstract string GetAccountAddress();

        #endregion

        #region // Contact management

        /// <summary>
        /// Get the default (i.e. minimum contact info). This has a single network 
        /// address entry for this mesh and mesh account. 
        /// </summary>
        /// <returns>The default contact.</returns>
        public abstract Contact CreateDefaultContact(bool meshUDF = false);

        #endregion

        #region // Store management

        /// <summary>
        /// Return a <see cref="ConstraintsSelect"/> instance that requests synchronization to the
        /// remote store whose status is described by <paramref name="statusRemote"/>.
        /// </summary>
        /// <param name="statusRemote">Status of the remote store.</param>
        /// <returns>The selection constraints.</returns>
        public ConstraintsSelect GetStoreStatus(ContainerStatus statusRemote) {
            if (DictionaryStores.TryGetValue(statusRemote.Container, out var syncStore)) {
                var storeLocal = syncStore.Store;

                return storeLocal.FrameCount >= statusRemote.Index ? null :
                    new ConstraintsSelect() {
                        Container = statusRemote.Container,
                        IndexMax = statusRemote.Index,
                        IndexMin = (int)storeLocal.FrameCount
                        };
                }

            else {
                using var storeLocal = new Store(StoresDirectory, statusRemote.Container,
                            decrypt: false, create: false);
                //Console.WriteLine($"Container {statusRemote.Container}   Local {storeLocal.FrameCount} Remote {statusRemote.Index}");
                return storeLocal.FrameCount >= statusRemote.Index ? null :
                    new ConstraintsSelect() {
                        Container = statusRemote.Container,
                        IndexMax = statusRemote.Index,
                        IndexMin = (int)storeLocal.FrameCount
                        };
                }
            }

        /// <summary>
        /// Update the store according to the values <paramref name="containerUpdate"/>.
        /// </summary>
        /// <param name="containerUpdate">The update to apply.</param>
        /// <returns>The number of envelopes successfully added.</returns>
        public int UpdateStore(ContainerUpdate containerUpdate) {
            int count = 0;
            if (DictionaryStores.TryGetValue(containerUpdate.Container, out var syncStore)) {
                var store = syncStore.Store;
                foreach (var entry in containerUpdate.Envelopes) {
                    if (entry.Index == 0) {
                        throw new NYI();
                        }

                    count++;
                    store.AppendDirect(entry);
                    }
                return count;
                }

            else {
                // we have zero envelopes being returned in this update.

                Store.Append(StoresDirectory, containerUpdate.Envelopes, containerUpdate.Container);
                return containerUpdate.Envelopes.Count;
                }

            }

        /// <summary>
        /// Return a <see cref="Store"/> instance for the store named <paramref name="name"/>. If the
        /// parameter <paramref name="blind"/> is true, only the sequence header values are read.
        /// </summary>
        /// <param name="name">The store to open.</param>
        /// <param name="blind">If true, only the sequence header values are read</param>
        /// <returns>The <see cref="Store"/> instance.</returns>
        public Store GetStore(string name, bool blind = false) {
            if (DictionaryStores.TryGetValue(name, out var syncStore)) {
                if (!blind & (syncStore.Store is CatalogBlind)) {
                    // if we have a blind store from a sync operation but need a populated one,
                    // remake it.
                    syncStore.Store.Dispose();
                    syncStore.Store = MakeStore(name);
                    }
                return syncStore.Store;
                }

            //Console.WriteLine($"Open store {name} on {MeshMachine.DirectoryMesh}");

            var store = blind ? new CatalogBlind(StoresDirectory, name) : MakeStore(name);
            syncStore = new SyncStatus(store);

            DictionaryStores.Add(name, syncStore);
            return syncStore.Store;
            }

        /// <summary>
        /// Create a new instance bound to the specified core within this account context.
        /// </summary>
        /// <param name="name">The name of the store to bind.</param>
        /// <returns>The store instance.</returns>
        protected virtual Store MakeStore(string name) => name switch
            {
                CatalogCapability.Label => new CatalogCapability(StoresDirectory, name, ContainerCryptoParameters, KeyCollection),
                _ => throw new NYI(),
                };
        #endregion



        #region Implement IKeyLocate


        /// <summary>
        /// Resolve a public encryption key by identifier. This may be a UDF fingerprint of the key,
        /// an account identifier or strong account identifier.
        /// </summary>
        /// <param name="keyID">The identifier to resolve.</param>
        /// <returns>The identifier.</returns>
        public CryptoKey GetByAccountEncrypt(string keyID) => throw new NotImplementedException();

        /// <summary>
        /// Resolve a public signature key by identifier. This may be a UDF fingerprint of the key,
        /// an account identifier or strong account identifier.
        /// </summary>
        /// <param name="keyID">The identifier to resolve.</param>
        /// <returns>The identifier.</returns>
        public CryptoKey GetByAccountSign(string keyID) => throw new NotImplementedException();

        /// <summary>
        /// Attempt to obtain a private key with identifier <paramref name="keyID"/>.
        /// </summary>
        /// <param name="keyID">The key identifier to match.</param>
        /// <returns>The key pair if found.</returns>
        public CryptoKey LocatePrivateKeyPair(string keyID) => throw new NotImplementedException();

        /// <summary>
        /// Attempt to obtain a recipient with identifier <paramref name="keyID"/>.
        /// </summary>
        /// <param name="keyID">The key identifier to match.</param>
        /// <returns>The key pair if found.</returns>
        public CryptoKey TryMatchRecipient(string keyID) => throw new NotImplementedException();
        #endregion



        #region Implement IDare

        // Bug: this is going to fail because information from the contact catalog is not available.


        /// <summary>
        /// Create a new DARE Envelope from the specified parameters.
        /// </summary>
        /// <param name="plaintext">The payload plaintext. If specified, the plaintext will be used to
        /// create the message body. Otherwise the body is specified by calls to the Process method.</param>
        /// <param name="contentMeta">The content metadata</param>
        /// <param name="cloaked">Data to be converted to an EDS and presented as a cloaked header.</param>
        /// <param name="dataSequences">Data sequences to be converted to an EDS and presented 
        /// as an EDSS header entry.</param>
        /// <param name="recipients">If specified, encrypt the envelope with decryption blobs
        /// for the specified recipients.</param>
        /// <param name="sign">If true sign the envelope.</param>
        /// <returns></returns>
        public DareEnvelope DareEncode(
                    byte[] plaintext,
                    ContentMeta contentMeta = null,
                    byte[] cloaked = null,
                    List<byte[]> dataSequences = null,
                    List<string> recipients = null,
                    bool sign = false) {


            true.AssertFalse(); // This method has serious issues.

            KeyPair signingKey = sign ? KeySignature : null;
            List<CryptoKey> encryptionKeys = null;

            if (recipients != null) {
                foreach (var recipient in recipients) {
                    encryptionKeys.Add(GetByAccountEncrypt(recipient));
                    }
                }

            var cryptoParameters = new CryptoParameters(signer: signingKey, recipients: null);
            return new DareEnvelope(cryptoParameters, plaintext, contentMeta, cloaked, dataSequences);

            }

        /// <summary>
        /// Decode a DARE envelope
        /// </summary>
        /// <param name="envelope">The envelope to decode.</param>
        /// <param name="verify">It true, verify the signature first.</param>
        /// <returns>The plaintext payload data.</returns>
        public byte[] DareDecode(
                    DareEnvelope envelope,
                    bool verify = false) => envelope.GetPlaintext(this);

        #endregion

        }
    }
