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


namespace Goedel.Mesh;


#region // The data classes CatalogMember, CatalogedMember
/// <summary>
/// Capability catalog. Contains keys used to perform capabilities.
/// </summary>
public class CatalogAccess : Catalog<CatalogedAccess> {
    #region // Properties
    ///<summary>The canonical label for the catalog</summary>
    public const string Label = MeshConstants.MMM_Access;

    ///<summary>The catalog label</summary>
    public override string ContainerDefault => Label;


    ///<summary>Dictionary for locating capabilities for use.</summary>
    public Dictionary<string, CapabilityDecrypt> DictionaryDecryptByKeyId =
            new();

    ///<summary>Dictionary for locating capabilities for use.</summary>
    public Dictionary<string, CapabilitySign> DictionarySignByAccountAddress =
            new();

    ///<summary>Dictionary for locating capabilities for use.</summary>
    public Dictionary<string, CapabilityKeyGenerate> DictionaryKeyGenerate =
            new();

    ///<summary>Dictionary for locating capabilities for use.</summary>
    public Dictionary<string, CapabilityFairExchange> DictionaryFairExchange =
            new();

    #endregion
    #region // Factory methods and constructors


    /// <summary>
    /// Factory delegate
    /// </summary>
    /// <param name="directory">Directory of store file on local machine.</param>
    /// <param name="storeId">Store identifier.</param>
    /// <param name="cryptoParameters">Cryptographic parameters for the store.</param>
    /// <param name="policy">The cryptographic policy to be applied to the catalog.</param>
    /// <param name="keyCollection">Key collection to be used to resolve keys</param>
    /// <param name="decrypt">If true, attempt decryption of payload contents./</param>
    /// <param name="create">If true, create a new file if none exists.</param>
    /// <param name="meshClient">Parent account context used to obtain a mesh client.</param>
    public static new Store Factory(
            string directory,
                string storeId,
                IMeshClient meshClient,
                DarePolicy? policy = null,
                CryptoParameters? cryptoParameters = null,
                IKeyCollection? keyCollection = null,
                bool decrypt = true,
                bool create = true) =>
        new CatalogAccess(directory, storeId, policy, cryptoParameters, keyCollection, meshClient, decrypt: decrypt, create: create);


    /// <summary>
    /// Constructor for a catalog named <paramref name="storeName"/> in directory
    /// <paramref name="directory"/> using the cryptographic parameters <paramref name="cryptoParameters"/>
    /// and key collection <paramref name="keyCollection"/>.
    /// </summary>
    /// <param name="create">Create a new persistence store on disk if it does not already exist.</param>
    /// <param name="decrypt">Attempt to decrypt the contents of the catalog if encrypted.</param>
    /// <param name="directory">The directory in which the catalog persistence container is stored.</param>
    /// <param name="storeName">The catalog persistence container file name.</param>
    /// <param name="cryptoParameters">The default cryptographic enhancements to be applied to container entries.</param>
    /// <param name="policy">The cryptographic policy to be applied to the container.</param>
    /// <param name="keyCollection">The key collection to be used to resolve keys when reading entries.</param>
    /// <param name="meshClient">Parent account context used to obtain a mesh client.</param>
    public CatalogAccess(
                string directory,
                string? storeName = null,
                DarePolicy? policy = null,
                CryptoParameters? cryptoParameters = null,
                IKeyCollection? keyCollection = null,
                IMeshClient? meshClient = null,
                bool decrypt = true,
                bool create = true) :
                base(directory, storeName ?? Label,
                    policy, cryptoParameters, keyCollection, meshClient: meshClient, decrypt: decrypt, create: create) {
        //Screen.WriteLine($"*** Create Access {countStores++}");
        // Hack: likely to have issues here because the CatalogAccess needs to be readable by the service
        // Should treat this like any other account except that the service is granted access when it
        // connects.


        }
    #endregion
    #region // Class methods

