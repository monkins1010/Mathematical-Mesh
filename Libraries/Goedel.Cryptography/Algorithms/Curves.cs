﻿using Goedel.Utilities;

using System;
using System.Collections.Generic;
using System.Numerics;

namespace Goedel.Cryptography.Algorithms {

    /// <summary>
    /// Base Class for Elliptic Curve implementations
    /// </summary>
    public abstract class Curve {

        ///<summary>The domain parameters</summary>
        public abstract DomainParameters DomainParameters { get;  }

        ///<summary>The modulus</summary>
        public BigInteger Prime => DomainParameters.P;

        /// <summary>The number of bits to multiply</summary>
        public int Bits => DomainParameters.Bits;




        ///<summary>The square root of -1.</summary>
        public BigInteger SqrtMinus1 => DomainParameters.SqrtMinus1;

        /// <summary>
        /// Add the point <paramref name="point"/> to this point on the curve
        /// and return the result as a new point.
        /// </summary>
        /// <param name="point">The point to add.</param>
        /// <returns>The sum of the two points.</returns>
        public abstract Curve Add(Curve point);

        /// <summary>
        /// Return a IKeyAdvancedPublic public key for this point. 
        /// </summary>
        public abstract IKeyAdvancedPublic KeyAdvancedPublic { get; }
        }


    /// <summary>
    /// Montgomery Curve [v^2 = u^3 + A*u^2 + u] for 2^255-19
    /// </summary>
    public abstract class CurveMontgomery : Curve {

        /// <summary>The U value</summary>
        public BigInteger U { get; set; }

        /// <summary>The V value, used for Point Addition.</summary>
        public BigInteger V { get => v;
            set {
                v = value;
                odd = !v.IsEven;
                }
            }
        BigInteger v;

        ///<summary>If true the point value is odd.</summary>
        public bool? Odd {
            set {
                v = (value == null) ? -1 : GetV(value);
                odd = value;
                }
            get => odd;
            }
        bool? odd;


        /// <summary>The parameter A24</summary>
        public BigInteger A => DomainParameters.A;

        /// <summary>The parameter A24</summary>
        public BigInteger A24 => DomainParameters.A24;

        /// <summary>
        /// Return the V point corresponding to U
        /// </summary>
        /// <param name="odd">If true, the odd value of V is returned. If false, the
        /// even value is returned. Otherwise, either value may be returned.</param>
        /// <returns>The V corresponding to U.</returns>
        public BigInteger GetV(bool? odd = null) {
            return GetV(U, odd);
            }

        /// <summary>
        /// Return the V point corresponding to the u coordinate value <paramref name="u"/>
        /// with sign specified by <paramref name="odd"/>.
        /// </summary>
        /// <param name="odd">If true, the odd value of V is returned. If false, the
        /// even value is returned. Otherwise, either value may be returned.</param>
        /// <param name="u">The u coordinate value.</param>
        /// <returns>The V corresponding to U.</returns>
        public BigInteger GetV(BigInteger u , bool? odd = null) {
            //v^2 = u^3 + A*u^2 + u

            var u2 = (u * u).Mod(Prime);
            var u3 = (u * u2).Mod(Prime);
            var v2 = (u3 + A * u2 + u).Mod(Prime);

            var v = v2.Sqrt(Prime, SqrtMinus1, odd);

            // check the value is correct
            ((v * v).Mod(Prime) == v2).AssertTrue();
            return v;
            }



        #region // equality
        /// <summary>
        /// Return the hash code
        /// </summary>
        /// <returns>The hash code value.</returns>
        public override int GetHashCode() => U.GetHashCode();

        /// <summary>
        /// Check points for equality
        /// </summary>
        /// <param name="obj">Other point</param>
        /// <returns>True if the two points are equal (the P and U values are the same), otherwise false.</returns>
        public override bool Equals(object obj) => Equal((CurveMontgomery)obj);

        /// <summary>
        /// Check points for equality
        /// </summary>
        /// <param name="obj">Other point</param>
        /// <returns>True if the two points are equal (the P and U values are the same), otherwise false.</returns>
        public bool Equal(CurveMontgomery obj) => this == obj;

