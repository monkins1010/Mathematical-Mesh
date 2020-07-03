﻿using Goedel.Cryptography;
using Goedel.Cryptography.Dare;
using Goedel.Utilities;
using Goedel.Cryptography.Jose;
using Goedel.Mesh;

using System;
using System.Collections.Generic;
using System.IO;


namespace Goedel.Mesh {


    public interface ICryptographicClient {

        /// <summary>
        /// Perform a partial key agreement.
        /// </summary>
        /// <param name="keyPair">The key pair to perform the agreement against.</param>
        /// <returns>The key agreement result.</returns>
        public KeyAgreementResult Agreement(string service, KeyPair keyPair);

        }


    public interface ICapabilityPartial {

        /// <summary>
        ///The identifier used to claim the capability from the service.[Only present for
        ///a partial capability.]
        /// </summary>

        string ServiceId { get; set; }
        /// <summary>
        ///The service account that supports a serviced capability. [Only present for
        ///a partial capability.]
        /// </summary>

        string ServiceAddress { get; set; }

        }


    public interface ICapabilityServiced {

        /// <summary>
        ///UDF of trust root under which request to use a serviced capability must be 
        ///authorized. [Only present for a serviced capability]
        /// </summary>

        string AuthenticationId { get; set; }

        }


    public partial class CryptographicCapability  {


        ///<summary>If not null, specifies a key to which key shares MUST be encrypted
        ///when creating.</summary>
        public CryptoKey KeyDataEncryptionKey;

        ///<summary>The primary key is the value of the <see cref="Id"/> property.</summary>
        public override string _PrimaryKey => Id;

        public CryptographicCapability() {

            }

        public CryptographicCapability(KeyPair keyPair) {
            KeyData = new KeyData(keyPair);
            }


        public IKeyAdvancedPrivate GetKeyPairAdvancedPrivate() {
            var keypair = KeyData.GetKeyPair() as KeyPairAdvanced;
            return keypair.IKeyAdvancedPrivate;


            }

        }


    public partial class CapabilitySign : IKeySign {


        /// <summary>
        /// Sign a precomputed digest
        /// </summary>
        /// <param name="data">The data to sign.</param>
        /// <param name="algorithmID">The algorithm to use.</param>
        /// <param name="context">Additional data added to the signature scope
        /// for protocol isolation.</param>
        /// <returns>The signature data</returns>       
        public virtual byte[] SignHash(
            byte[] data,
            CryptoAlgorithmId algorithmID =
            CryptoAlgorithmId.Default,
            byte[] context = null) => throw new OperationNotSupported();
        }

    public partial class CapabilityDecrypt : IKeyDecrypt {




        protected KeyPairAdvanced KeyPair => keyPair ?? null;

        KeyPairAdvanced keyPair;


        /// <summary>
        /// Perform a key exchange to encrypt a bulk or wrapped key under this one.
        /// </summary>
        /// <param name="encryptedKey">The encrypted session</param>
        /// <param name="ephemeral">Ephemeral key input (required for DH)</param>
        /// <param name="algorithmID">The algorithm to use (redundant?)</param>
        /// <param name="partial">Partial key agreement carry in (for recryption)</param>
        /// <param name="salt">Optional salt value for use in key derivation. If specified
        /// must match the salt used to encrypt.</param>        
        /// <returns>The decoded data instance</returns>
        public virtual byte[] Decrypt(
                byte[] encryptedKey,
                KeyPair ephemeral = null,
                CryptoAlgorithmId algorithmID = CryptoAlgorithmId.Default,
                KeyAgreementResult partial = null,
                byte[] salt = null) => KeyPair.Decrypt(encryptedKey, 
                    ephemeral, algorithmID, partial, salt);


        /// <summary>
        /// Perform a key agreement.
        /// </summary>
        /// <param name="keyPair">The key pair to perform the agreement against.</param>
        /// <returns>The key agreement result.</returns>
        public virtual KeyAgreementResult Agreement(KeyPair keyPair) =>
            KeyPair.Agreement(keyPair);

        }
    public partial class CapabilityDecryptPartial :ICapabilityPartial {

        public ICryptographicClient CryptographicClient { protected get; set; }
        
        /// <summary>
        /// Perform a key exchange to encrypt a bulk or wrapped key under this one.
        /// </summary>
        /// <param name="encryptedKey">The encrypted session</param>
        /// <param name="ephemeral">Ephemeral key input (required for DH)</param>
        /// <param name="algorithmID">The algorithm to use (redundant?)</param>
        /// <param name="partial">Partial key agreement carry in (for recryption)</param>
        /// <param name="salt">Optional salt value for use in key derivation. If specified
        /// must match the salt used to encrypt.</param>        
        /// <returns>The decoded data instance</returns>
        public override byte[] Decrypt(
                byte[] encryptedKey,
                KeyPair ephemeral = null,
                CryptoAlgorithmId algorithmID = CryptoAlgorithmId.Default,
                KeyAgreementResult partial = null,
                byte[] salt = null) {

            // delegate service... 
            var partial2 = CryptographicClient.Agreement(ServiceAddress, ephemeral);

            if (partial != null) {
                //partial2 = partial2.Add(partial);
                }

            return KeyPair.Decrypt(encryptedKey, ephemeral, algorithmID, partial2, salt);
            }


        }
    public partial class CapabilityDecryptServiced :ICapabilityServiced {
        }



    public partial class CapabilityKeyGenerate {


        public void CreateShares(params CryptographicCapability[] capabilities) {
            var keyAdvanced = KeyData.GetKeyPair() as KeyPairAdvanced;
            var privateAdvanced = keyAdvanced.IKeyAdvancedPrivate;

            var keys = privateAdvanced.MakeRecryptionKeySet(capabilities.Length);

            for (var i = 0; i < capabilities.Length; i++) {
                var key = keys[i].GetKeyPair(keySecurity: KeySecurity.Exportable);
                capabilities[i].KeyData = new KeyData(key);
                }
            }

        }

    }