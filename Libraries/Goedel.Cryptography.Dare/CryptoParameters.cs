﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

using Goedel.Cryptography.Jose;
using Goedel.Protocol;
using Goedel.Utilities;
namespace Goedel.Cryptography.Dare {


    /// <summary>
    /// Specifies a set of cryptographic parameters to be used to create 
    /// CryptoStacks
    /// </summary>
    public partial class CryptoParameters {

        /// <summary>The key collection to use to resolve names to keys</summary>
        public KeyCollection KeyCollection;
        /// <summary>The set of keys to encrypt to.</summary>
        public List<KeyPair> EncryptionKeys;
        /// <summary>The set of keys to use to sign</summary>
        public List<KeyPair> SignerKeys;


        /// <summary>The authentication algorithm to use</summary>
        public CryptoAlgorithmID DigestID;

        /// <summary>The encryption algorithm to use</summary>
        public CryptoAlgorithmID EncryptID;



        /// <summary>
        /// If true, data is to be encrypted.
        /// </summary>
        public bool Encrypt => EncryptID != CryptoAlgorithmID.NULL;

        /// <summary>
        /// If true, data is to be digested.
        /// </summary>
        public bool Digest => DigestID != CryptoAlgorithmID.NULL;



        void SetEncrypt() => EncryptID = EncryptID == CryptoAlgorithmID.NULL ? CryptoAlgorithmID.Default : EncryptID;
        void SetDigest() => DigestID = DigestID == CryptoAlgorithmID.NULL ? CryptoAlgorithmID.Default : DigestID;

        /// <summary>
        /// If true, data is to be signed.
        /// </summary>
        public bool Sign => SignerKeys != null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected CryptoParameters() {
            }

        /// <summary>
        /// Create a CryptoParameters instance to encode data for the specified recipients and
        /// signers using the specified KeyCollection to resolve the identifiers.
        /// </summary>
        /// <param name="keyCollection">The Key collection to be used to resolve names.</param>
        /// <param name="recipients">The public keys to be used to encrypt.</param>
        /// <param name="signers">The private keys to be used in signing.</param>
        /// <param name="recipient">The public keys to be used to encrypt.</param>
        /// <param name="signer">The private keys to be used in signing.</param>
        /// <param name="encryptID">The cryptographic enhancement to be applied to the
        /// content.</param>
        /// <param name="digestID">The digest algorithm to be applied to the message
        /// encoding.</param>
        public CryptoParameters(
                        KeyCollection keyCollection = null,
                        List<string> recipients = null,
                        List<string> signers = null,
                        KeyPair recipient = null,
                        KeyPair signer = null,
                        CryptoAlgorithmID encryptID = CryptoAlgorithmID.NULL,
                        CryptoAlgorithmID digestID = CryptoAlgorithmID.NULL) {
            this.DigestID = digestID;
            this.EncryptID = encryptID;

            this.KeyCollection = keyCollection;

            if (recipients != null) {
                SetEncrypt();

                EncryptionKeys = new List<KeyPair>();
                foreach (var Entry in recipients) {
                    AddEncrypt(Entry);
                    }
                }
            else if (recipient != null) {
                SetEncrypt();
                EncryptionKeys = new List<KeyPair>() { recipient };
                }
            if (signers != null) {
                SetDigest();

                SignerKeys = new List<KeyPair>();
                foreach (var Entry in signers) {
                    AddSign(Entry);
                    }
                }
            else if (signer != null) {
                SetDigest();
                SignerKeys = new List<KeyPair>() { signer };
                }
            }


        ///// <summary>
        ///// Create a CryptoParameters instance to encode data for the specified recipients and
        ///// signers using the specified KeyCollection to resolve the identifiers.
        ///// </summary>
        ///// <param name="keyCollection">The Key collection to be used to resolve names.</param>
        ///// <param name="recipient">The public keys to be used to encrypt.</param>
        ///// <param name="signer">The private keys to be used in signing.</param>
        ///// <param name="encryptID">The cryptographic enhancement to be applied to the
        ///// content.</param>
        ///// <param name="digestID">The digest algorithm to be applied to the message
        ///// encoding.</param>
        //public CryptoParameters(
        //    KeyCollection keyCollection=null,

        //    CryptoAlgorithmID encryptID = CryptoAlgorithmID.NULL,
        //    CryptoAlgorithmID digestID = CryptoAlgorithmID.NULL) {

        //    KeyCollection = keyCollection;

        //    this.DigestID = digestID;
        //    this.EncryptID = encryptID;



        //    }






        /// <summary>
        /// Add a recipient entry.
        /// </summary>
        /// <param name="AccountId">Identifier of the key to add.</param>
        protected virtual void AddEncrypt(string AccountId) {
            EncryptionKeys = EncryptionKeys ?? new List<KeyPair>();
            EncryptionKeys.Add(KeyCollection.GetByAccountEncrypt(AccountId));
            }

        /// <summary>
        /// Add a signer entry.
        /// </summary>
        /// <param name="AccountId">Identifier of the key to add.</param>
        protected virtual void AddSign(string AccountId) {
            SignerKeys = SignerKeys ?? new List<KeyPair>();
            SignerKeys.Add(KeyCollection.GetByAccountSign(AccountId));
            }


        /// <summary>
        /// Generate a new CryptoStack for this parameter set.
        /// </summary>
        /// <returns>The created CryptoStack.</returns>
        public CryptoStack GetCryptoStack() => new CryptoStack(this);


        }


    }