    /// <summary>
    /// Resolve a decryption capability corresponding to the key <paramref name="keyId"/>.
    /// </summary>
    /// <param name="keyId">The identifier of the public key to obtain a decryption 
    /// capability against.</param>
    /// <param name="keyDecrypt">The decryption key if found, otherwise null.</param>
    /// <returns>true if a key is found, otherwise false.</returns>
    public bool TryFindKeyDecryption(string keyId, out IKeyDecrypt keyDecrypt) {
        var found = DictionaryDecryptByKeyId.TryGetValue(keyId, out var key);
        keyDecrypt = key;
        return found;
        }

    /// <summary>
    /// Resolve a signing capability for the key <paramref name="keyId"/>.
    /// </summary>
    /// <param name="keyId">The identifier of the public key to obtain a signature 
    /// capability for.</param>
    /// <returns>The signing capability if found, otherwise null.</returns>
    public IKeySign TryFindKeySign(string keyId) {
        DictionarySignByAccountAddress.TryGetValue(keyId, out var result);

        return result;
        }

    /// <summary>
    /// Resolve a key share generation capability for the key <paramref name="keyId"/>
    /// </summary>
    /// <param name="keyId">The identifier of the public key to obtain a key share 
    /// generation capability for.</param>
    /// <returns>The key share generation capability if found, otherwise null.</returns>
    public CapabilityKeyGenerate TryFindKeyGenerate(string keyId) {
        DictionaryKeyGenerate.TryGetValue(keyId, out var result);

        return result;
        }


    /// <summary>
    /// Add <paramref name="capability"/> to the catalog.
    /// </summary>
    /// <param name="capability">Capability to add.</param>

    public void Add(
                Capability capability
                ) {
        var catalogedCapability = new CatalogedAccess(capability);
        New(catalogedCapability);
        }

    /// <summary>
    /// Callback called before adding a new entry to the catalog. Overriden to update the values
    /// in the dictionaries serving key discovery.
    /// </summary>
    /// <param name="catalogedEntry">The entry being added.</param>
    public override void NewEntry(CatalogedAccess catalogedEntry) => UpdateLocal(catalogedEntry);


    ///<inheritdoc/>
    public override void UpdateEntry(CatalogedAccess catalogedEntry) => UpdateLocal(catalogedEntry);

    ///<inheritdoc/>
    public override void UpdateLocal(CatalogedEntry catalogedEntry) {

        base.UpdateLocal(catalogedEntry);

        var catalogedCapability = catalogedEntry as CatalogedAccess;
        switch (catalogedCapability?.Capability) {
            case CapabilityDecrypt capabilityDecryption: {
                    DictionaryDecryptByKeyId.Add(capabilityDecryption.Id,
                        capabilityDecryption);

                    if (capabilityDecryption is ICapabilityPartial meshClientCapability) {
                        meshClientCapability.KeyCollection = KeyCollection;
                        }

                    break;
                    }
            case CapabilitySign capabilityAdministrator: {
                    DictionarySignByAccountAddress.Add(capabilityAdministrator.SubjectAddress,
                        capabilityAdministrator);
                    break;
                    }
            case CapabilityKeyGenerate capabilityKeyGenerate: {
                    DictionaryKeyGenerate.Add(capabilityKeyGenerate.SubjectId,
                        capabilityKeyGenerate);
                    break;
                    }
            case CapabilityFairExchange capabilityFairExchange: {
                    DictionaryFairExchange.Add(capabilityFairExchange.SubjectAddress,
                        capabilityFairExchange);
                    break;
                    }
            }
        }

    /// <summary>
    /// Create an activation entry
    /// </summary>
    /// <param name="right">The right under which the activation is granted.</param>
    /// <param name="keyPair">The key to grant.</param>
    /// <param name="keyIdentifier">The key identifier.</param>
    /// <param name="transactContextAccount">The transaction context to add threshold key
    /// creations to.</param>
    /// <returns>The entry created.</returns>
    public ActivationEntry MakeActivation(
                            Right right,
                KeyPairAdvanced keyPair, string keyIdentifier,
                ITransactContextAccount transactContextAccount = null) => new() {
                    Resource = right.Name,
                    Key = MakeKeyData(right, keyPair, keyIdentifier, transactContextAccount)
                    };

