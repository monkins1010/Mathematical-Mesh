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
using Goedel.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Goedel.Cryptography {

    /// <summary>
    /// Base class for Shamir Shared Secrets.
    /// </summary>
    public class Shared {

        ///<summary>The prime modulus</summary>
        public BigInteger Prime;

        ///<summary>The secret value.</summary>
        public virtual BigInteger Value { get; set; }

        /// <summary>
        /// Base constructor.
        /// </summary>
        public Shared() {
            }

        /// <summary>
        /// Construct a secret sharing of <paramref name="value"/> using modulus 
        /// <paramref name="prime"/>.
        /// </summary>
        /// <param name="value">The secret value to share.</param>
        /// <param name="prime">The prime modulus.</param>
        public Shared(BigInteger value, BigInteger prime) {
            Value = value;
            Prime = prime;
            }

        /// <summary>
        /// Create a set of N key shares with a quorum of K.
        /// </summary>
        /// <param name="keyShares">The key shares pre-initialized with the x coordinate value.</param>
        /// <param name="t">Quorum of key shares required to reconstruct the secret.</param>
        /// <param name="polynomial">The polynomial co-efficients generated.</param>
        /// <returns>The key shares created.</returns>
        public virtual void Split(KeyShare[] keyShares, int t, out BigInteger[] polynomial) {
            var n = keyShares.Length;

            polynomial = new BigInteger[t];
            polynomial[0] = Value;

            Console.WriteLine("Key = {0} ", polynomial[0]);

            for (int i = 1; i < t; i++) {
                polynomial[i] = BigNumber.Random(Prime);
                }

            for (int i = 0; i < n; i++) {
                keyShares[i].Value = PolyMod(keyShares[i].Index, polynomial, Prime);
                keyShares[i].Prime = Prime;
                }

            return;
            }

        // Note here that since BigInteger is a structure, not a class, we don't
        // need to worry about boxing or call by value issues
        BigInteger PolyMod(BigInteger x, BigInteger[] polyNomial, BigInteger modulus) {
            BigInteger power = 1;  // expect the optimizer to catch
            var result = polyNomial[0];
            for (int i = 1; i < polyNomial.Length; i++) {
                power = (x * power) % modulus;
                result = (modulus + result + polyNomial[i] * power) % modulus;
                }

            return result;
            }

        /// <summary>
        /// Combine the shares <paramref name="shares"/> using the prime modulus 
        /// <paramref name="modulus"/> with a threshold of <paramref name="threshold"/>
        /// </summary>
        /// <param name="shares">The shares to construct the coefficient from.</param>
        /// <param name="modulus">The prime modulus.</param>
        /// <param name="threshold">The threshold value.</param>
        /// <returns>The recovered value.</returns>
        public static BigInteger CombineNT(KeyShare[] shares,
                    BigInteger modulus,
                    int threshold) {
            BigInteger accum = 0;

            for (var formula = 0; formula < threshold; formula++) {

                var lagrange = Lagrange(shares, formula, modulus);
                var value = shares[formula].Value;

                accum = (accum + (value * lagrange)).Mod(modulus);
                }
            return accum;
            }

        /// <summary>
        /// Construct the Lagrange coefficient for the share <paramref name="index"/> among
        /// the shares <paramref name="shares"/> in the prime modulus <paramref name="modulus"/>.
        /// </summary>
        /// <param name="shares">The shares to construct the coefficient from.</param>
        /// <param name="index">The index of the share to construct the coefficient for.</param>
        /// <param name="modulus">The prime modulus.</param>
        /// <returns>The Lagrange coefficient</returns>
        public static BigInteger Lagrange(KeyShare[] shares,
                    int index,
                    BigInteger modulus) {

            BigInteger numerator = 1, denominator = 1;
            var start = shares[index].Index;

            for (var count = 0; count < shares.Length; count++) {
                if (index == count) {
                    continue;  // If not the same value
                    }

                var next = shares[count].Index;

                numerator = (numerator * -next).Mod(modulus);
                denominator = (denominator * (start - next)).Mod(modulus);

                Console.WriteLine($"Numerator {numerator}");
                Console.WriteLine($"Denominator {denominator}");
                }

            var invDenominator = ModInverse(denominator, modulus);
            var result = (numerator * invDenominator).Mod(modulus);

            Console.WriteLine($"InvDenominator {invDenominator}");
            Console.WriteLine($"result {result}");

            return result;
            }


        ///// <summary>
        ///// Combine the shares <paramref name="shares"/> using the prime modulus 
        ///// <paramref name="modulus"/> with a threshold of <paramref name="threshold"/>
        ///// </summary>
        ///// <param name="shares">The shares to construct the coefficient from.</param>
        ///// <param name="modulus">The prime modulus.</param>
        ///// <param name="threshold">The threshold value.</param>
        ///// <returns>The recovered value.</returns>
        //public static BigInteger CombineNT2(KeyShare[] shares, BigInteger modulus, int threshold) {

        //    Assert.False(shares.Length < threshold, InsufficientShares.Throw);

        //    BigInteger accum = 0;

        //    for (var formula = 0; formula < threshold; formula++) {

        //        var value = shares[formula].Value;
        //        var start = shares[formula].Index;

        //        Console.WriteLine($"Value = {start}, {value} ");

        //        BigInteger numerator = 1, denominator = 1;
        //        for (var count = 0; count < threshold; count++) {
        //            if (formula == count) {
        //                continue;  // If not the same value
        //                }


        //            var next = shares[count].Index;

        //            numerator = (numerator * -next) % modulus;
        //            denominator = (denominator * (start - next)) % modulus;

        //            Console.WriteLine($"    {count},{next}:{numerator}/{denominator}");
        //            }

        //        Console.WriteLine($"Total {formula}: {numerator}/{denominator}");

        //        var InvDenominator = ModInverse(denominator, modulus);

        //        accum = (accum + (value * numerator * InvDenominator)).Mod(modulus);
        //        }

        //    return accum;
        //    }


        // Not the fastest way to do modular inverse but the easiest with the
        // available tools in .net
        static BigInteger ModInverse(BigInteger k, BigInteger m) {
            var m2 = m - 2;
            if (k < 0) {
                k += m;
                }

            return BigInteger.ModPow(k, m2, m);
            }


        }
    /// <summary>
    /// Represents a secret key that may be split into or reformed from 
    /// a collection of shares.
    /// </summary>
    public class SharedSecret : Shared {

        /// <summary>
        /// Size of key in bits. This will determine the size of the prime
        /// </summary>
        public virtual int KeyBits => Key.Length * 8;

        /// <summary>
        /// Size of key in bytes;
        /// </summary>
        public int KeyBytes => Key.Length;

        /// <summary>
        /// The Key value
        /// </summary>
        public virtual byte[] Key { get; set; }

        /// <summary>
        /// The Key Value as a Base32 encoded string.
        /// </summary>
        public virtual string UDFKey => Cryptography.UDF.SymmetricKey(Key);

        ///<summary>The UDF identifier of the secret value.</summary>
        public string UDFIdentifier => Cryptography.UDF.ContentDigestOfUDF(UDFKey, bits: KeyBits * 2);

        ///<summary>The maximum allowed secret value.</summary>
        public BigInteger SecretMax;

        ///<summary>The number of 32 bit words used to represent the value.</summary>
        public int ShareWords;


        /// <summary>
        /// Create a new random secret with the specified number of bits.
        /// </summary>
        /// <param name="bits">Nyumber of bits in the secret</param>
        public SharedSecret(int bits) : this(CryptoCatalog.GetBits(bits)) {
            }




        /// <summary>
        /// Create a secret from the specified key value.
        /// </summary>
        /// <param name="key">The key value.</param>
        public SharedSecret(byte[] key) {
            Key = key;
            Value = key.BigIntegerBigEndian();
            Prime = GetPrime(KeyBits, out SecretMax, out ShareWords);
            }


        /// <summary>
        /// Create a secret from the specified key value.
        /// </summary>
        /// <param name="udf">The key value as a UDF.</param>
        public SharedSecret(string udf) => this.Key = UDF.SymmetricKey(udf);

        /// <summary>
        /// Recreate a secret from the specified shares.
        /// </summary>
        /// <param name="shares">The shares to be recombined.</param>
        public SharedSecret(KeyShareSymmetric[] shares) => Key = Combine(shares);

        /// <summary>
        /// Recreate a secret from shares specified as Base32 encoded strings.
        /// </summary>
        /// <param name="shares">The shares to be recombined.</param>
        public SharedSecret(IEnumerable<string> shares) {
            var KeyShares = new KeyShareSymmetric[shares.Count()];
            int i = 0;
            foreach (var Share in shares) {
                KeyShares[i++] = new KeyShareSymmetric(Share);
                }

            Key = Combine(KeyShares);
            }



        /// <summary>
        /// Constructor for use in inherited classes.
        /// </summary>
        protected SharedSecret() {
            }

        // Override this because we override the Equals operator
        /// <summary>
        /// Hash code of the current class.
        /// </summary>
        /// <returns>Hash code of object instance.</returns>
        public override int GetHashCode() => UDFKey.GetHashCode();

        /// <summary>Test for equality
        /// </summary>
        /// <param name="obj">The secret to test against</param>
        /// <returns>true if the parameter has the same key value, false otherwise.</returns>
        public override bool Equals(System.Object obj) {
            if (obj == null) {
                return false;
                }
            if (!(obj is SharedSecret p)) {
                return false;
                }
            if (KeyBytes != p.KeyBytes) {
                return false;
                }
            for (int i = 0; i < KeyBytes; i++) {
                if (Key[i] != p.Key[i]) {
                    return false;
                    }
                }

            return true;
            }

        ///<summary>The set of prime offset values to be added to 32^(n) to give the
        ///discrete modulus for secrets of up to 32n bits.</summary>
        readonly static int[] PrimeValues = new int[] {
            15,
            13,
            61,
            51,
            7,
            133,
            735,
            297,
            127,
            27,
            55,
            231,
            235,
            211,
            165,
            75,
            };

        /// <summary>
        /// Return the prime number that is strictly greater than 2^n where n is 
        /// the smallest integer multiple of 32 greater or equal to <paramref name="bits"/>.
        /// </summary>
        /// <param name="bits">The number of bits to return the prime value for.</param>
        /// <param name="exponent">The power of 32</param>
        /// <param name="index">The number of 32 bit blocks required.</param>
        /// <returns>The prime number.</returns>
        public static BigInteger GetPrime(int bits, out BigInteger exponent, out int index) {
            Assert.True(bits > 0 & bits <= 512, KeySizeNotSupported.Throw);

            index = (bits + 31) / 32;
            exponent = BigInteger.Pow(2, 32 * index);
            return exponent + new BigInteger(PrimeValues[index - 1]);
            }


        //// Right now, this is not completely general as we require there
        //// to be at least two shares and the quorum to be at least two.

        //private BigInteger MakePositive(byte[] data) {
        //    var Data1 = new byte[data.Length + 1];
        //    Array.Copy(data, Data1, data.Length);
        //    return new BigInteger(Data1);
        //    }

        /// <summary>
        /// Create a set of N key shares with a quorum of K.
        /// </summary>
        /// <param name="n">Number of key shares to create (max is 32).</param>
        /// <param name="k">Quorum of key shares required to reconstruct the secret.</param>
        /// <returns>The key shares created.</returns>
        public KeyShareSymmetric[] Split(int n, int k) => Split(n, k, out var _);

        /// <summary>
        /// Create a set of N key shares with a quorum of K.
        /// </summary>
        /// <param name="n">Number of key shares to create (max is 15).</param>
        /// <param name="k">Quorum of key shares required to reconstruct the secret.</param>
        /// <param name="polynomial">The polynomial co-efficients generated.</param>
        /// <returns>The key shares created.</returns>
        public KeyShareSymmetric[] Split(int n, int k, out BigInteger[] polynomial) {
            Assert.False(k > n, QuorumExceedsShares.Throw);
            Assert.False(k < 1, QuorumInsufficient.Throw);
            Assert.False(n < 1, SharesInsufficient.Throw);
            Assert.False(n > 15, QuorumExceeded.Throw);
            Assert.False(k > 15, QuorumDegreeExceeded.Throw);

            var keyShares = new KeyShareSymmetric[n];

            for (int i = 0; i < n; i++) {
                keyShares[i] = new KeyShareSymmetric(i, k, ShareWords*4);
                }
            Split(keyShares, k, out polynomial);

            // Check that all our share chunks will fit into a UDF
            for (var search = true; search;
                Split(keyShares, k, out polynomial)) {
                search = false;
                for (int i = 0; i < n; i++) {
                    search = search ? keyShares[i].Value < SecretMax : false;
                    }
                }

            return keyShares;
            }

        static byte[] Combine(KeyShareSymmetric[] Shares) {

            var threshold = Shares[0].Threshold;
            foreach (var Share in Shares) {
                Assert.True(Share.Threshold == threshold, MismatchedShares.Throw);
                }
            return CombineNK(Shares);
            }


        static byte[] CombineNK(KeyShareSymmetric[] shares) {
            var threshold = shares[0].Threshold;
            Assert.False(shares.Length < threshold, InsufficientShares.Throw);

            var secretBits = shares[0].KeyBits - 8;
            var modulus = GetPrime(secretBits, out var secretMax, out var shareChunks);

            var accum = CombineNT(shares, modulus, threshold);
            Console.WriteLine($"Reconstructed value = {accum}");
            return accum.ToByteArrayBigEndian(shareChunks * 4);
            }

        }



    /// <summary>
    /// A member of a key share collection.
    /// </summary>
    public class KeyShare : SharedSecret {
        ///<summary>The index value</summary>
        public virtual BigInteger Index { get; set; }

        //public virtual BigInteger Value { get; }

        /// <summary>
        /// Calculate the Lagrange coefficient for the shares <paramref name="shares"/> and
        /// index <paramref name="i"/>.
        /// </summary>
        /// <param name="shares">The shares to calculate the index for.</param>
        /// <param name="i">The x value.</param>
        /// <returns>The Lagrange coefficient.</returns>
        public BigInteger Lagrange(KeyShare[] shares, int i) => Lagrange(shares, i, Prime);


        }

    /// <summary>
    /// A member of a key share collection.
    /// </summary>
    public class KeyShareSymmetric : KeyShare {
        /// <summary>
        /// The Key Value as a Base32 encoded string.
        /// </summary>
        public override string UDFKey => Cryptography.UDF.KeyShare(Key);

        ///<summary>The value of the first byte specifying the threshold and index values</summary>
        public int First;

        ///<summary>The number of bytes of the constructed share.</summary>
        public int ShareBytes;

        /// <summary>
        /// Quorum required to recombine the key shares to recover the secret.
        /// </summary>
        public int Threshold => First / 16;

        /// <summary>
        /// Index of this key share in the collection.
        /// </summary>
        public override BigInteger Index => (First & 0xf) + 1;

        /// <summary>
        /// The full key share data.
        /// </summary>
        public override byte[] Key {
            get => key;
            set {
                key = value;
                data = new byte[key.Length - 1];
                Array.Copy(key, 1, data, 0, key.Length-1);
                First = key[0];
                valueStore = data.BigIntegerBigEndian();
                }
            }

        byte[] key;

        /// <summary>
        /// The key share data (stripped of the quorum and index information)
        /// </summary>
        public byte[] Data {
            get => data;
            set {
                data = value;
                key = new byte[data.Length + 1];
                key[0] = (byte)First;
                Array.Copy(data, 0, key, 1, data.Length);
                valueStore = data.BigIntegerBigEndian();
                }
            }
        byte[] data;

        ///<summary>The key share value.</summary>
        public override BigInteger Value {
            get => valueStore;
            set {
                valueStore = value;
                data = Value.ToByteArrayBigEndian(ShareBytes);
                key = new byte[data.Length + 1];
                key[0] = (byte)First;
                Array.Copy(data, 0, key, 1, data.Length);
                }
            }
        BigInteger valueStore;

        /// <summary>
        /// Construct a key share with index <paramref name="index"/>, sharing a secret of 
        /// <paramref name="shareBytes"/> bytes and a threshold of <paramref name="threshold"/>.
        /// </summary>
        /// <param name="index">The x value of the share.</param>
        /// <param name="threshold">The threshold value for the shares.</param>
        /// <param name="shareBytes">The number of bytes shared.</param>
        public KeyShareSymmetric(int index, int threshold, int shareBytes) {
            First = index + 0x10 * threshold;
            ShareBytes = shareBytes;
            }

        /// <summary>
        /// Construct a key share with the specified secret value.
        /// </summary>
        /// <param name="text">The secret value in text form.</param>
        public KeyShareSymmetric(string text) {
            var buffer = UDF.Parse(text, out var code);
            Assert.True(code == (byte)UdfTypeIdentifier.ShamirSecret);
            Key = buffer;
            }


        /// <summary>
        /// Construct a key share with the specified secret value and index.
        /// </summary>
        /// <param name="index">The key share index and threshold.</param>
        /// <param name="value">The key share value/</param>
        /// <param name="bytes">Number of bytes in the share to be constructed.</param>
        public KeyShareSymmetric(int index, BigInteger value, int bytes) {
            First = index;
            ShareBytes = bytes;
            Value = value;
            }


        /// <summary>
        /// Construct a key share with the specified secret value and index.
        /// </summary>
        /// <param name="index">The key share index and threshold.</param>
        /// <param name="data">The key share value/</param>
        public KeyShareSymmetric(int index, byte[] data) {
            First = index;
            Data = data;

            }
        }
    }
