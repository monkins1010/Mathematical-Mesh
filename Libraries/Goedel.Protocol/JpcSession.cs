﻿//  Copyright © 2015 by Comodo Group Inc.
//  Copyright © 2021 Threshold Secrets Llc
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

using System.Collections.Generic;
using System.IO;

using Goedel.Utilities;

namespace Goedel.Protocol {


    /// <summary>
    /// Enumeration describing the different connection modes
    /// </summary>
    public enum JpcConnection {
        ///<summary>The client makes a direct call to the service API.</summary> 
        Direct,
        ///<summary>The client makes a direct call to the service through the JSON
        ///serialization/deserialization interfaces.</summary> 
        Serialized,
        ///<summary>The client makes a remote call to the service by means of a 
        ///HTTP POST method containing a JSON serialization.</summary> 
        Http,
        ///<summary>The client makes a remote call to the service using a ticket 
        ///previously issued by the service by means of either the HTTP or the UDP 
        ///transport.</summary> 
        Ticketed

        }

    public delegate JpcSession JpcSessionFactoryDelegate(
                JpcCredential jpcCredential);




    public abstract class JpcCredential {

        ///<summary>The account address (Account@Domain or @callsign)</summary>
        public string AccountAddress { get; }

        ///<summary>The account portion of <see cref="AccountAddress"/></summary>
        public string Account { get; }

        ///<summary>The domain portion of <see cref="AccountAddress"/></summary>
        public string Domain { get; }


        public static JpcSessionFactoryDelegate GetSessionSerialized => throw new NYI();
        public static JpcSessionFactoryDelegate GetSessionHttp => throw new NYI();
        public static JpcSessionFactoryDelegate GetSessionTicketed => throw new NYI();

        public JpcCredential(string accountAddress) {
            AccountAddress = accountAddress;
            }


        public virtual JpcSession GetJpcSession(JpcConnection jpcConnection) =>

            jpcConnection switch {
                JpcConnection.Direct => new JpcSessionDirect(AccountAddress),
                JpcConnection.Serialized => GetSessionSerialized(this),
                JpcConnection.Http => GetSessionHttp(this),
                JpcConnection.Ticketed => GetSessionTicketed(this),
                _ => throw new NYI()
                };

        }

    //public class JpcCredentialDirect : JpcCredential {
    //    public JpcDispatch Host { get; }

    //    public JpcCredentialDirect(JpcDispatch host, string accountAddress) :
    //                base(accountAddress) {
    //        Host = host;
    //        }


    //    public override JpcSession GetJpcSession(JpcConnection jpcConnection) =>

    //jpcConnection switch {
    //    JpcConnection.Direct => new DirectSession(AccountAddress),
    //    JpcConnection.Serialized => GetSessionSerialized(this),
    //    _ => throw new NYI()
    //    };


    //    }



    /// <summary>
    /// The session class describes the caller of a method.
    /// </summary>
    public abstract class JpcSession {

        ///<summary>The service identifier (Account@Domain)</summary>
        public string AccountAddress;


        ///<summary>The account portion of <see cref="AccountAddress"/></summary>
        public string Account;

        ///<summary>The domain portion of <see cref="AccountAddress"/></summary>
        public string Domain;

        /// <summary>
        /// Fingerprint of authentication key
        /// </summary>
        public string UDF;

        /// <summary>
        /// If true we have an authentication structure.
        /// </summary>
        public bool Authenticated;

        /// <summary>
        /// VerifiedAccount instance describing the verified account details. 
        /// </summary>
        public virtual VerifiedAccount VerifiedAccount => !Authenticated ? null :
            new VerifiedAccount() { AccountAddress = AccountAddress };



        /// <summary>
        /// Constructor for a session with service <paramref name="accountAddress"/>.
        /// </summary>
        /// <param name="accountAddress">The name of the service (e.g. example.com) or an account 
        /// at the service (e.g. alice@example.com).</param>
        public JpcSession(string accountAddress) {
            AccountAddress = accountAddress;
            accountAddress.SplitAccountIDService(out Domain, out Account);
            }


        /// <summary>
        /// Factory method returning a new JPC interface calling a service of
        /// type <typeparamref name="T"/> with an initial <see cref="JpcSession"/> binding of
        /// the calling instance. This binding MAY be updated through interaction with the 
        /// service, e.g. to replace a HTTP/JSON binding authenticated by means of a direct
        /// key exchange with a key exchange established in a referenced token.
        /// </summary>
        /// <typeparam name="T">Type of the instance to return.</typeparam>
        /// <returns></returns>
        public T GetWebClient<T>() where T : JpcInterface, new() => new T {
            JpcSession = this
            };



        }

