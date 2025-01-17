﻿#region // Copyright - MIT License
//  © 2021 by Phill Hallam-Baker
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
#endregion


namespace Goedel.Mesh.Client;

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
public abstract partial class ContextAccount : Disposable, IKeyCollection, IMeshClient {

    #region // Properties

    #region // Machine and context properties
    ///<summary>The Mesh host</summary>
    public MeshHost MeshHost { get; }

    ///<summary>The Device Entry in the CatalogHost</summary>
    public CatalogedMachine CatalogedMachine;

    ///<summary>The account profile</summary>
    public abstract Profile Profile { get; }

    ///<summary>The device profile</summary>
    public virtual ProfileDevice ProfileDevice => CatalogedMachine?.ProfileDevice;

    ///<summary>The cataloged device</summary>
    public virtual CatalogedDevice CatalogedDevice => CatalogedMachine?.CatalogedDevice;


    ///<summary>The current service profile</summary> 
    public virtual ProfileService ProfileService { get; set; }

    ///<summary>The Machine context.</summary>
    protected IMeshMachineClient MeshMachine => MeshHost.MeshMachine;

    ///<summary>The key collection for use with the context.</summary>
    public virtual IKeyCollection KeyCollection => MeshMachine.KeyCollection;

    ///<summary>The connection binding the calling context to the account.</summary>
    public abstract Connection Connection { get; }

    /// <summary>
    /// Create a new ICredential.
    /// </summary>
    /// <returns>The credential</returns>
    public virtual MeshCredentialPrivate GetMeshCredentialPrivate() {
        var profileDevice = ProfileDevice;
        profileDevice.Activate(KeyCollection);
        return new(profileDevice, null, null, profileDevice.KeyAuthentication as KeyPairAdvanced);
        }
    ///<summary>Returns the MeshClient and caches the result for future use.</summary>
    public virtual MeshServiceClient MeshClient {
        get => meshClient ??
          GetMeshClient(GetMeshCredentialPrivate()).CacheValue(out meshClient);
        set => meshClient = value;
        }

    MeshServiceClient meshClient;

    ///<summary>The host assignment binding.</summary> 
    public AccountHostAssignment AccountHostAssignment => accountHostAssignment ??
        CatalogedMachine?.EnvelopedAccountHostAssignment?.Decode().CacheValue(out accountHostAssignment);
    AccountHostAssignment accountHostAssignment;

    ///<summary>The host encryption key.</summary> 
    public KeyPair HostEncryptAccount => hostEncryptAccount ??
        AccountHostAssignment?.AccessEncrypt?.GetKeyPair().CacheValue(out hostEncryptAccount);
    KeyPair hostEncryptAccount;
    ///<summary>The Account Address</summary>
    public abstract string AccountAddress { get; }
    #endregion
    #region // Activated account keys
    ///<summary>The account activation</summary>
    public ActivationCommon ActivationCommon { get; set; }

    ///<summary>The device identifier within the account.</summary> 
    public string AccountDeviceId => ActivationCommon.AccountDeviceId;


    ///<summary>The account profile key</summary>
    protected KeyPair KeyProfile => ActivationCommon?.ProfileSignatureKey;
    ///<summary>The administration signature key</summary>
    protected KeyPair KeyAdministratorSign => ActivationCommon?.AdministratorSignatureKey;
    ///<summary>The administration signature key</summary>
    protected KeyPair KeyAdministratorEncrypt => ActivationCommon?.AdministratorEncryptionKey;


    ///<summary>The account encryption key </summary>
    protected KeyPair KeyCommonSignature => ActivationCommon?.CommonSignatureKey;
    ///<summary>The account encryption key </summary>
    protected KeyPair KeyCommonEncryption => ActivationCommon?.CommonEncryptionKey;
    ///<summary>The authentication key used to authenticate as the account.</summary>
    protected KeyPair KeyCommonAuthentication => ActivationCommon?.CommonAuthenticationKey;


    ///<summary>True iff the device has administrator privilege.</summary> 
    protected bool IsAdministrator => KeyAdministratorSign != null;

