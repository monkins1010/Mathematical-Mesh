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

using Goedel.Cryptography;

namespace Goedel.Protocol.Presentation;


/// <summary>
/// Constants class
/// </summary>
public static partial class PresentationConstants {
    #region // Properties
    ///<summary>The minimum packet size.</summary> 
    public const int MinimumPacketSize = 1200;


    ///<summary>The number of bytes reserved for the initial stream identifier (all zeros)</summary> 
    public const int SizeReservedInitialStreamId = 16;

    ///<summary>Size of packet nonce to be used in AES-GCM packet.</summary> 
    public const int SizeNonceAesGcm = 16;
    ///<summary>Size of initialization vector / AES nonce to be used in AES-GCM packet.</summary> 
    public const int SizeIvAesGcm = 12;
    ///<summary>Size of authentication tag to be used in AES-GCM packet.</summary> 
    public const int SizeTagAesGcm = 16;
    ///<summary>Size of key to be used in AES-GCM packet.</summary> 
    public const int SizeKeyAesGcm = 32;

    ///<summary>The KDF info tag to be used to derive initialization vectors.</summary> 
    public readonly static byte[] TagIv = "IV".ToUTF8();

    ///<summary>The KDF info tag to be used to derive keys.</summary> 
    public readonly static byte[] TagKey = "Key".ToUTF8();

    ///<summary>Fixed constant containing the reserved client initial stream identifier.</summary> 
    public readonly static byte[] StreamIdClientInitial = new byte[PresentationConstants.SizeReservedInitialStreamId];

    ///<summary>The KDF info tag to be used to derive keys.</summary> 
    public readonly static byte[] ByteKeyInitiatorResponder = TagKeyInitiatorResponder.ToUTF8();

    ///<summary>The KDF info tag to be used to derive keys.</summary> 
    public readonly static byte[] ByteKeyResponderInitiator = TagKeyResponderInitiator.ToUTF8();
    #endregion
    #region // Methods 

    /// <summary>
    /// Using the primary key <paramref name="ikm"/> and generated nonce <paramref name="nonce"/>,
    /// derive key <paramref name="key"/> and initialization vector <paramref name="iv"/>.
    /// </summary>
    /// <param name="ikm">The primary key.</param>
    /// <param name="nonce">The generated nonce.</param>
    /// <param name="iv">The generated initialization vector.</param>
    /// <param name="key">The generated key.</param>
    public static void Derive(byte[] ikm, out byte[] nonce, out byte[] iv, out byte[] key) {
        nonce = Platform.GetRandomBytes(SizeNonceAesGcm);
        nonce[0] |= 0b1000_0000;

        Derive2(ikm, nonce, out iv, out key);
        }

    /// <summary>
    /// Using the primary key <paramref name="ikm"/> and provided nonce <paramref name="nonce"/>,
    /// derive key <paramref name="key"/> and initialization vector <paramref name="iv"/>.
    /// </summary>
    /// <param name="ikm">The primary key.</param>
    /// <param name="nonce">The nonce.</param>
    /// <param name="iv">The generated initialization vector.</param>
    /// <param name="key">The generated key.</param>
    public static void Derive2(byte[] ikm, byte[] nonce, out byte[] iv, out byte[] key) {
        // Performance: refactor HKDF to avoid need to copy nonce?

        var keyDerive = new KeyDeriveHKDF(ikm, nonce);
        iv = keyDerive.Derive(TagIv, SizeIvAesGcm);
        key = keyDerive.Derive(TagKey, SizeKeyAesGcm);
        }
    #endregion
    }