        /// <summary>
        /// Check points for equality
        /// </summary>
        /// <param name="a">First point</param>
        /// <param name="b">Second point</param>
        /// <returns>True if the two points are equal (the P and U values are the same), otherwise false.</returns>
        public static bool operator ==(CurveMontgomery a, CurveMontgomery b) =>
            (a.Prime == b.Prime) & (a.U == b.U);

        /// <summary>
        /// Check points for inequality
        /// </summary>
        /// <param name="a">First point</param>
        /// <param name="b">Second point</param>
        /// <returns>True if the two points are not equal (the P and/or U values are different), otherwise false.</returns>
        public static bool operator !=(CurveMontgomery a, CurveMontgomery b) => !(a == b);
        #endregion

        /// <summary>
        /// Create a point from the specified U value.
        /// </summary>
        /// <param name="u">U value to convert</param>
        /// <param name="odd">If true, the V value is odd, if false, the V value 
        /// is even, otherwise, the V value is undefined.</param>
        /// <returns>Created point</returns>
        public abstract CurveMontgomery Factory(BigInteger u, bool? odd);


        /// <summary>
        /// Multiply a point by a scalar
        /// </summary>
        /// <param name="u">The u coordinate of the point P = {u, v}</param>
        /// <param name="s">Scalar factor</param>
        /// <param name="x_2">The projective coordinate xz of s.P</param>
        /// <param name="z_2">The projective coordinate multiplier z of s.P</param>
        /// <param name="x_3">The projective coordinate x of s.(P+1)</param>
        /// <param name="z_3">The projective coordinate multiplier z of s.(P+1)</param>
        /// <returns>The results of the multiplication, (sU, (s+1)U)</returns>
        public void ScalarAccumulate(BigInteger u, BigInteger s,
                    out BigInteger x_2, out BigInteger z_2,
                    out BigInteger x_3, out BigInteger z_3) {
            var P = Prime;

            var x_1 = u;
            x_2 = 1;
            z_2 = 0;
            x_3 = u;
            z_3 = 1;

            bool swap = false;

            for (var i = Bits - 1; i >= 0; i--) {
                var k_t = ((s >> i) & 1) == 1;
                swap ^= k_t;
                Cswap(swap, ref x_2, ref x_3);
                Cswap(swap, ref z_2, ref z_3);
                swap = k_t;

                var A = (x_2 + z_2).Mod(P);
                var AA = (A * A).Mod(P);
                var B = (x_2 - z_2).Mod(P);
                var BB = (B * B).Mod(P);
                var E = (AA - BB).Mod(P);
                var C = (x_3 + z_3).Mod(P);
                var D = (x_3 - z_3).Mod(P);
                var DA = (D * A).Mod(P);
                var CB = (C * B).Mod(P);

                var x_3rt = (DA + CB);
                x_3 = (x_3rt * x_3rt).Mod(P);

                var z_3rt = (DA - CB);
                z_3 = (x_1 * (z_3rt * z_3rt)).Mod(P);
                x_2 = (AA * BB).Mod(P);
                z_2 = (E * (AA + A24 * E)).Mod(P);
                }

            Cswap(swap, ref x_2, ref x_3);
            Cswap(swap, ref z_2, ref z_3);

            Console.WriteLine($"x_2  {x_2.IsEven}  z_2  {z_2.IsEven}");
            }


        public BigInteger Recover(BigInteger x_2, BigInteger z_2) =>
            (x_2 * (BigInteger.ModPow(z_2, (Prime - 2), Prime))).Mod(Prime);

        /// <summary>
        /// Multiply a point by a scalar
        /// </summary>
        /// <param name="u">The U value of the point to multiply</param>
        /// <param name="s">Scalar factor</param>
        /// <returns>The results of the multiplication, (sU, (s+1)U)</returns>
        public (BigInteger, BigInteger) ScalarAccumulate(BigInteger u, BigInteger s) {
            ScalarAccumulate(u, s, out var x_2, out var z_2, out var x_3, out var z_3);

            Console.WriteLine($"x_2  {x_2.IsEven}  z_2  {z_2.IsEven}");

            var sU = Recover(x_2, z_2);
            var s1U = (x_3 * (BigInteger.ModPow(z_3, (Prime - 2), Prime))).Mod(Prime);

            return (sU, s1U);
            }