    /// <summary>
    /// Make a key data entry granting the key <paramref name="keyPair"/> according to
    /// the right <paramref name="right"/>. If a threshold share is created, create
    /// a capability to be claimed under the authentication key
    /// <paramref name="keyIdentifier"/> and append to the access catalog of
    /// <paramref name="transactContextAccount"/>.
    /// </summary>
    /// <param name="right"></param>
    /// <param name="keyPair"></param>
    /// <param name="keyIdentifier"></param>
    /// <param name="transactContextAccount"></param>
    /// <returns></returns>
    public KeyData MakeKeyData(
                Right right,
                KeyPairAdvanced keyPair,
                string keyIdentifier, ITransactContextAccount transactContextAccount = null) {
        switch (right.Degree) {
            case Degree.Direct: {
                    return new KeyData(keyPair, true);
                    }
            case Degree.Service: {
                    transactContextAccount.AssertNotNull(NYI.Throw);
                    var (keyData, capabilityDecryptServiced) = MakeShare(keyPair,
                        transactContextAccount.AccountId,
                        transactContextAccount.HostEncryptAccount,
                        keyIdentifier);
                    var catalogedCapability = new CatalogedAccess(capabilityDecryptServiced);
                    transactContextAccount.CatalogUpdate(this, catalogedCapability);
                    return keyData;
                    }
            default: {
                    throw new NYI();
                    }
            }
        }

    /// <summary>
    /// Make a KeyShare / capabilityService pair for the key <paramref name="key"/> and
    /// label with the service identifier <paramref name="serviceId"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="serviceId"></param>
    /// <param name="serviceEncrypt">The service encryption key.</param>
    /// <param name="granteeUdf">If not null, </param>
    /// <param name="granteeAccount"></param>

    public static (KeyData, CapabilityDecryptServiced) MakeShare(
                KeyPairAdvanced key, string serviceId,
                KeyPair serviceEncrypt, string granteeUdf, string granteeAccount = null) {

        var shares = key.IKeyAdvancedPrivate.MakeThresholdKeySet(2);
        var deviceKey = shares[0].GetKeyPair(KeySecurity.Exportable, KeyUses.Encrypt);
        var remoteKey = shares[1].GetKeyPair(KeySecurity.Exportable, KeyUses.Encrypt);

        var deviceKeyShare = new KeyShare() {
            PublicPrimary = Key.GetPublic(key),
            Share = Key.GetPrivate(deviceKey),
            ServiceAddress = serviceId,
            };

        var deviceKeyData = new KeyData() {
            PrivateParameters = deviceKeyShare,
            Udf = key.KeyIdentifier
            };


        var remoteKeyShare = new KeyShare() {
            PublicPrimary = Key.GetPublic(key),
            Share = Key.GetPrivate(remoteKey),
            ServiceAddress = serviceId,
            };

        var remoteKeyData = new KeyData() {
            PrivateParameters = remoteKeyShare,
            Udf = key.KeyIdentifier
            };

        remoteKeyData.Envelope(encryptionKey: serviceEncrypt);


        var capabilityDecrypt = new CapabilityDecryptServiced() {
            Active = true,
            Id = deviceKey.KeyIdentifier,
            GranteeUdf = granteeUdf,
            GranteeAccount = granteeAccount,
            EnvelopedKeyShare = remoteKeyData.GetEnvelopedKeyData()
            };

        return (deviceKeyData, capabilityDecrypt);
        }



    #endregion
    }

// NYI should all be DareMessages to allow them to be signed.
public partial class CatalogedAccess {
    #region // Properties
    /// <summary>
    /// The primary key used to catalog the entry, this is the identifier of the key.
    /// </summary>
    public override string _PrimaryKey => Capability._PrimaryKey;
    #endregion
    #region // Factory methods and constructors
    /// <summary>
    /// Default constructor for serialization.
    /// </summary>
    public CatalogedAccess() { }

    /// <summary>
    /// Create a cataloged capability for <paramref name="capability"/>.
    /// </summary>
    public CatalogedAccess(Capability capability) => Capability = capability;
    #endregion
    }
#endregion
