﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Goedel.Cryptography.Dare {
    public partial class DARESignature {

        /// <summary>
        /// Default constructor for use in deserialization.
        /// </summary>
        public DARESignature() {
            }

        /// <summary>
        /// Sign the digest value <paramref name="DigestValue"/> with 
        /// <paramref name="SignerKey"/> and create a DARESignature
        /// instance with the resulting values.
        /// </summary>
        /// <param name="SignerKey">The signature key.</param>
        /// <param name="DigestValue">The digest value.</param>
        /// <param name="DigestId">The digest algorithm used to calculate 
        /// <paramref name="DigestValue"/>.</param>
        /// <param name="KeyDerive">Key derivation function used to calculate a signature witness 
        /// value (if required).</param>
        public DARESignature(KeyPair SignerKey, byte[] DigestValue,
                    CryptoAlgorithmID DigestId, KeyDerive KeyDerive=null) {
            SignatureValue = SignerKey.SignHash(DigestValue, DigestId);

            if (KeyDerive != null) {
                WitnessValue = KeyDerive.Derive(SignatureValue);
                }


            }
        }


    public partial class DARESigner {

        /// <summary>
        /// Default constructor for use in deserialization.
        /// </summary>
        public DARESigner() {
            }


        /// <summary>
        /// Create a DARESigner instance for the specified public key information.
        /// </summary>
        /// <param name="SignerKey">The verification key parameters.</param>
        /// <param name="DigestId">The digest identifier used to create the 
        /// signature value.</param>
        public DARESigner(KeyPair SignerKey, CryptoAlgorithmID DigestId) {

            var Alg = SignerKey.SignatureAlgorithmID(DigestId);

            }
        }


    }