        /// <summary>
        /// Multiply a point by a scalar and return the U value only.
        /// </summary>
        /// <param name="s">Scalar factor</param>
        /// <returns>The result of the multiplication</returns>
        public BigInteger ScalarMultiply(BigInteger s) {
            ScalarAccumulate(U, s, out var x_2, out var z_2, out var x_3, out var z_3);
            return Recover(x_2, z_2);
            }

        /// <summary>
        /// Multiply this point by a scalar and return the new point.
        /// </summary>
        /// <param name="s">Scalar factor</param>
        /// <returns>The result of the multiplication</returns>
        public CurveMontgomery Multiply(BigInteger s) {
            BigInteger u;
            if (Odd == null) {
                u = ScalarMultiply(s);
                return Factory(u, null);
                }
            return MultiplySigned(s);
            }



        /// <summary>
        /// Multiply this point by a scalar and return the new point.
        /// </summary>
        /// <param name="s">Scalar factor</param>
        /// <returns>The result of the multiplication</returns>
        public CurveMontgomery MultiplySigned(BigInteger s) {
            var (u, v) = ScalarMultiplySigned(s);
            return Factory(u, !v.IsEven);
            }

        /// <summary>
        /// Multiply this point by a scalar and return the new point.
        /// </summary>
        /// <param name="s">Scalar factor</param>
        /// <returns>The result of the multiplication</returns>
        public CurveMontgomery MultiplySignedSlow(BigInteger s) {
            ScalarAccumulate(U, s, out var xq, out var zq, out var xq1, out var zq1);

            var uq = Recover(xq, zq);
            var uq1 = Recover(xq1, zq1);

            var vq = GetV(uq, true);
            //var vq1 = GetV(uq1, true);

            CheckCurve(uq, vq);
            var q = Factory(uq, !vq.IsEven);

            var q1 = Add(q);
            if (uq1 != q1.U) {
                q.V = (Prime - q.V).Mod(Prime);
                }
            return q;
            }


        /// <summary>
        /// Multiply a point by a scalar and return the U and V values.
        /// </summary>
        /// <param name="s">Scalar factor</param>
        /// <returns>The result of the multiplication (u, v)</returns>
        public (BigInteger, BigInteger) ScalarMultiplySigned(BigInteger s) {
            var P = Prime;
            var xp = U;
            var yp = V;
            var A = DomainParameters.A;

            ScalarAccumulate(U, s, out var xq, out var zq, out var xq1, out var zq1);

            var v1 = (xp * zq).Mod(P);
            var v2 = (xq + v1).Mod(P);
            var v3 = (xq - v1).Mod(P);
            v3 = (v3 * v3).Mod(P);
            v3 = (v3 * xq1).Mod(P);
            v1 = (2 * A * zq).Mod(P);

            v2 = (v2 + v1).Mod(P);
            var v4 = (xp * xq).Mod(P);
            v4 = (v4 + zq).Mod(P);
            v2 = (v2 * v4).Mod(P);
            v1 = (v1 * zq).Mod(P);
            v2 = (v2 - v1).Mod(P);
            v2 = (v2 * zq1).Mod(P);

            var y_out = (v2 - v3).Mod(P);
            v1 = (2 * yp).Mod(P);
            v1 = (v1 * zq).Mod(P);
            v1 = (v1 * zq1).Mod(P);
            var x_out = (v1 * xq).Mod(P);
            var z_out = (v1 * zq).Mod(P);

            var x = Recover(x_out, z_out);
            var y = Recover(y_out, z_out);

            return (x, y);

            }


        /// <summary>
        /// Conditional swap in constant time.
        /// </summary>
        /// <param name="Swap">If false A'=A and B'=B on exit, if true A'=B and B'=A on exit</param>
        /// <param name="A">First parameter</param>
        /// <param name="B">Second parameter</param>
        public static void Cswap(bool Swap, ref BigInteger A, ref BigInteger B) {

            if (Swap) {  // Limit: This should be in constant time
                var Dummy = A;
                A = B;
                B = Dummy;
                }

            }



        /// <summary>
        /// Encode the code point.
        /// </summary>
        /// <returns>The encoded format of the point</returns>
        public abstract byte[] Encode(bool extended=false);


