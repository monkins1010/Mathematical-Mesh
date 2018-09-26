﻿//   Copyright © 2015 by Comodo Group Inc.
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
//  
//  
using System;
using System.IO;
using System.Collections.Generic;
using Goedel.Utilities;

namespace Goedel.Cryptography {

    ///// <summary>
    ///// Describes the mechanism that created a cryptographic output.
    ///// </summary>
    //public enum CryptoOperation {
    //    /// <summary>
    //    /// Unknown.
    //    /// </summary>
    //    Unknown,

    //    /// <summary>
    //    /// Data is plaintext source.
    //    /// </summary>
    //    Plaintext,

    //    /// <summary>
    //    /// Cryptographic Digest.
    //    /// </summary>
    //    Digest,

    //    /// <summary>
    //    /// Message Authentication Code,
    //    /// </summary>
    //    Authenticate,

    //    /// <summary>
    //    /// Public key signature.
    //    /// </summary>
    //    Sign,

    //    /// <summary>
    //    /// Signature or Authentication Code verification.
    //    /// </summary>
    //    Verify,

    //    /// <summary>
    //    /// Encryption.
    //    /// </summary>
    //    Encrypt,

    //    /// <summary>
    //    /// Authenticated Encryption.
    //    /// </summary>
    //    AuthenticatedEncrypt,

    //    /// <summary>
    //    /// Decryption or decryption with authentication.
    //    /// </summary>
    //    Decrypt,

    //    /// <summary>
    //    /// Symmetric key wrap.
    //    /// </summary>
    //    WrapKey,

    //    /// <summary>
    //    /// Symmetric key unwrap.
    //    /// </summary>
    //    UnwrapKey,

    //    /// <summary>
    //    /// Derive key.
    //    /// </summary>
    //    DeriveKey,

    //    /// <summary>
    //    /// Derive bits not to be used as a key
    //    /// </summary>
    //    DeriveBits

    //    }



    /// <summary>
    /// Container for data generated by a cryptographic operation
    /// </summary>
    public class CryptoData {

        // Input and output streams

        /// <summary>
        /// The Input stream
        /// </summary>
        public Stream InputStream { get; set; }

        /// <summary>
        /// The Output stream
        /// </summary>
        public Stream OutputStream { get; set; }

        // Cryptographic parameters for final and intermediate results

        /// <summary>Initialization vector for block modes that require one.</summary>
        public byte[] IV {
            get; set;
            }

        /// <summary>The encryption key</summary>
        public byte[] Key {
            get; set;
            }

        /// <summary>
        /// The output data value as a byte array. This is only available if the 
        /// OutputStream is a MemoryStream.
        /// </summary>
        public byte[] OutputData => (OutputStream as MemoryStream).ToArray();  

        /// <summary>
        /// The Algorithm identifier used to construct this instance.
        /// This may contain entries for CryptoAlgorithmID.Default
        /// as the bulk or meta algorithm.
        /// </summary>
        public CryptoAlgorithmID BaseAlgorithmIdentifier { get; } = 
            CryptoAlgorithmID.Default;

        /// <summary>
        /// The Algorithm identifier that was inferred from the 
        /// BaseAlgorithmIdentifier
        /// </summary>
        public CryptoAlgorithmID AlgorithmIdentifier { get; set; } = 
            CryptoAlgorithmID.Unknown;

        /// <summary>Return the meta algorithm identifier (for debugging)</summary>
        public CryptoAlgorithmID MetaID => AlgorithmIdentifier.Meta(); 

        /// <summary>Return the bulk algorithm identifier (for debugging)</summary>
        public virtual CryptoAlgorithmID BulkID  => AlgorithmIdentifier.Bulk(); 


        ///// <summary>List of signature blobs</summary>
        //public List<CryptoDataSignature> Signatures;

        ///// <summary>List of key exchange blobs</summary>
        //public List<CryptoDataExchange> Exchanges;

        /// <summary>
        /// OID of algorithm that produced the result.
        /// </summary>
        public string OID { get; set; }

        /// <summary>
        /// Create and populate a result.
        /// </summary>
        /// <param name="Identifier">The Goedel Cryptography identifier.</param>
        /// <param name="Bulk">Provider to use to process the bulk data</param>
        protected CryptoData(CryptoAlgorithmID Identifier, 
                            CryptoProviderBulk Bulk) {
            this.BaseAlgorithmIdentifier = Identifier;
            this.ProviderBulk = Bulk;
            }

        /// <summary>
        /// CryptoProvider for performing operations on the bulk data under
        /// the wrapped key.
        /// </summary>
        public CryptoProviderBulk ProviderBulk { get; }

        /// <summary>
        /// Integrity value
        /// </summary>
        public byte[] Integrity { get; set; }

        /// <summary>
        /// The result of the cryptographic transform on the data
        /// </summary>
        public byte[] ProcessedData { get; set; }

        /// <summary>
        /// Fingerprint of the key used to wrap the inner key
        /// </summary>
        public string UDF { get; }

        /// <summary>
        /// Terminate the encoding operation and perform the signature.
        /// </summary>
        public virtual void Complete() {
            ProviderBulk.Complete(this);
            }

        /// <summary>
        /// Write the binary data to the input stream.
        /// </summary>
        /// <param name="Data">The data to write</param>
        /// <param name="Count">Number of bytes to process</param>
        /// <param name="Offset">Offset to begin processing at.</param>
        public void Write (byte[] Data, int Offset=0, int Count=-1) {
            if (Count < 0) {
                Count = Data.Length - Offset;
                }

            if (Data != null) {
                InputStream.Write(Data, Offset, Count);
                }
            }



