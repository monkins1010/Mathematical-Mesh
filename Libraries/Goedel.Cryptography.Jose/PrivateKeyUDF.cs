﻿namespace Goedel.Cryptography.Jose {

    /// <summary>
    /// Interface providing key activation functions. This interface is supported
    /// by PrivateKeyUDF allowing keys to be derived from a pair of seeds. Devices
    /// whose keys are bound to an HSM should provide a class exposing the IActivate
    /// interface that exposes the necessary functionality for key location,
    /// activation and use.
    /// </summary>
    public interface IActivate {

        ///<summary>The encryption algorithm identifier</summary>
        CryptoAlgorithmId AlgorithmEncryptID { get; }

        ///<summary>The signature algorithm identifier</summary>
        CryptoAlgorithmId AlgorithmSignID { get; }

        ///<summary>The authentication algorithm identifier</summary>
        CryptoAlgorithmId AlgorithmAuthenticateID { get; }


        /// <summary>
        /// Generate a composite private key by generating private keys by means
        /// of the activation seed <paramref name="activationSeed"/> and the
        /// class instance generator.
        /// </summary>
        /// <param name="activationSeed">The activation seed value.</param>
        /// <param name="keyCollection">The key collection to register the private key to</param>
        /// <param name="keyUses">The permitted key uses.</param>
        /// <param name="saltSuffix">The salt suffix for use in key derrivation.</param>
        /// <param name="cryptoAlgorithmID">The cryptographic algorithm.</param>
        /// <returns>The generated ephemeral key.</returns>
        KeyPair ActivatePrivate(
            string activationSeed,
            IKeyLocate keyCollection,
            KeyUses keyUses, string saltSuffix,
            CryptoAlgorithmId cryptoAlgorithmID);

        }

    public partial class PrivateKeyUDF: IActivate {


        ///<summary>The encryption algorithm identifier</summary>
        public CryptoAlgorithmId AlgorithmEncryptID =>
            AlgorithmEncrypt != null ? AlgorithmEncrypt.FromJoseID() : CryptoAlgorithmId.Default;

        ///<summary>The signature algorithm identifier</summary>
        public CryptoAlgorithmId AlgorithmSignID =>
            AlgorithmSign != null ? AlgorithmSign.FromJoseID() : CryptoAlgorithmId.Default;

        ///<summary>The authentication algorithm identifier</summary>
        public CryptoAlgorithmId AlgorithmAuthenticateID =>
            AlgorithmAuthenticate != null ? AlgorithmAuthenticate.FromJoseID() : CryptoAlgorithmId.Default;


        /// <summary>
        /// Basic constructor for deserialization
        /// </summary>
        public PrivateKeyUDF() {
            }

        ///// <summary>
        ///// Constructor generating a new instance with a private key derived from the
        ///// seed <paramref name="secret"/> if not null or a randomly generated key
        ///// of <paramref name="bits"/> bits otherwise.
        ///// </summary>
        ///// <param name="udfAlgorithmIdentifier">The type of master secret.</param>
        ///// <param name="algorithmEncrypt">The encryption algorithm.</param>
        ///// <param name="algorithmSign">The signature algorithm</param>
        ///// <param name="algorithmAuthenticate">The signature algorithm</param>
        ///// <param name="secret">The master secret.</param>
        ///// <param name="bits">The size of key to generate in bits/</param>
        //public PrivateKeyUDF(
        //        UdfAlgorithmIdentifier udfAlgorithmIdentifier,
        //        CryptoAlgorithmId algorithmEncrypt = CryptoAlgorithmId.Default,
        //        CryptoAlgorithmId algorithmSign = CryptoAlgorithmId.Default,
        //        CryptoAlgorithmId algorithmAuthenticate = CryptoAlgorithmId.Default,
        //        byte[] secret = null,
        //        int bits = 256) : this(
        //            udfAlgorithmIdentifier,
        //            UDF.DerivedKey(udfAlgorithmIdentifier, data: secret ?? Platform.GetRandomBits(bits)), 
        //                        algorithmEncrypt, algorithmSign, algorithmAuthenticate
        //            ) {
        //    }



        /// <summary>
        /// Constructor generating a new instance with a private key derrived from the
        /// seed  <paramref name="udf"/>.
        /// </summary>
        /// <param name="udfAlgorithmIdentifier">The type of master secret.</param>
        /// <param name="udf">The master secret as a UDF string.</param>
        /// <param name="secret">The master secret as a byte array (ignored 
        /// if <paramref name="udf"/> is not null).</param>
        /// <param name="algorithmEncrypt">The encryption algorithm.</param>
        /// <param name="algorithmSign">The signature algorithm</param>
        /// <param name="algorithmAuthenticate">The signature algorithm</param>
        /// <param name="bits">The size of key to generate in bits/</param>
        public PrivateKeyUDF(
                    UdfAlgorithmIdentifier udfAlgorithmIdentifier,
                string udf = null,
                byte[] secret = null,
                CryptoAlgorithmId algorithmEncrypt = CryptoAlgorithmId.Default,
                CryptoAlgorithmId algorithmSign = CryptoAlgorithmId.Default,
                CryptoAlgorithmId algorithmAuthenticate = CryptoAlgorithmId.Default,
                int bits = 256) {

            PrivateValue = udf ?? UDF.DerivedKey(udfAlgorithmIdentifier, 
                data: secret ?? Platform.GetRandomBits(bits));
            KeyType = udfAlgorithmIdentifier.ToString();

            AlgorithmSign = algorithmSign.ToJoseID();
            AlgorithmEncrypt = algorithmEncrypt.ToJoseID();
            AlgorithmAuthenticate = algorithmAuthenticate.ToJoseID();
            }


        /// <summary>
        /// Generate a composite private key by generating private keys by means
        /// of the activation seed <paramref name="activationSeed"/> and the
        /// class instance generator.
        /// </summary>
        /// <param name="activationSeed">The activation seed value.</param>
        /// <param name="keyCollection">The key collection to register the private key to</param>
        /// <param name="keyUses">The permitted key uses.</param>
        /// <param name="saltSuffix">The salt suffix for use in key derrivation.</param>
        /// <param name="cryptoAlgorithmID">The cryptographic algorithm.</param>
        /// <returns>The generated ephemeral key.</returns>
        public KeyPair ActivatePrivate(
                    string activationSeed,
                    IKeyLocate keyCollection, 
                    KeyUses keyUses, string saltSuffix, 
                    CryptoAlgorithmId cryptoAlgorithmID) {
            var baseKey = UDF.DeriveKey(PrivateValue, keyCollection,
                    KeySecurity.Ephemeral, keyUses: keyUses, cryptoAlgorithmID, saltSuffix) as KeyPairAdvanced;

            //Console.WriteLine($"Private: Base-{baseKey.UDF} Seed-{activationSeed} Type-{meshKeyType}");

            var activationKey = UDF.DeriveKey(activationSeed, keyCollection,
                    KeySecurity.Ephemeral, keyUses: keyUses, cryptoAlgorithmID, saltSuffix) as KeyPairAdvanced;



            var combinedKey = activationKey.Combine(baseKey, keyUses: keyUses);

            //Console.WriteLine($"   result {combinedKey}");

            return combinedKey;
            }


        }



    }