        public override Curve Add(Curve point) => Add (point as CurveMontgomery);

        /// <summary>
        /// Add two points
        /// </summary>
        /// <param name="point">Second point</param>
        /// <returns>The result of the addition.</returns>
        public CurveMontgomery Add(CurveMontgomery point) {
            var (u, v) = Add(this, point);
            return Factory(u, !v.IsEven);
            }

        /// <summary>
        /// Add two points
        /// </summary>
        /// <param name="point1">first point</param>
        /// <param name="point2">Second point</param>
        /// <returns>The result of the addition.</returns>
        public (BigInteger, BigInteger) Add(CurveMontgomery point1, CurveMontgomery point2) {

            (point1.Prime == point2.Prime).AssertTrue();

            var u1 = point1.U;
            var v1 = point1.V;

            var u2 = point2.U;
            var v2 = point2.V;

            if ((u1 == u2) & (v1 == v2)) {
                return Double(point1);
                }

            var B = (v2 - v1).Mod(Prime);
            var C = (u2 - u1).Mod(Prime);
            var CINV = C.ModularInverse(Prime);
            var D = (B * CINV).Mod(Prime);
            var DD = (D * D).Mod(Prime);
            var DDD = (DD * D).Mod(Prime);

            var u3 = (DD - A - u1 - u2).Mod(Prime);
            var v3 = ((u1 + u1 + u2 + A) * B * CINV) - DDD - v1;
            
            CheckCurve(u3, v3).AssertTrue();

            return (u3, v3);
            }


        public (BigInteger, BigInteger) Double (CurveMontgomery point) {
            var x = point.U;
            var y = point.V;

            var xx3 = (x * x *3).Mod(Prime);
            //var x2 = (x + x).Mod(Prime);
            var y2inv = (y + y).ModularInverse(Prime);

            var l = ((xx3 + 2*A* x+ 1) * y2inv).Mod(Prime);
            var ll = (l * l).Mod(Prime);
            var lll = (ll * l).Mod(Prime);

            var u3 = (ll - A - x - x).Mod(Prime);
            var v3 = ((x + x + x + A) * l - lll - y).Mod(Prime);

            return (u3, v3);
            }


        public bool IsValid => CheckCurve(U, V);

        public bool CheckCurve(BigInteger u, BigInteger v) {
            var vv = (v * v).Mod(Prime);
            var uu = (u * u).Mod(Prime);
            var uuu = (uu * u).Mod(Prime);

            return (vv == (uuu + A * uu + u).Mod(Prime));
            }


        /// <summary>
        /// Add two points
        /// </summary>
        /// <param name="point">Second point</param>
        /// <returns>The result of the addition.</returns>
        public virtual void Accumulate(CurveMontgomery point) => throw new NYI();


        }



    /// <summary>
    /// Edwards Curve [x^2 = (y^2 - 1) / (d y^2 + 1) (mod p)] for 2^255-19
    /// </summary>
    public abstract class CurveEdwards : Curve {

        /// <summary>The X coordinate</summary>
        public BigInteger X { get; set; }

        /// <summary>The Y coordinate</summary>
        public BigInteger Y { get; set; }

        /// <summary>The projected Z coordinate</summary>
        public BigInteger Z { get; set; }

        ///<summary>The Curve Constant d</summary>
        public BigInteger CurveConstantD => DomainParameters.D;

        public abstract byte[] Encode();

        /// <summary>
        /// Add this point to a second point
        /// </summary>
        /// <param name="P2">Second point</param>
        /// <returns>The result of the addition.</returns>
        public abstract CurveEdwards Add(CurveEdwards P2);

        public override Curve Add(Curve point) => Add(point as CurveEdwards);

        #region // Equality testing
        /// <summary>Test to see if two points on a curve are equal</summary>
        /// <param name="other">The point to test for equality.</param>
        /// <returns>True if the points are equal, otherwise false.</returns>
        public bool Equal(CurveEdwards other) {
            Assert.True(other.Prime == Prime);

            if (((X * other.Z) - (other.X * Z)).Mod(Prime) != 0) {
                return false;
                }
            if (((Y * other.Z) - (other.Y * Z)).Mod(Prime) != 0) {
                return false;
                }
            return true;
            }

