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
using System;
using System.Security.Cryptography;
using System.Text;

using Goedel.Utilities;

namespace Goedel.Cryptography.KeyFile {

    /// <summary>
    /// Recognized key file formats
    /// </summary>
    public enum KeyFileFormat {
        /// <summary>Default format according to context.</summary>
        Default,
        /// <summary>PEM private key file, used for many SSH implementations</summary>
        PEMPrivate,
        /// <summary>PEM public key file</summary>
        PEMPublic,
        /// <summary>PuTTY private key file</summary>
        PuTTY,
        /// <summary>OpenSSH native format</summary>
        OpenSSH,
        /// <summary>X509</summary>
        X509DER,
        /// <summary>PKCS 12 / PFX</summary>
        PKCS12,
        /// <summary>PKCS 7</summary>
        PKCS7
        }

    /// <summary>Extension methods</summary>
    /// <remarks>This currently hard wires to the .NET framework providers
    /// rather than the portable base classes.</remarks>
    public static class Extension {
        /// <summary>
        /// Convert key pair to specified format
        /// </summary>
        /// <param name="KeyPair">Keypair to convert</param>
        /// <param name="KeyFileFormat">Format to convert to</param>
        /// <param name="passphrase">Optional passphrase to be used as encryption key.</param>
        /// <returns>The keyfile data</returns>
        public static string ToKeyFile(this KeyPair KeyPair, KeyFileFormat KeyFileFormat,
                string passphrase = null) {




            switch (KeyFileFormat) {
                case KeyFileFormat.PEMPrivate: {
                    return ToPEMPrivate(KeyPair, passphrase);
                    }
                case KeyFileFormat.PEMPublic: {
                    return ToPEMPublic(KeyPair);
                    }
                case KeyFileFormat.PuTTY: {
                    return ToPuTTY(KeyPair);
                    }
                case KeyFileFormat.OpenSSH: {
                    return ToOpenSSH(KeyPair);
                    }

                case KeyFileFormat.Default:
                break;
                case KeyFileFormat.X509DER:
                break;
                case KeyFileFormat.PKCS12:
                break;
                case KeyFileFormat.PKCS7:
                break;
                default:
                break;
                }
            return null;
            }

        /// <summary>
        /// Convert key pair to OpenSSH format
        /// </summary>
        /// <param name="KeyPair">Key pair to convert</param>
        /// <returns>The keyfile data</returns>
        public static string ToOpenSSH(this KeyPair KeyPair) {
            if (KeyPair is KeyPairBaseRSA RSAKeyPair) {
                return ToOpenSSH(RSAKeyPair);
                }

            //switch (KeyPair) {
            //    case RSAKeyPair RSAKeyPair:
            //        return ToOpenSSH(RSAKeyPair);
            //    }
            return null;
            }

        /// <summary>
        /// Convert key pair to PEMPrivate format
        /// </summary>
        /// <param name="KeyPair">Key pair to convert</param>
        /// <param name="passphrase">Optional passphrase to be used as encryption key.</param>
        /// <returns>The keyfile data</returns>
        public static string ToPEMPrivate(this KeyPair KeyPair, string passphrase=null) {

            passphrase.AssertNull(NYI.Throw);

            if (KeyPair is KeyPairBaseRSA RSAKeyPair) {
                return ToPEMPrivateRSA(RSAKeyPair, passphrase);
                }

            //switch (KeyPair) {
            //    case RSAKeyPair RSAKeyPair:
            //    return ToPEMPrivate(RSAKeyPair);
            //    }
            return null;
            }

        /// <summary>
        /// Convert key pair to PEMPublic format
        /// </summary>
        /// <param name="KeyPair">Key pair to convert</param>
        /// <returns>The keyfile data</returns>
        public static string ToPEMPublic(this KeyPair KeyPair) {
            if (KeyPair is KeyPairBaseRSA RSAKeyPair) {
                return ToPEMPublic(RSAKeyPair);
                }

            //switch (KeyPair) {
            //    case RSAKeyPair RSAKeyPair:
            //    return ToPEMPublic(RSAKeyPair);
            //    }
            return null;
            }