    ///<summary>The set of assigned privileges.</summary> 
    protected HashSet<string> Privileges { get; } = new();


    #endregion
    #region // Store definitions
    ///<summary>The directory containing the catalogs related to the account.</summary>
    public virtual string StoresDirectory { get; set; }

    ///<summary>Dictionary locating the stores connected to the context.</summary>
    public Dictionary<string, SyncStatus> DictionaryStores = new();

    ///<summary>List of catalogs</summary>
    public virtual Dictionary<string, StoreFactoryDelegate> DictionaryCatalogDelegates => catalogDelegates;

    readonly Dictionary<string, StoreFactoryDelegate> catalogDelegates = new() {
        // All contexts have a capability catalog:
            { CatalogAccess.Label, CatalogAccess.Factory },
            { CatalogPublication.Label, CatalogPublication.Factory }
        };
    ///<summary>List of spools, these are the same for each type of account.</summary>
    public virtual Dictionary<string, StoreFactoryDelegate> DictionarySpoolDelegates => StaticSpoolDelegates;

    ///<summary>List of spools, these are the same for each type of account.</summary>
    protected static Dictionary<string, StoreFactoryDelegate> StaticSpoolDelegates { get; set; } = new() {
            { SpoolInbound.Label, SpoolInbound.Factory },
            { SpoolOutbound.Label, SpoolOutbound.Factory },
            { SpoolLocal.Label, SpoolLocal.Factory },
            { SpoolArchive.Label, SpoolArchive.Factory },
        };
    #endregion
    #endregion


    static int countStores = 0;

    /// <summary>
    /// Disposal method called on exit.
    /// </summary>
    protected override void Disposing() {
        //Screen.WriteLine($"*** Dispose ContextAccount {AccountAddress} {countStores}");
        countStores--;
        foreach (var status in DictionaryStores) {
            var store = status.Value.Store;
            store.Dispose();
            }

        }

    #region // Constructors

    /// <summary>
    /// Constructor, creates a <see cref="ContextUser"/> instance for the catalog entry 
    /// <paramref name="catalogedMachine"/> on machine <paramref name="meshHost"/>.
    /// </summary>
    /// <param name="catalogedMachine">Description of the device profile.</param>
    /// <param name="meshHost">The Mesh host to add the admin context to.</param>
    public ContextAccount(
            MeshHost meshHost,
            CatalogedMachine catalogedMachine) {
        countStores++;
        //Screen.WriteLine($"*** Create ContextAccount {AccountAddress} {countStores}");


        MeshHost = meshHost;
        CatalogedMachine = catalogedMachine;
        }


    ///// <summary>
    ///// Constructor, generate a new context from the activation record 
    ///// <paramref name="activationAccount"/>.
    ///// </summary>
    ///// <param name="activationAccount">The activation record.</param>
    //public ContextAccount(ActivationAccount activationAccount) =>
    //    ActivationAccount = activationAccount;


    #endregion


    #region // PIN code generation and use
    /// <summary>
    /// Create a PIN value of length <paramref name="bits"/> bits valid for 
    /// <paramref name="validity"/> minutes.
    /// </summary>
    /// <param name="bits">The size of the PIN value to create in bits.</param>
    /// <param name="validity">The validity interval in minutes from the current 
    /// date and time.</param>
    /// <param name="action">The action to which this pin is bound.</param>
    /// <param name="automatic">If true, presentation of the pin code is sufficient
    /// to authenticate and authorize the action.</param>
    /// <param name="register">If true, register the pin at the service.</param>
    /// <param name="encryptKey">The encryption key to be used to encrypt the PIN registration.</param>
    /// <param name="roles">The authorized roles.</param>
    /// <returns>A <see cref="MessagePin"/> instance describing the created parameters.</returns>
    public MessagePin GetPIN(string action, bool automatic = true,
                        int bits = 120, long validity = MeshConstants.DayInTicks,
                        bool register = true, CryptoKey encryptKey = null,
                        List<string> roles = null) {


        var pin = UDF.AuthenticationKey(bits);
        var expires = DateTime.Now.AddTicks(validity);
        var messagePin = new MessagePin(pin, automatic, expires, AccountAddress, action) {
            Roles = roles
            };

        encryptKey ??= KeyCommonEncryption;

        if (register) {
            var transactRequest = TransactBegin();
            transactRequest.LocalMessage(messagePin, encryptKey);
            transactRequest.Transact();
            }
        return messagePin;
        }


