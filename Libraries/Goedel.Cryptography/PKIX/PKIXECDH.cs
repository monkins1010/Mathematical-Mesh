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


namespace Goedel.Cryptography.PKIX;


/// <summary>
/// PKIXPublicKeyECDH 
/// </summary>
public abstract partial class PKIXPublicKeyECDH : Goedel.ASN.ByteArrayVerbatim,
                IPkixPublicKey, IKeyPublicECDH {

    /// <summary>The Jose curve identifier.</summary>
    public abstract string CurveJose { get; }

    /// <summary>
    /// Construct a PKIX SubjectPublicKeyInfo block
    /// </summary>
    /// <param name="oidValue">The OID value</param>
    /// <returns>The PKIX structure</returns>
    public SubjectPublicKeyInfo SubjectPublicKeyInfo(int[] oidValue = null) {
        oidValue ??= OID;
        return new SubjectPublicKeyInfo(oidValue, DER());
        }

    /// <summary>Empty constructor for deserialization operations.</summary>
    public PKIXPublicKeyECDH() {
        }

    /// <summary>
    /// Create PKIX representation from the encoded values.
    /// </summary>
    /// <param name="data"></param>
    public PKIXPublicKeyECDH(byte[] data) => Data = data.Duplicate();


    /// <summary>
    /// Encode ASN.1 class members to specified buffer. 
    ///
    /// NB Assinine ASN.1 DER encoding rules requires members be added in reverse order.
    /// </summary>
    /// <param name="buffer">Output buffer</param>
    public override void Encode(Goedel.ASN.Buffer buffer) => buffer.Encode__Octets(Data, 0, -1);

    /// <summary>Return the corresponding public parameters</summary>
    public IPkixPublicKey PublicParameters => this;

    }

/// <summary>
/// PKIXPrivateKeyECDH 
/// </summary>
public abstract partial class PKIXPrivateKeyECDH : Goedel.ASN.ByteArrayVerbatim,
                    IPKIXPrivateKey, IKeyPrivateECDH {

    /// <summary>The Jose curve identifier.</summary>
    public abstract string CurveJose { get; }


    /// <summary>If true, this is a recryption key.</summary>
    public bool IsRecryption { get; set; } = false;

    /// <summary>
    /// Empty constructor for deserialization operations.
    /// </summary>
    public PKIXPrivateKeyECDH() {
        }

    /// <summary>
    /// Create PKIX representation from the encoded values.
    /// </summary>
    /// <param name="data">The private key data as an octet string</param>
    /// <param name="public">The public key representation.</param>
    public PKIXPrivateKeyECDH(byte[] data, PKIXPublicKeyECDH @public) {
        this.Data = data.Duplicate();
        PKIXPublicKeyECDH = @public;
        }


    /// <summary>
    /// Construct a PKIX SubjectPublicKeyInfo block
    /// </summary>
    /// <param name="oidValue">The OID value</param>
    /// <returns>The PKIX structure</returns>
    public SubjectPublicKeyInfo SubjectPublicKeyInfo(int[] oidValue = null) {
        oidValue ??= OID;
        return new SubjectPublicKeyInfo(oidValue, DER());
        }


    /// <summary>
    /// Return the corresponding public parameters
    /// </summary>
    public IPkixPublicKey PublicParameters => PKIXPublicKeyECDH;


    /// <summary>
    /// Return the corresponding public parameters
    /// </summary>
    public PKIXPublicKeyECDH PKIXPublicKeyECDH { get; }

    }


#region // Edwards Curves
/// <summary>
/// PKIXPrivateKeyECDH 
/// </summary>
public partial class PKIXPublicKeyEd25519 : PKIXPublicKeyECDH {

    /// <summary>
    /// The Jose curve identifier (Ed25519);
    /// </summary>
    public override string CurveJose => CurveEdwards25519.CurveJose;

    /// <summary>
    /// Return the algorithm identifier that represents this key
    /// </summary>
    public override int[] OID => Constants.OID__id_Ed25519;

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPublicKeyEd25519() {
        }

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPublicKeyEd25519(byte[] data) => Data = data;

    }

/// <summary>
/// PKIXPrivateKeyECDH 
/// </summary>
public partial class PKIXPrivateKeyEd25519 : PKIXPrivateKeyECDH {

