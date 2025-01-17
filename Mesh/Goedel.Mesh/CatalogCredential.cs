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


using System.Text;

namespace Goedel.Mesh;


#region // The data classes CatalogCredential, CatalogedCredential
/// <summary>
/// Device catalog. Describes the properties of all devices connected to the user's Mesh account.
/// </summary>

public class CatalogCredential : Catalog<CatalogedCredential> {
    #region // Properties
    ///<summary>The canonical label for the catalog</summary>

    public const string Label = MeshConstants.MMM_Credential;

    ///<summary>The catalog label</summary>
    public override string ContainerDefault => Label;


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
    /// <param name="meshClient">Means of obtaining a Mesh Client.</param>
    public static new Store Factory(
            string directory,
                string? storeId,
                IMeshClient? meshClient = null,
                DarePolicy? policy = null,
                CryptoParameters? cryptoParameters = null,
                IKeyCollection? keyCollection = null,
                bool decrypt = true,
                bool create = true) {
        meshClient?.Future();
        return new CatalogCredential(directory, storeId, policy, cryptoParameters, keyCollection, decrypt, create);
        }

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
    public CatalogCredential(
                string directory,
                string? storeName = null,
                DarePolicy? policy = null,
                CryptoParameters? cryptoParameters = null,
                IKeyCollection? keyCollection = null,
                bool decrypt = true,
                bool create = true) :
        base(directory, storeName ?? Label,
                    policy, cryptoParameters, keyCollection, decrypt: decrypt, create: create) {
        }

    #endregion
    #region // Class methods



    //void UpdateLocal(CatalogedEntry catalogedEntry) {
    //    }

    /// <summary>
    /// Locate credential matching the specified service name, ignoring the protocol value.
    /// </summary>
    /// <param name="key">The service to be matched.</param>
    /// <returns>If a match is found, returns the matching entry, otherwise null.</returns>
    public CatalogedCredential? GetCredentialByService(string key) {
        foreach (var credential in AsCatalogedType) {
            if (credential.Service == key) {
                return credential;
                }
            }
        return null;
        }





    #endregion
    }


public partial class CatalogedCredential {
    #region // Properties

    ///<summary>The primary key is protocol:site </summary>
    public override string _PrimaryKey => GetKey(Protocol, Service);

    #endregion
    #region // Override methods

    /// <summary>
    /// Return a key for the credential to be used to connect to <paramref name="service"/>
    /// via <paramref name="protocol"/>.
    /// </summary>
    /// <param name="protocol">The protocol to use to make the connection.</param>
    /// <param name="service">The service to connect to.</param>
    /// <returns>The credential key.</returns>
    public static string GetKey(string? protocol, string? service) => $"{protocol ?? ""}:{service ?? ""}";



    ///<inheritdoc/>
    public override void Describe(StringBuilder builder, bool detail = false) => ItemToBuilder(builder);


    ///<inheritdoc/>

    public override void ItemToBuilder(StringBuilder builder, int indent = 0, IKeyCollection keyCollection = null) {

        if (Protocol != null) {
            builder.Append($"{Protocol}:");
            }
        builder.AppendLine($"{Username}@{Service} = [{Password}]");
        }


    #endregion
    }
#endregion