    /// <summary>
    /// Fetch the <see cref="MessagePin"/> with the identifier <paramref name="PinUDF"/>.
    /// </summary>
    /// <param name="PinUDF">The identifier of the PIN</param>
    /// <returns>The message (if found), otherwise null.</returns>
    public MessagePin GetMessagePIN(string PinUDF) {
        var spoolLocal = GetStore(SpoolLocal.Label) as SpoolLocal;

        var spooledPin = spoolLocal.CheckPIN(PinUDF);
        if (spooledPin == null) {
            return null;
            }

        var result = spooledPin?.Message as MessagePin;
        result.MessageStatus = spooledPin.MessageStatus;

        return result;
        }

    #endregion
    #region // Account operations sync, send message

    /// <summary>
    /// Delete the associated account from the local machine.
    /// </summary>
    public void DeleteAccount() {
        var unbindRequest = new UnbindRequest() {
            Account = AccountAddress
            };
        var response = MeshClient.UnbindAccount(unbindRequest);
        response.AssertSuccess(NYI.Throw);

        // close all open stores and clear the dictionary
        foreach (var status in DictionaryStores) {
            var store = status.Value.Store;
            store.Dispose();
            }
        DictionaryStores.Clear();

        // erase files from local storage
        Directory.Delete(StoresDirectory, true);

        // Erase from the registry
        MeshHost.Deregister(this);
        MeshHost.Delete(Profile.Udf);
        }