        /// <summary>
        /// Convert key pair to PuTTY format
        /// </summary>
        /// <param name="KeyPair">Key pair to convert</param>
        /// <returns>The keyfile data</returns>
        public static string ToPuTTY(this KeyPair KeyPair) {
            if (KeyPair is KeyPairBaseRSA RSAKeyPair) {
                return ToPuTTY(RSAKeyPair);
                }

            //switch (KeyPair) {
            //    case RSAKeyPair RSAKeyPair:
            //    return ToPuTTY(RSAKeyPair);
            //    }
            return null;
            }

        /// <summary>
        /// Convert key pair to PEM formatted string.
        /// </summary>
        /// <param name="RSAKeyPair">A  Key pair</param>
        /// <returns>Key Pair in PEM format</returns>
        public static string ToPuTTY(this KeyPairBaseRSA RSAKeyPair) => null;

        /// <summary>
        /// Convert key pair to OpenSSH formatted string.
        /// </summary>
        /// <param name="RSAKeyPair">A  Key pair</param>
        /// <param name="Tag">Tag to label key with</param>
        /// <returns>Key Pair in PEM format</returns>
        public static string ToOpenSSH(this KeyPairBaseRSA RSAKeyPair, string Tag = null) {
            var SSH_RSA = new SSH_RSA(RSAKeyPair);
            var Data = SSH_RSA.Encode();

            var Builder = new StringBuilder();
            Builder.Append("ssh-rsa ");
            Builder.ToStringBase64(Data, 0, Data.Length);
            Builder.Append(" ");
            Builder.Append(Tag ?? "");
            return Builder.ToString();
            }

        /// <summary>
        /// Convert key pair to PEM formatted string.
        /// </summary>
        /// <param name="RSAKeyPair">A  Key pair</param>
        /// <returns>Key Pair in PEM format</returns>
        public static string ToPEMPublic(this KeyPairBaseRSA RSAKeyPair) => null;

        /// <summary>
        /// Convert key pair to PEM formatted string.
        /// </summary>
        /// <param name="RSAKeyPair">An RSA Key pair</param>
        /// <param name="passphrase">Optional passphrase to be used as encryption key.</param>
        /// <returns>Key Pair in PEM format</returns>
        public static string ToPEMPrivateRSA(KeyPairBaseRSA RSAKeyPair, string passphrase = null) {
            //throw new NYI();
            //RSACryptoServiceProvider Provider = RSAKeyPair.Provider;
            //Assert.NotNull(Provider, NoProviderSpecified.Throw);

            //var RSAParameters = Provider.ExportParameters(true);
            //Assert.NotNull(RSAParameters, PrivateKeyNotAvailable.Throw);

            //var NewProvider = new RSACryptoServiceProvider();
            //NewProvider.ImportParameters(RSAParameters);


            ////RSAParameters.Dump();

            //var RSAPrivateKey = RSAParameters.RSAPrivateKey();
            passphrase.AssertNull(NYI.Throw);
            var RSAPrivateKey = RSAKeyPair.PKIXPrivateKey;

            var Builder = new StringBuilder();

            Builder.Append("-----BEGIN RSA PRIVATE KEY-----");
            var KeyDER = RSAPrivateKey.DER();
            Builder.ToStringBase64(KeyDER);
            Builder.Append("\n-----END RSA PRIVATE KEY-----\n");

            return Builder.ToString();
            }

        /// <summary>
        /// Debug utility
        /// </summary>
        /// <param name="RSAParameters">RSA Parameters in /NET format</param>
        public static void Dump(this RSAParameters RSAParameters) {
            RSAParameters.Modulus.Dump("Modulus");
            RSAParameters.Exponent.Dump("Exponent");
            RSAParameters.P.Dump("P");
            RSAParameters.Q.Dump("Q");
            RSAParameters.D.Dump("D");
            RSAParameters.DP.Dump("DP");
            RSAParameters.DQ.Dump("DQ");
            RSAParameters.InverseQ.Dump("InverseQ");
            }

        /// <summary>
        /// Debug output utility
        /// </summary>
        /// <param name="Data">Data to print</param>
        /// <param name="Tag">Tag to prepend to data</param>
        public static void Dump(this byte[] Data, string Tag) => Console.WriteLine("{0} : [{1}]  {2}.{3}.{4} ... {5}.{6}.{7}",
                Tag, Data.Length, Data[0], Data[1], Data[2],
                    Data[^3], Data[^2], Data[^1]);
        }
    }