        /// <summary>Test to see if the domain parameters are equal.</summary>
        /// <param name="other">The parameters to test against</param>
        /// <returns>True if the parameters are equal, otherwise false.</returns>
        public override bool Equals(object other) {
            if (!(other is CurveEdwards)) {
                return false;
                }

            return Equal((CurveEdwards)other);
            }

        /// <summary>Test to see if the domain parameters are equal.</summary>
        /// <param name="p1">First value to test</param>
        /// <param name="p2">Second value to test</param> 
        /// <returns>True if the parameters are equal, otherwise false.</returns>
        public static bool operator ==(CurveEdwards p1, CurveEdwards p2) => p1.Equal(p2);

        /// <summary>Test to see if the domain parameters are not equal.</summary>
        /// <param name="p1">First value to test</param>
        /// <param name="p2">Second value to test</param> 
        /// <returns>False if the parameters are equal, otherwise true.</returns>
        public static bool operator !=(CurveEdwards p1, CurveEdwards p2) => !(p1 == p2);

        /// <summary>
        /// Return a hash code. 
        /// </summary>
        /// <returns>Hash of the prime value.</returns>
        public override int GetHashCode() {
            var point = (X * Y * Z.ModularInverse(Prime)).Mod(Prime);

            return point.GetHashCode();
            }
        #endregion



        /// <summary>
        /// Recover the X coordinate from the Y value and sign of X.
        /// </summary>
        /// <param name="X0">If true X is odd, otherwise, X is even.</param>
        /// <returns>The X coordinate.</returns>
        public virtual BigInteger RecoverX(bool X0) {
            Assert.True(Y < Prime, InvalidOperation.Throw);
            var x2 = (Y * Y - 1) * (CurveConstantD * Y * Y + 1).ModularInverse(Prime);
            return x2.Sqrt(Prime, SqrtMinus1, X0);
            }

        /// <summary>Modular multiplicative inverse of Z</summary>
        public BigInteger ZInv => Z.ModularInverse(Prime);

        /// <summary>X translated to fixed coordinates</summary>
        public BigInteger X0 => (X * ZInv).Mod(Prime);

        /// <summary>Y translated to fixed coordinates</summary>
        public BigInteger Y0 => (Y * ZInv).Mod(Prime);

        /// <summary>
        /// Convert back from 3D to 2D representation
        /// </summary>
        /// <param name="Xout">The X value</param>
        /// <param name="Yout">The Y value</param>
        public void Translate(out BigInteger Xout, out BigInteger Yout) {
            Xout = (X * ZInv).Mod(Prime);
            Yout = (Y * ZInv).Mod(Prime);
            }

        /// <summary>
        /// Replace the current point value with the current value added to itself
        /// (used to implement multiply)
        /// </summary>
        public abstract void DoublePoint();

        /// <summary>
        /// Add two points
        /// </summary>
        /// <param name="Point">Second point</param>
        /// <returns>The result of the addition.</returns>
        public abstract void Accumulate(CurveEdwards Point);

        /// <summary>
        /// Get the K value corresponding to this point
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="encodedR"></param>
        /// <param name="encodedA"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract BigInteger GetK(
            byte[] domain,
            byte[] encodedR,
            byte[] data);


        public abstract bool Verify(BigInteger k, BigInteger s, CurveEdwards R);