        /// <summary>
        /// Ephemeral key pair used in wrap operation.
        /// </summary>
        public KeyPair Ephemeral { get; set; }

        }

    /// <summary>
    /// Represent the result of a Signature or Exchange operation
    /// </summary>
    public abstract class CryptoDataMeta : CryptoData {
        /// <summary>
        /// Crypto provider for keying operations (may be null)
        /// </summary>
        public abstract CryptoProvider Meta { get;}

        /// <summary>
        /// The bulk provider
        /// </summary>
        public CryptoData BulkData { get; set; }

        /// <summary>
        /// The Bulk Identifier
        /// </summary>
        public override CryptoAlgorithmID BulkID  => BulkData.BulkID;

        /// <summary>
        /// Create and populate a result.
        /// </summary>
        /// <param name="Identifier">The Goedel Cryptography identifier.</param>
        /// <param name="BulkData">Provider to use to process the bulk data</param>
        public CryptoDataMeta(CryptoAlgorithmID Identifier, CryptoData BulkData) :
                    base(Identifier, BulkData.ProviderBulk) => this.BulkData = BulkData;
        }


    ///// <summary>
    ///// Result of a Signature operation.
    ///// </summary>
    //public class CryptoDataSignature : CryptoDataMeta {
    //    /// <summary>
    //    /// Signature value
    //    /// </summary>
    //    public byte[] Signature { get; set; }


    //    /// <summary>
    //    /// Crypto provider for keying operations (may be null)
    //    /// </summary>
    //    public override CryptoProvider Meta => SignatureProvider; 

    //    /// <summary>
    //    /// Crypto provider for signature
    //    /// </summary>
    //    public CryptoProviderSignature SignatureProvider { get; }


    //    /// <summary>
    //    /// Create and populate a result.
    //    /// </summary>
    //    /// <param name="Identifier">The Goedel Cryptography identifier.</param>
    //    /// <param name="Bulk">Provider to use to process the bulk data</param>
    //    /// <param name="SignatureProvider">The signature provider</param>
    //    public CryptoDataSignature(CryptoAlgorithmID Identifier,
    //                        CryptoData Bulk, CryptoProviderSignature SignatureProvider) : base (Identifier, Bulk) {
    //        Bulk.Signatures = Bulk.Signatures ?? new List<CryptoDataSignature>();
    //        Bulk.Signatures.Add(this);
    //        this.SignatureProvider = SignatureProvider;
    //        }

    //    }

    ///// <summary>
    ///// Result of a key encryption, exchange or wrap operation.
    ///// </summary>
    //public class CryptoDataExchange : CryptoDataMeta {

    //    /// <summary>Wrapped key, if required.</summary>
    //    public byte[] Wrap {
    //        get; set;
    //        }

    //    /// <summary>
    //    /// The public key exchange parameters
    //    /// </summary>
    //    public byte[] Exchange { get; set; }

    //    /// <summary>
    //    /// Optional carry result to be used in decryption
    //    /// </summary>
    //    public byte[] Carry { get; set; }


    //    /// <summary>
    //    /// Crypto provider for keying operations (may be null)
    //    /// </summary>
    //    public override CryptoProvider Meta => ExchangeProvider; 

    //    /// <summary>
    //    /// Crypto provider for signature
    //    /// </summary>
    //    public CryptoProviderExchange ExchangeProvider { get; }



    //    /// <summary>
    //    /// Create and populate a result.
    //    /// </summary>
    //    /// <param name="Identifier">The Goedel Cryptography identifier.</param>
    //    /// <param name="Bulk">Encoder for the bulk data</param>
    //    /// <param name="Meta">The Key Exchange Provider</param>
    //    public CryptoDataExchange(CryptoAlgorithmID Identifier,
    //                  CryptoData Bulk, CryptoProviderExchange Meta) :
    //                        base(Identifier, Bulk) => ExchangeProvider = Meta;
    //    }

    /// <summary>
    /// Wrapped Crypto Data
    /// </summary>
    public class CryptoDataEncoder : CryptoData {

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="Identifier">The Goedel Cryptography identifier.</param>
        /// <param name="Bulk">Provider to use to process the bulk data
        /// signature operations where the asymmetric operation is performed after the
        /// bulk operation completes.</param> 

        public CryptoDataEncoder(CryptoAlgorithmID Identifier,
                        CryptoProviderBulk Bulk) :
                             base(Identifier, Bulk) => AlgorithmIdentifier = Identifier;
        }

    /// <summary>
    /// Wrapped Crypto Data
    /// </summary>
    public class CryptoDataDecoder : CryptoData {
        /// <summary>
        /// Result of verification operation. If true, verification
        /// succeeded. If false verification failed. If null, no
        /// verification operation was performed.
        /// </summary>
        public bool? Verified { get; set; } = null;


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="Identifier">The Goedel Cryptography identifier.</param>
        /// <param name="Bulk">Provider to use to process the bulk data
        /// signature operations where the asymmetric operation is performed after the
        /// bulk operation completes.</param>
        public CryptoDataDecoder(CryptoAlgorithmID Identifier,
                        CryptoProviderBulk Bulk) :
                             base(Identifier, Bulk) {
            }
        }




    }
