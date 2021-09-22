﻿//  © 2021 by Phill Hallam-Baker
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

using System;
using System.Collections.Generic;

using Goedel.Cryptography;
using Goedel.Utilities;


namespace Goedel.Protocol.Presentation {

    /// <summary>
    /// Connection credentialed by means of raw key alone.
    /// </summary>
    public class KeyCredentialPublic : ICredentialPublic {
        #region // Properties

        ///<inheritdoc/>
        public KeyPairAdvanced AuthenticationPublic { get; private set; }

        ///<inheritdoc/>
        public string Account { get; init ; }

        public string Provider => throw new NotImplementedException();

        public string AuthenticationKeyId => AuthenticationPublic.KeyIdentifier;

        public CredentialValidation CredentialValidation => throw new NotImplementedException();
        #endregion
        #region // Constructors

        /// <summary>
        /// Create a new credential from a raw key specified in <paramref name="packetExtension"/>
        /// </summary>
        /// <param name="packetExtension">The packet extension specifying the key</param>
        public KeyCredentialPublic(PacketExtension packetExtension) {
            throw new NYI();
            }

        /// <summary>
        /// Create a new instance with the public key <paramref name="authenticationPublic"/>
        /// </summary>
        /// <param name="authenticationPublic">The public key.</param>
        public KeyCredentialPublic(KeyPairAdvanced authenticationPublic) {
            AuthenticationPublic = authenticationPublic;
            }



        #endregion
        #region // Implement Interface: ICredentialPrivate

        ///<inheritdoc/>
        public (KeyPairAdvanced, KeyPairAdvanced) SelectKey() => throw new NotImplementedException();
        
        ///<inheritdoc/>
        public (KeyPairAdvanced, KeyPairAdvanced) SelectKey(List<KeyPairAdvanced> ephemerals, string keyId) => throw new NotImplementedException();
        #endregion
        }

    /// <summary>
    /// Private key credential specifying only a raw key.
    /// </summary>
    public class KeyCredentialPrivate : KeyCredentialPublic, ICredentialPrivate {
        #region // Properties

        string Tag { get; }


        KeyPairAdvanced AuthenticationPrivate { get; }
        #endregion

        #region // Destructor
        #endregion

        #region // Constructors

        /// <summary>
        /// Create a new instance with the private key <paramref name="authenticationPrivate"/>
        /// </summary>
        /// <param name="authenticationPrivate">The private key.</param>
        public KeyCredentialPrivate(KeyPairAdvanced authenticationPrivate) : 
                    base (authenticationPrivate) {
            AuthenticationPrivate = authenticationPrivate;
            Tag = authenticationPrivate switch {

                KeyPairX448 => Constants.ExtensionTagsX448Tag,
                //KeyPairX25519 => Constants.ExtensionTagsX25519Tag,
                _ => throw new NYI()
                };

            }

        #endregion

        #region // Implement Interface: ICredentialPrivate
        public void AddCredentials(List<PacketExtension> extensions) => throw new NotImplementedException();
        public void AddEphemerals(List<PacketExtension> extensions, ref List<KeyPairAdvanced> ephmeralsOffered) => throw new NotImplementedException();
        public ICredentialPublic GetCredentials(List<PacketExtension> extensions) => throw new NotImplementedException();
        public (KeyPairAdvanced, KeyPairAdvanced) SelectKey(List<PacketExtension> extensions) => throw new NotImplementedException();
        public (KeyPairAdvanced, KeyPairAdvanced) SelectKey(string keyId, byte[] ephemeral) => throw new NotImplementedException();


        #endregion



        }
    }