    /// <summary>
    /// Direct connection between client and service host. Useful for debugging
    /// and for direct access to a service on the same machine.
    /// </summary>
    public partial class JpcSessionDirect : JpcSession {

        /// <summary>
        /// Create a direct session for the specified account.
        /// </summary>
        /// <param name="accountAddress">The account name</param>
        public JpcSessionDirect(string accountAddress) : base(accountAddress) => Authenticated = true;


        }



    /// <summary>
    /// Direct connection between client and service host. Useful for debugging
    /// and for direct access to a service on the same machine.
    /// </summary>
    public abstract partial class JpcRemoteSession : JpcSession {


        /// <summary>
        /// Create a direct session for the specified account.
        /// </summary>
        /// <param name="accountAddress">The account name</param>
        public JpcRemoteSession(string accountAddress) : base(accountAddress) {
            }

        ///// <summary>
        ///// Set the authentication key for use with the session
        ///// </summary>
        ///// <param name="UDF">Fingerprint of the authentication key.</param>
        ///// <returns>True is successful. Otherwise, false.</returns>
        //public override bool Authenticate(string UDF) {
        //    this.UDF = UDF;
        //    return false;
        //    }

        /// <summary>
        /// Post the specified data to the remote service.
        /// </summary>
        /// <param name="Data">Input data</param>
        /// <param name="request">The request</param>
        /// <returns>The response data</returns>
        public abstract Stream Post(MemoryStream Data, JsonObject request);

        /// <summary>
        /// Construct a Post string.
        /// </summary>
        /// <param name="Tag">Operation to perform.</param>
        /// <param name="request">Request data.</param>
        /// <returns>string returned in response.</returns>
        public virtual string Post(string Tag, JsonObject request) {

            var Buffer = new MemoryStream();
            var JSONWriter = new JSONWriter(Buffer);

            // Wrap the request object with the transaction name.
            JSONWriter.WriteObjectStart();
            JSONWriter.WriteToken(Tag, 0);
            request.Serialize(JSONWriter, false);
            JSONWriter.WriteObjectEnd();

            // Send the request
            var ResponseBuffer = Post(Buffer, request);

            return ResponseBuffer.GetUTF8();
            }


        public virtual JsonObject Post(string Tag, string TagResponse, JsonObject request) {

            throw new NYI();
            }

        }

    /// <summary>
    /// Direct connection between client and service host with messages 
    /// encoded and decoded from JSON. For use in debugging issues that
    /// might be the result of JSON encoding issues and to collect samples
    /// for documentation.
    /// </summary>
    public partial class JpcSessionSerialized : JpcRemoteSession {
        /// <summary>
        /// The provider.
        /// </summary>
        protected JpcInterface Host;


        /// <summary>
        /// Create a remote session with authentication under the
        /// specified credential.
        /// </summary>
        /// <param name="Host">The host implementation</param>
        /// <param name="accountAddress">The service account.</param>
        public JpcSessionSerialized(JpcInterface Host, string accountAddress) : base(accountAddress) =>
                this.Host = Host;

        /// <summary>
        /// Post a request and retrieve the response.
        /// </summary>
        /// <param name="Data">StreamBuffer object containing JSON encoded request.</param>
        /// <param name="requestObject">The request object.</param>
        /// <returns>StreamBuffer object containing JSON encoded response.</returns>
        public override Stream Post(MemoryStream Data, JsonObject requestObject) {

            var DataText = Data.GetUTF8();
            var JSONReader = new JsonReader(DataText);

            var result = Host.Dispatch(this, JSONReader);
            return new MemoryStream(result.GetBytes());
            }

        }
    public partial class JpcSessionHTTP : JpcRemoteSession {
        public JpcSessionHTTP(string discovery, string domain, string account) :
                base (account){
            }

        public override Stream Post(MemoryStream Data, JsonObject request) => throw new System.NotImplementedException();
        }

    public partial class JpcSessionTicketed : JpcRemoteSession {
        public JpcSessionTicketed(JpcTicket ticket, string account) :
                base(account) {
            }
        public override Stream Post(MemoryStream Data, JsonObject request) => throw new System.NotImplementedException();

        }

    }