    /// <summary>
    /// Returns a Mesh service client for <paramref name="credentialPrivate"/>.
    /// </summary>
    /// <returns>The Mesh service client</returns>
    public MeshServiceClient GetMeshClient(ICredentialPrivate credentialPrivate) =>
                MeshMachine.GetMeshClient(credentialPrivate, Profile.Udf);


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
            CatalogedDeviceDigest = CatalogedMachine.CatalogedDeviceDigest
            };
        var status = MeshClient.Status(statusRequest);

        status.ContainerStatus.AssertNotNull(ServerResponseInvalid.Throw, status);
        if (status.EnvelopedCatalogedDevice != null) {
            var catalogedDevice = status.EnvelopedCatalogedDevice.Decode(this);
            UpdateCatalogedMachine(catalogedDevice, status.CatalogedDeviceDigest, true);
            }

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

        // check here to see if we have an update to the Cataloged Device


        foreach (var update in download.Updates) {
            count += UpdateStore(update);
            }

        // At this point we want to look at all the pending messages and see if there
        // are any PIN authenticated auto-executing messages.
        // TBS: If we have synchronized the catalogs, upload cached offline updates here.

        return count;
        }

    //public bool SyncProgress(int maxEnvelopes = -1) => SyncProgressUpload(maxEnvelopes);



    /// <summary>
    /// Update the <paramref name="catalogedDevice"/> entry in the machine catalog.
    /// </summary>
    /// <param name="catalogedDevice">The entry to update.</param>
    /// <param name="digestUDF">The UDF of the digest value of the cataloged devbice data.</param>
    /// <param name="registerContext">If true associate register this context with the host.</param>
    public void UpdateCatalogedMachine(CatalogedDevice catalogedDevice,
            string digestUDF, bool registerContext) {

        //Screen.WriteLine($"Install/update: {digestUDF} Entries: {catalogedDevice.ApplicationEntries?.Count}");



        CatalogedMachine.CatalogedDevice = catalogedDevice;
        CatalogedMachine.CatalogedDeviceDigest = digestUDF;
        //UDF.Sha2ToString(catalogedDevice.DareEnvelope?.PayloadDigest);
        MeshHost.Register(CatalogedMachine, registerContext ? this : null);
        }


    /// <summary>
    /// Synchronize the device to the store in increments of no more than <paramref name="maxEnvelopes"/>
    /// at a time. This should really be changed to something more Async callback friendly. Hours in
    /// a day... ??? Its midnight.
    /// </summary>
    /// <param name="maxEnvelopes">The maximum number of envelopes to return.</param>
    /// <returns>If true, the synchronization has completed.</returns>
    public bool SyncProgressUpload(int maxEnvelopes = -1) {
        bool complete = true;
        var updates = new List<ContainerUpdate>();

        //// Always do the devices first (if we are an admin device)
        //if (SyncStatusDevice != null) {
        //    maxEnvelopes -= AddUpload(updates, SyncStatusDevice, maxEnvelopes);
        //    }

        try {
            // upload all the containers here

            // This is not working right because it is uploading all the envelopes every time
            // regardless of the remote store status.
            foreach (var store in DictionaryStores) {
                maxEnvelopes -= AddUpload(updates, store.Value, maxEnvelopes);
                }
            }
        catch {
            }


        if (updates.Count > 0) {
            var uploadRequest = new TransactRequest() {
                Updates = updates
                };
            MeshClient.Transact(uploadRequest);
            }

        return complete;
        }

    static int AddUpload(List<ContainerUpdate> containerUpdates, SyncStatus syncStatus, int maxEnvelopes = -1) {

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

    #endregion
    #region // Contact management

    /// <summary>
    /// Get the default (i.e. minimum contact info). This has a single network 
    /// address entry for this mesh and mesh account. 
    /// </summary>
    /// <returns>The default contact.</returns>
    public virtual Contact CreateContact(
            List<CryptographicCapability> capabilities = null) => throw new NYI();

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
    public virtual int UpdateStore(ContainerUpdate containerUpdate) {
        int count = 0;
        if (DictionaryStores.TryGetValue(containerUpdate.Container, out var syncStore)) {
            var store = syncStore.Store;
            foreach (var entry in containerUpdate.Envelopes) {
                if (entry.Index == 0) {
                    throw new NYI();
                    }

                count++;
                store.AppendDirect(entry, true);
                }


            // need to set the store end frame!!!

            return count;
            }

        else {
            // we have zero envelopes being returned in this update.

            Store.Append(StoresDirectory, this, containerUpdate.Envelopes, containerUpdate.Container);
            return containerUpdate.Envelopes.Count;
            }

        }

    /// <summary>
    /// Return a <see cref="Store"/> instance for the store named <paramref name="name"/>. If the
    /// parameter <paramref name="blind"/> is true, only the sequence header values are read.
    /// </summary>
    /// <param name="name">The store to open.</param>
    /// <param name="blind">If true, only the sequence header values are read</param>
    /// <param name="decrypt">If true, decrypt the store contents on access.</param>
    /// <param name="create">Create store if it does not exist</param>
    /// <returns>The <see cref="Store"/> instance.</returns>
    public Store GetStore(string name, 
                bool blind = false, 
                bool decrypt = true,
                bool create = true) {
        if (DictionaryStores.TryGetValue(name, out var syncStore)) {
            if (!blind & (syncStore.Store is CatalogBlind)) {
                // if we have a blind store from a sync operation but need a populated one,
                // remake it.
                syncStore.Store.Dispose();
                syncStore.Store = MakeStore(name, decrypt: decrypt);
                }

            return syncStore.Store;
            }

        //Console.WriteLine($"Open store {name} on {MeshMachine.DirectoryMesh}");

        var store = blind ? new CatalogBlind(StoresDirectory, name) : 
                    MakeStore(name, decrypt: decrypt, create: create);
        syncStore = new SyncStatus(store);

        DictionaryStores.Add(name, syncStore);
        return syncStore.Store;
        }

    /// <summary>
    /// Create a new instance bound to the specified core within this account context.
    /// </summary>
    /// <param name="name">The name of the store to bind.</param>
    /// <param name="darePolicy">Policy to be applied to the store.</param>
    /// <returns>The store instance.</returns>
    /// <param name="decrypt">If true, attempt decryption of payload contents.</param>
    /// <param name="create">Create store if it does not exist</param>
    protected Store MakeStore(string name, 
                DarePolicy? darePolicy = null,
                bool decrypt=true,
                bool create=true) {

        //// special case this for now
        //switch (name) {
        //    case CatalogAccess.Label : return new CatalogAccess(StoresDirectory,
        //            name, ContainerCryptoParameters, KeyCollection, meshClient: (this as IMeshClient));
        //    }

        if (DictionaryCatalogDelegates.TryGetValue(name, out var factory)) {
            darePolicy ??= ActivationCommon.GetDarePolicy(name);
            return factory(StoresDirectory, name, this, darePolicy, null, this, 
                decrypt: decrypt, create: create);
            }
        if (DictionarySpoolDelegates.TryGetValue(name, out factory)) {
            return factory(StoresDirectory, name, this, null, null, this, 
                decrypt: decrypt, create: create);
            }


        throw new NYI();
        }

    /// <summary>
    /// Force generation of all stores.
    /// </summary>
    protected void LoadStores() {
        foreach (var entry in DictionaryCatalogDelegates) {
            GetStore(entry.Key, false);
            }
        }

    #endregion
    #region Implement IKeyLocate


    /// <summary>
    /// Resolve a public encryption key by identifier. This may be a UDF fingerprint of the key,
    /// an account identifier or strong account identifier.
    /// </summary>
    /// <param name="keyId">The identifier to resolve.</param>
    /// <param name="cryptoKey">The found key </param>
    /// <returns>The identifier.</returns>
    public virtual bool TryFindKeyEncryption(string keyId, out CryptoKey cryptoKey) =>
                KeyCollection.TryFindKeyEncryption(keyId, out cryptoKey);

    /// <summary>
    /// Resolve a public encryption key by identifier. This may be a UDF fingerprint of the key,
    /// an account identifier or strong account identifier.
    /// </summary>
    /// <param name="keyId">The identifier to resolve.</param>
    /// <param name="cryptoKey">The found key </param>
    /// <returns>The identifier.</returns>
    public virtual bool TryFindPublicKey(string keyId, out CryptoKey cryptoKey) =>
                KeyCollection.TryFindPublicKey(keyId, out cryptoKey);


    /// <summary>
    /// Attempt to obtain a private key with identifier <paramref name="keyId"/>.
    /// </summary>
    /// <param name="keyId">The key identifier to match.</param>
    /// <param name="cryptoKey">The found key </param>
    /// <returns>The key pair if found.</returns>
    public virtual bool LocatePrivateKeyPair(string keyId, out CryptoKey cryptoKey) =>
                KeyCollection.LocatePrivateKeyPair(keyId, out cryptoKey);

    /// <summary>
    /// Attempt to obtain a recipient with identifier <paramref name="keyId"/>.
    /// </summary>
    /// <param name="keyId">The key identifier to match.</param>
    /// <param name="cryptoKey">The found key </param>
    /// <returns>The key pair if found.</returns>
    public virtual bool TryFindKeyDecryption(string keyId, out IKeyDecrypt cryptoKey) =>
                KeyCollection.TryFindKeyDecryption(keyId, out cryptoKey);

    /// <summary>
    /// Resolve a private key by identifier. This may be a UDF fingerprint of the key,
    /// an account identifier or strong account identifier.
    /// </summary>
    /// <param name="signingKey">The identifier to resolve.</param>
    /// <param name="cryptoKey">The found key </param>
    /// <returns>The identifier.</returns>
    public virtual bool TryFindKeySignature(string signingKey, out CryptoKey cryptoKey) =>
                KeyCollection.TryFindKeySignature(signingKey, out cryptoKey);

    /// <summary>
    /// Add a keypair to the collection.
    /// </summary>
    /// <param name="keyPair">The key pair to add.</param>
    public void Add(KeyPair keyPair) => KeyCollection.Add(keyPair);

    /// <summary>
    /// Persist a private key if permitted by the KeySecurity model of the key.
    /// </summary>
    /// <param name="keyPair">The key to persist.</param>
    public void Persist(KeyPair keyPair) => KeyCollection.Persist(keyPair);

    /// <summary>
    /// Resolve a public signature key by identifier. This may be a UDF fingerprint of the key,
    /// an account identifier or strong account identifier.
    /// </summary>
    /// <param name="cryptoKey">The key to validate.</param>
    /// <returns>The identifier.</returns>
    public virtual bool ValidateTrustAnchor(CryptoKey cryptoKey) => throw new NYI();


    ///<inheritdoc cref="IKeyLocate.RemoteAgreement"/>
    public KeyAgreementResult RemoteAgreement(
                string serviceAddress,
                KeyPairAdvanced ephemeral,
                string shareId) {

        var operation = new CryptographicOperationKeyAgreement() {
            KeyId = shareId,
            PublicKey = Key.GetPublic(ephemeral)
            };

        var operateRequest = new OperateRequest() {
            AccountAddress = serviceAddress,
            Operations = new List<CryptographicOperation>() {
                    operation
                    }
            };
        ;
        var response = MeshClient.Operate(operateRequest);
        response.AssertSuccess(CryptographicOperationRefused.Throw);

        var result = response.Results[0] as CryptographicResultKeyAgreement;

        return result.KeyAgreement.KeyAgreementResult;


        }


    #endregion
    #region Implement IDare

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

        KeyPair signingKey = sign ? KeyCommonSignature : null;
        List<CryptoKey> encryptionKeys;

        // probably going to fail here unless we have a way to pull keys out of the contacts catalog 
        // for the group.
        if (recipients != null) {
            encryptionKeys = new List<CryptoKey>();
            foreach (var recipient in recipients) {
                TryFindKeyEncryption(recipient, out var key);
                encryptionKeys.Add(key);
                }
            }

        var cryptoParameters = new CryptoParameters(keyCollection: this,
                    signer: signingKey, recipients: recipients);
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
                bool verify = false) {
        verify.Future();
        return envelope.GetPlaintext(this);
        }

    /// <summary>
    /// Attempt to form a trust path for the key used to sign <paramref name="dareSignature"/>.
    /// </summary>
    /// <param name="dareSignature">The signature to validate.</param>
    /// <param name="anchor">If present specifies the fingerprint of a key that MUST anchor
    /// the trust path.</param>
    /// <returns>The result of the trust path analysis.</returns>
    public TrustResult ValidateTrustPath(DareSignature dareSignature, string anchor = null) => throw new NotImplementedException();


    /// <summary>
    /// Attempt to erase the private key with fingerprint <paramref name="udf"/> from the
    /// associated persistence store.
    /// </summary>
    /// <param name="udf"></param>
    /// <returns>True if the key was found, otherwise false.</returns>
    public void ErasePrivateKey(string udf) => throw new NotImplementedException();

    /// <summary>
    /// Locate the private key with fingerprint <paramref name="udf"/> and return
    /// the corresponding JSON description.
    /// </summary>
    /// <param name="udf">Key to locate</param>
    /// <returns>The JSON description (if found).</returns>
    public IJson LocatePrivateKey(string udf) => throw new NotImplementedException();

    /// <summary>
    /// Persist the key pair specified by <paramref name="privateKey"/> and mark as exportable
    /// or non-exportable according to the value of <paramref name="Exportable"/>.
    /// </summary>
    /// <param name="udf">The UDF of the key</param>
    /// <param name="privateKey">The private key parameters.</param>
    /// <param name="Exportable">If true, the key is exportable.</param>
    public void Persist(string udf, Cryptography.PKIX.IPKIXPrivateKey privateKey, bool Exportable) => throw new NotImplementedException();

    /// <summary>
    /// Persist the key pair specified by <paramref name="joseKey"/> and mark as exportable
    /// or non-exportable according to the value of <paramref name="exportable"/>.
    /// </summary>
    /// <param name="udf">The UDF of the key</param>
    /// <param name="joseKey">The private key parameters.</param>
    /// <param name="exportable">If true, the key is exportable.</param>
    public void Persist(string udf, IJson joseKey, bool exportable) => throw new NotImplementedException();

    #endregion


    }