        public abstract ThresholdCoordinatorEdwards Coordinator();

        }


    public abstract class CurveEdwardsPrivate : IKeyAdvancedPrivate {


        public abstract CurveEdwards PublicPoint { get;}

        /// <summary>The private key, i.e. a scalar</summary>
        public BigInteger Private { get; protected set; }

        /// <summary>
        /// Perform a partial key agreement.
        /// </summary>
        /// <param name="keyPair">The key pair to perform the agreement against.</param>
        /// <returns>The key agreement result.</returns>
        public abstract KeyAgreementResult Agreement(KeyPair keyPair);

        /// <summary>
        /// Split the private key into a number of recryption keys.
        /// <para>
        /// Since the
        /// typical use case for recryption requires both parts of the generated machine
        /// to be used on a machine that is not the machine on which they are created, the
        /// key security level is always to permit export.</para>
        /// </summary>
        /// <param name="shares">The number of keys to create.</param>
        /// <returns>The created keys</returns>
        public abstract IKeyAdvancedPrivate[] MakeRecryptionKeySet(int shares);

        /// <summary>
        /// Make a recryption keyset by splitting the private key.
        /// </summary>
        /// <param name="Shares">Number of shares to create</param>
        /// <returns>Array shares.</returns>
        public abstract IKeyAdvancedPrivate CompleteRecryptionKeySet(IEnumerable<KeyPair> Shares);
        
        /// <summary>
        /// Combine the two public keys to create a composite public key.
        /// </summary>
        /// <param name="contribution">The key contribution.</param>
        /// <returns>The composite key</returns>
        public abstract IKeyAdvancedPrivate Combine(IKeyAdvancedPrivate contribution, 
            KeySecurity keySecurity = KeySecurity.Admin, KeyUses keyUses = KeyUses.Any);


        /// <summary>
        /// Sign a message using the public key according to RFC8032
        /// </summary>
        /// <remarks>This method does not prehash the message data since if
        /// prehashing is desired, it is because the data needs to be hashed
        /// before being presented.</remarks>
        /// <param name="message">The message</param>
        /// <param name="domain">Context value, if used.</param>
        /// <returns>The encoded signature data</returns>
        public byte[] Sign(byte[] message, byte[] domain = null) {

            // 1.Hash the private key, 32 octets, using SHA-512.  Let h denote the
            // resulting digest. Construct the secret scalar s from the first
            // half of the digest, and the corresponding public key A, as
            // described in the previous section.Let prefix denote the second
            // half of the hash digest, h[32],..., h[63].

            // 2.  Compute SHA-512(dom2(F, C) || prefix || PH(M)), where M is the
            // message to be signed.Interpret the 64-octet digest as a little-
            // endian integer r.

            // 3.  Compute the point[r]B. For efficiency, do this by first
            // reducing r modulo L, the group order of B.Let the string R be
            // the encoding of this point.

            // 4.  Compute SHA512(dom2(F, C) || R || A || PH(M)), and interpret the
            // 64-octet digest as a little-endian integer k.

            // 5.  Compute S = (r + k * s) mod L.For efficiency, again reduce k
            // modulo L first.

            // 6.  Form the signature of the concatenation of R (32 octets) and the
            // little-endian encoding of S(32 octets; the three most
            // significant bits of the final octet are always zero).


            var (r, Rs) = PreSign(message, domain);
            var k = PublicPoint.GetK(domain, Rs, message);
            var S = Sign(k, r);
            return EncodeSignature(Rs, S);
            }


        public abstract (BigInteger, byte[]) PreSign(byte[] message, byte[] context = null);

        public abstract BigInteger Sign(BigInteger k, BigInteger r);

        public abstract byte[] EncodeSignature(byte[] r, BigInteger scalar);




        }



    /// <summary>
    /// Specifies the curve parameters for a Montgomery or Edwards Elliptic Curve
    /// v^2 = u^3 + A*u^2 + u
    /// </summary>
    public struct DomainParameters : IEquatable<DomainParameters> {

        /// <summary>The prime field</summary>
        public BigInteger P { get; }

        /// <summary>The number of bits in the prime field</summary>
        public int Bits { get; }

        /// <summary>The Montgomery curve coeffient A</summary>
        public BigInteger A { get; }

        /// <summary>The Montgomery curve coeffient constant = (A-2) / 4</summary>
        public BigInteger A24 { get; }

        /// <summary>The Edwards curve coeffient D</summary>
        public BigInteger D { get; }

        /// <summary>The Montgomery initial point U value</summary>
        public BigInteger U { get; }

        /// <summary>The Montgomery initial point V value</summary>
        public BigInteger V { get; }

        /// <summary>The Edwards curve generator Y coordinate</summary>
        public BigInteger By { get; }

        /// <summary>Square root of -1 mod p.</summary>
        public BigInteger SqrtMinus1 { get; }

        ///<summary>The small order subgroup q</summary>
        public BigInteger Q { get; }

        #region // Equality testing bollocks
        /// <summary>Test to see if the domain parameters are equal.</summary>
        /// <param name="other">The parameters to test against</param>
        /// <returns>True if the parameters are equal, otherwise false.</returns>
        public override bool Equals(object other) {
            if (!(other is DomainParameters)) {
                return false;
                }

            return Equals((DomainParameters)other);
            }

        /// <summary>Test to see if the domain parameters are equal.</summary>
        /// <param name="other">The parameters to test against</param>
        /// <returns>True if the parameters are equal, otherwise false.</returns>
        public bool Equals(DomainParameters other) => this == other;

        /// <summary>Test to see if the domain parameters are equal.</summary>
        /// <param name="p1">First value to test</param>
        /// <param name="p2">Second value to test</param> 
        /// <returns>True if the parameters are equal, otherwise false.</returns>
        public static bool operator ==(DomainParameters p1, DomainParameters p2) => (p1.P == p2.P);

        /// <summary>Test to see if the domain parameters are not equal.</summary>
        /// <param name="p1">First value to test</param>
        /// <param name="p2">Second value to test</param> 
        /// <returns>False if the parameters are equal, otherwise true.</returns>
        public static bool operator !=(DomainParameters p1, DomainParameters p2) => !(p1 == p2);

        /// <summary>
        /// Return a hash code. 
        /// </summary>
        /// <returns>Hash of the prime value.</returns>
        public override int GetHashCode() {
            return P.GetHashCode();
            }
        #endregion




        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="p">The prime.</param>
        /// <param name="a">The curve coefficient.</param>
        /// <param name="u">The Montgomery initial point U value.</param>
        /// <param name="v">The Montgomery initial point V value.</param>
        /// <param name="d">The Edwards curve coeffient D</param>
        /// <param name="by">The Edwards curve base point Y value.</param>
        /// <param name="bits">The number of bits in the prime.</param>
        /// <param name="q">The small order subgroup q</param>
        public DomainParameters(BigInteger p, BigInteger a, BigInteger u, BigInteger v, BigInteger d,
                        BigInteger by, int bits, BigInteger q) {
            P = p;
            A = a;
            U = u;
            V = v;
            D = d;
            By = by;
            A24 = (a - 2) / 4;
            Bits = bits;
            Q = q;
            SqrtMinus1 = p.SqrtMinus1();
            }

        static readonly BigInteger Curve25519Prime = BigInteger.Pow(2, 255) - 19;

        static readonly BigInteger CurveConstant25519 = (
            -121665 * (121666.ModularInverse(Curve25519Prime))).Mod(Curve25519Prime);

        static readonly BigInteger Curve25519BaseY = (4 * 5.ModularInverse(Curve25519Prime)) % Curve25519Prime;

        static readonly BigInteger Curve25519BaseV = (
            "1478161944758954479102059356840998688726" +
            "4606134616475288964881837755586237401").DecimalToBigInteger();

        static readonly BigInteger Curve25519Q =
            BigInteger.Pow(2, 252) + "27742317777372353535851937790883648493".DecimalToBigInteger();

        /// <summary>
        /// Montgomery curve parameters for Curve25519S
        /// </summary>
        public static readonly DomainParameters Curve25519 = new DomainParameters(
                Curve25519Prime,
                486662,
                9,
                Curve25519BaseV,
                CurveConstant25519,
                Curve25519BaseY,
                255,
                Curve25519Q);

        static readonly BigInteger Curve448BaseY = (
            "298819210078481492676017930443930673437544040154080242095928241" +
            "372331506189835876003536878655418784733982303233503462500531545062" +
            "832660").DecimalToBigInteger();

        static readonly BigInteger Curve448BaseV = (
            "355293926785568175264127502063783334808976399387714271831880898" +
            "435169088786967410002932673765864550910142774147268105838985595290" +
            "606362").DecimalToBigInteger();

        ///<summary>The small order subgroup q</summary>
        public static readonly BigInteger Curve448Q =
            BigInteger.Pow(2, 446) -
            "13818066809895115352007386748515426880336692474882178609894547503885".DecimalToBigInteger();



        /// <summary>
        /// Montgomery parameters for Curve 448
        /// </summary>
        public static readonly DomainParameters Curve448 = new DomainParameters(
                BigInteger.Pow(2, 448) - BigInteger.Pow(2, 224) - 1,
                156326,
                5,
                Curve448BaseV,
                - 39081,
                Curve448BaseY,
                448,
                Curve448Q);




        }

    }
