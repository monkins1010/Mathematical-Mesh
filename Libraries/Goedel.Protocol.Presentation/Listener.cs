﻿//  Copyright © 2021 by Threshold Secrets Llc.
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

using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Goedel.Cryptography;
using Goedel.Cryptography.Dare;
using Goedel.Utilities;
using Goedel.Protocol;
using System.Threading.Tasks;


namespace Goedel.Protocol.Presentation {

    /// <summary>
    /// Port identifier. Specifies an IP address and port number.
    /// </summary>
    public record PortId {
        ///<summary>The IP address.</summary> 
        public IPAddress IPAddress;

        ///<summary>The port number.</summary> 
        public int Port;
        }

    /// <summary>
    /// Port history. Used to track possible abuse.
    /// </summary>
    public record PortHistory {
        ///<summary>Time at which the last challenge was issued.</summary> 
        public DateTime LastChallenge;

        ///<summary>Number of challenges issued.</summary> 
        public int Challenges;

        ///<summary>Number of refusals made.</summary> 
        public int Refusals;

        /// <summary>
        /// Constructor, initialize the last challenge time to now.
        /// </summary>
        public PortHistory() => LastChallenge = DateTime.Now;
        }



    /// <summary>
    /// Base class for presentation listeners.
    /// </summary>
    public abstract partial class Listener : Disposable {
        #region // Properties
        ///<summary>Private credential of self.</summary> 
        public virtual Credential CredentialSelf { get; }

        #endregion
        #region // Constructors

        ///<summary>Dictionary mapping inbound source Ids to sessions.</summary> 
        public Dictionary<StreamId, SessionResponder> DictionarySessionsInbound = new();

        /// <summary>
        /// Base constructor, populate the common properties.
        /// </summary>
        /// <param name="credentialSelf">The credential used by the listener.</param>
        public Listener(Credential credentialSelf) => CredentialSelf = credentialSelf;
        
        
        
        #endregion

        /// <summary>
        /// Create a challenge value over the packet <paramref name="packetRequest"/> and
        /// payload <paramref name="payload"/> and return as a list of packet extensions.
        /// </summary>
        /// <param name="packetRequest">The packet request.</param>
        /// <param name="payload">The payload.</param>
        /// <returns>List of challenge tokens.</returns>
        public abstract List<PacketExtension> MakeChallenge(
            Packet packetRequest,
            byte[] payload = null);

        /// <summary>
        /// Verify the challenge data in <paramref name="packetRequest"/> returning true if
        /// verification succeeds, false otherwise.
        /// </summary>
        /// <param name="packetRequest">The packet to be validated.</param>
        /// <returns>True if challenge was valid, otherwise false.</returns>
        public abstract bool VerifyChallenge(
            Packet packetRequest);

        /// <summary>
        /// Accept the inbound connection request described in <paramref name="packetRequest"/>.
        /// </summary>
        /// <param name="packetRequest">Parsed inbound request packet.</param>
        /// <returns>The host connection. This may be used to wait for inbound requests from the 
        /// connection.</returns>
        public abstract SessionResponder Accept(
                    Packet packetRequest);

        /// <summary>
        /// Defer creation of a host connection by sending a challenge to the source.
        /// </summary>
        /// <param name="packetRequest">Parsed inbound request packet.</param>
        public virtual SessionResponder GetTemporaryResponder(
                    Packet packetRequest) => throw new NYI();



        }
    }
