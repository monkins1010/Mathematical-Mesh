﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Goedel.CryptoLibNG {




    /// <summary>
    /// Base class for all cryptographic hash providers.
    /// 
    /// Provides utility and convenience functions that are employed in derrived
    /// classes. This provides consistency when using either the built in .NET
    /// providers or those from other sources.
    /// 
    /// Unlike the .NET API, the wrapper provider completely conceals the details 
    /// of the cryptographic algorithm implementation. It is not necessary to 
    /// observe block boundaries when using the TransformData methods.
    /// </summary>
    public abstract class CryptoProviderDigest : CryptoProviderBulk {

        /// <summary>
        /// The type of algorithm
        /// </summary>
        public override CryptoAlgorithmClass AlgorithmClass { get { return CryptoAlgorithmClass.Digest; } }


        /// <summary>
        /// Hash algorithm provider.
        /// </summary>
        public HashAlgorithm HashAlgorithm;
        

        /// <summary>
        /// Initializes an instance of the hash provider with the specified
        /// implementation.
        /// </summary>
        /// <param name="HashAlgorithm"></param>
        protected CryptoProviderDigest(HashAlgorithm HashAlgorithm) {
            this.HashAlgorithm = HashAlgorithm;
            //Buffer = new byte[HashAlgorithm.InputBlockSize];
            }

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected CryptoProviderDigest() {
            }


        /// <summary>
        /// Computes the hash value for the specified region of the specified byte array
        /// </summary>
        /// <param name="Buffer">The input to compute the hash code for.</param>
        /// <param name="offset">The offset into the byte array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the array to use as data.</param>
        /// <returns>The computed hash code.</returns>
        public override CryptoData Process(byte[] Buffer, int offset, int count) {
            var Data = HashAlgorithm.ComputeHash(Buffer, offset, count);
            var CryptoDigestValue = new CryptoData(CryptoAlgorithmID, OID, Data);
            return CryptoDigestValue;
            }

        /// <summary>
        /// Terminates a part processing session and returns the result.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        public override CryptoData TransformFinal() {
            if (MemoryStream == null) {
                MemoryStream = new MemoryStream();
                }
            var Data = HashAlgorithm.ComputeHash(MemoryStream);
            var CryptoDigestValue = new CryptoData(CryptoAlgorithmID, OID, Data);
            return CryptoDigestValue;
            }

        /// <summary>
        /// Initializes or re-initializes an instance.
        /// </summary>
        public virtual void Initialize() {
            HashAlgorithm.Initialize();
            //Pointer = 0;
            }

        }


    /// <summary>
    /// Provider for the SHA-2 256 bit Hash Algorithm
    /// </summary>
    public class CryptoProviderSHA2_256 : CryptoProviderDigest {


        /// <summary>
        /// Return a CryptoAlgorithm structure with properties describing this provider.
        /// </summary>
        public override CryptoAlgorithm CryptoAlgorithm {
            get { return new CryptoAlgorithm (
                CryptoAlgorithmID.SHA_2_256, Name, 256,
                null, null, null, 
                "http://www.w3.org/2001/04/xmlenc#sha256",
                CryptoAlgorithmClass.Digest,
                GetCryptoProvider);}
            }



        /// <summary>
        /// The CryptoAlgorithmID Identifier.
        /// </summary>
        public override CryptoAlgorithmID CryptoAlgorithmID {
            get {
                return CryptoAlgorithmID.SHA_2_256;
                }
            }
        /// <summary>
        /// .NET Framework name
        /// </summary>
        public override string Name {
            get {
                return "SHA256";
                }
            }
        /// <summary>
        /// JSON Algorithm Name
        /// </summary>
        public override string JSONName {
            get {
                return "HS256";
                }
            }
        /// <summary>
        /// Default output size.
        /// </summary>
        public override int Size {
            get {
                return 256;
                }
            }
        /// <summary>
        /// Returns a factory delegate for this class.
        /// </summary>
        public override GetCryptoProvider GetCryptoProvider {
            get {
                return Factory;
                }
            }
        private static CryptoProvider Factory(int KeySize, CryptoAlgorithmID DigestAlgorithm) {
            return new CryptoProviderSHA2_256();
            }

        /// <summary>
        /// Create a SHA-2-256 digest provider.
        /// </summary>
        public CryptoProviderSHA2_256()
            : base(new SHA256Cng()) {
            }
        }

    /// <summary>
    /// Provider for the SHA-2 512 bit Hash Algorithm
    /// </summary>
    public class CryptoProviderSHA2_512 : CryptoProviderDigest {


        /// <summary>
        /// Return a CryptoAlgorithm structure with properties describing this provider.
        /// </summary>
        public override CryptoAlgorithm CryptoAlgorithm {
            get {
                return new CryptoAlgorithm(
                    CryptoAlgorithmID.SHA_2_512, Name, 512,
                    null, null, null,
                    "http://www.w3.org/2001/04/xmlenc#sha512",
                    CryptoAlgorithmClass.Digest,
                    GetCryptoProvider);
                }
            }

        /// <summary>
        /// The CryptoAlgorithmID Identifier.
        /// </summary>
        public override CryptoAlgorithmID CryptoAlgorithmID {
            get {
                return CryptoAlgorithmID.SHA_2_512;
                }
            }
        /// <summary>
        /// .NET Framework name
        /// </summary>
        public override string Name {
            get {
                return "SHA512";
                }
            }
        /// <summary>
        /// JSON Algorithm Name
        /// </summary>
        public override string JSONName {
            get {
                return "HS512";
                }
            }
        /// <summary>
        /// Default output size.
        /// </summary>
        public override int Size {
            get {
                return 512;
                }
            }
        /// <summary>
        /// Returns the default crypto provider.
        /// </summary>
        public override GetCryptoProvider GetCryptoProvider {
            get {
                return Factory;
                }
            }
        private static CryptoProvider Factory(int KeySize, CryptoAlgorithmID DigestAlgorithm) {
            return new CryptoProviderSHA2_512();
            }
        /// <summary>
        /// Create a SHA-2-256 digest provider.
        /// </summary>
        public CryptoProviderSHA2_512() : base (new SHA512Cng()) {
            }
        }


    }