    /// <summary>
    /// The Jose curve identifier (Ed25519);
    /// </summary>
    public override string CurveJose => CurveEdwards25519.CurveJose;

    /// <summary>
    /// Return the algorithm identifier that represents this key
    /// </summary>
    public override int[] OID => Constants.OID__id_Ed25519;


    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPrivateKeyEd25519() : base() {
        }

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    /// <param name="data">The private key data as an octet string</param>
    /// <param name="publicKey">The public key representation.</param>
    public PKIXPrivateKeyEd25519(byte[] data, PKIXPublicKeyECDH publicKey) : base(data, publicKey) {
        }

    }

/// <summary>
/// PKIXPrivateKeyECDH 
/// </summary>
public partial class PKIXPublicKeyEd448 : PKIXPublicKeyECDH {

    /// <summary>
    /// The Jose curve identifier (Ed25519);
    /// </summary>
    public override string CurveJose => CurveEdwards448.CurveJose;

    /// <summary>
    /// Return the algorithm identifier that represents this key
    /// </summary>
    public override int[] OID => Constants.OID__id_Ed448;

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPublicKeyEd448() {
        }

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPublicKeyEd448(byte[] data) => Data = data;

    }

/// <summary>
/// PKIXPrivateKeyECDH 
/// </summary>
public partial class PKIXPrivateKeyEd448 : PKIXPrivateKeyECDH {

    /// <summary>
    /// The Jose curve identifier (Ed25519);
    /// </summary>
    public override string CurveJose => CurveEdwards448.CurveJose;

    /// <summary>
    /// Return the algorithm identifier that represents this key
    /// </summary>
    public override int[] OID => Constants.OID__id_Ed448;

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPrivateKeyEd448() {
        }

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPrivateKeyEd448(byte[] data, PKIXPublicKeyECDH publicKey) : base(data, publicKey) {
        }

    }
#endregion
#region // Montgomery Curves
/// <summary>
/// PKIXPrivateKeyECDH 
/// </summary>
public partial class PKIXPublicKeyX25519 : PKIXPublicKeyECDH {

    /// <summary>
    /// The Jose curve identifier (Ed25519);
    /// </summary>
    public override string CurveJose => CurveX25519.CurveJose;

    /// <summary>
    /// Return the algorithm identifier that represents this key
    /// </summary>
    public override int[] OID => Constants.OID__id_X25519;

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPublicKeyX25519() {
        }

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPublicKeyX25519(byte[] data) => Data = data;

    }



/// <summary>
/// PKIXPrivateKeyECDH 
/// </summary>
public partial class PKIXPrivateKeyX25519 : PKIXPrivateKeyECDH {

    /// <summary>
    /// The Jose curve identifier (Ed25519);
    /// </summary>
    public override string CurveJose => CurveX25519.CurveJose;

    /// <summary>
    /// Return the algorithm identifier that represents this key
    /// </summary>
    public override int[] OID => Constants.OID__id_X25519;


    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPrivateKeyX25519() : base() {
        }

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPrivateKeyX25519(byte[] data, PKIXPublicKeyECDH publicKey) : base(data, publicKey) {
        }

    }



/// <summary>
/// PKIXPrivateKeyECDH 
/// </summary>
public partial class PKIXPublicKeyX448 : PKIXPublicKeyECDH {

    /// <summary>
    /// The Jose curve identifier (Ed25519);
    /// </summary>
    public override string CurveJose => CurveX448.CurveJose;

    /// <summary>
    /// Return the algorithm identifier that represents this key
    /// </summary>
    public override int[] OID => Constants.OID__id_X448;

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPublicKeyX448() {
        }

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPublicKeyX448(byte[] data) => Data = data;

    }



/// <summary>
/// PKIXPrivateKeyECDH 
/// </summary>
public partial class PKIXPrivateKeyX448 : PKIXPrivateKeyECDH {

    /// <summary>
    /// The Jose curve identifier (Ed25519);
    /// </summary>
    public override string CurveJose => CurveX448.CurveJose;

    /// <summary>
    /// Return the algorithm identifier that represents this key
    /// </summary>
    public override int[] OID => Constants.OID__id_X448;

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPrivateKeyX448() {
        }

    /// <summary>
    /// Default constructor, create empty structure.
    /// </summary>
    public PKIXPrivateKeyX448(byte[] data, PKIXPublicKeyECDH publicKey) : base(data, publicKey) {
        }

    }
#endregion