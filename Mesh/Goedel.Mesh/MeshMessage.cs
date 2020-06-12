﻿using Goedel.Cryptography;
using Goedel.Protocol;
using Goedel.Cryptography.Dare;
using Goedel.Utilities;

using System.Collections.Generic;
using System;

namespace Goedel.Mesh {


    public partial class Reference {

        ///<summary>Returns the envelope ID corresponding to the MessageID</summary>
        public string EnvelopeID => Message.GetEnvelopeId(MessageID);

        ///<summary>Accessor for the <see cref="Relationship"/> property
        ///as a <see cref="MessageStatus"/> property.</summary>
        public MessageStatus MessageStatus {
            get => Relationship switch
                    {
                        "Open" => MessageStatus.Open,
                        "Closed" => MessageStatus.Closed,
                        "Read" => MessageStatus.Read,
                        "Unread" => MessageStatus.Unread,
                        _ => MessageStatus.None
                        };
            set => Relationship = value switch
                {
                    MessageStatus.Open => "Open",
                    MessageStatus.Closed => "Closed",
                    MessageStatus.Read => "Read",
                    MessageStatus.Unread => "Unread",
                    _ => "Unknown"
                    };
            }


        }

    public partial class Message {


        ///<summary>The message status.</summary>
        public MessageStatus MessageStatus;



        /// <summary>
        /// Encode the message using the signature key <paramref name="signingKey"/>.
        /// </summary>
        /// <param name="signingKey">The signature key.</param>
        /// <param name="encryptionKey">The encryption key.</param>
        /// <returns>The enveloped, signed message.</returns>
        public DareEnvelope Encode(KeyPair signingKey = null, KeyPair encryptionKey=null) {

            MessageID ??= UDF.Nonce(); // Add a message ID unless one is already defined.

            var data = this.GetBytes();
            var contentMeta = new ContentMeta() {
                UniqueID = MessageID,
                Created = DateTime.Now,
                ContentType = Constants.IanaTypeMeshMessage,
                MessageType = _Tag
                };

            DareEnvelope = DareEnvelope.Encode(data, contentMeta: contentMeta, 
                signingKey: signingKey, encryptionKey: encryptionKey);

            DareEnvelope.Header.EnvelopeID = GetEnvelopeId(MessageID);

            return DareEnvelope;
            }

        /// <summary>
        /// Decode <paramref name="envelope"/> and return the inner <see cref="Message"/>
        /// </summary>
        /// <param name="envelope">The envelope to decode.</param>
        /// <param name="keyCollection">Key collection to use to obtain decryption keys.</param>
        /// <returns>The decoded profile.</returns>
        public static new Message Decode(DareEnvelope envelope,
                    IKeyLocate keyCollection = null) =>
                        MeshItem.Decode(envelope, keyCollection) as Message;


        /// <summary>
        /// Compute the EnvelopeID for <paramref name="messageID"/>.
        /// </summary>
        /// <param name="messageID">The message identifier to calculate the envelope 
        /// identifer of</param>
        /// <returns>The envelope identifier.</returns>
        public static string GetEnvelopeId (string messageID) =>
                    UDF.ContentDigestOfUDF(messageID);
        }

    public partial class MessageComplete {

        ///<summary>Constant for the response Accept.</summary>
        public const string Accept = "Accept";

        ///<summary>Constant for the response Reject.</summary>
        public const string Reject = "Reject";

        ///<summary>Constant for the response Read.</summary>
        public const string Read = "Read";

        ///<summary>Constant for the response Unread.</summary>
        public const string Unread = "Unread";

        /// <summary>
        /// Constructor for use by deserializers.
        /// </summary>
        public MessageComplete() { }


        /// <summary>
        /// Constructor for a completion message.
        /// </summary>
        /// <param name="messageID">The message the completion message completes.</param>
        /// <param name="relationship">Relationship to the message.</param>
        /// <param name="responseID">The response code.</param>
        public MessageComplete(
                    string messageID, string relationship, string responseID = null) {
            var reference = new Reference() {
                MessageID = messageID,
                Relationship = relationship,
                ResponseID = responseID
                };
            References = new List<Reference>() { reference };

            }

        /// <summary>
        /// Decode <paramref name="envelope"/> and return the inner <see cref="MessageComplete"/>
        /// </summary>
        /// <param name="envelope">The envelope to decode.</param>
        /// <param name="keyCollection">Key collection to use to obtain decryption keys.</param>
        /// <returns>The decoded profile.</returns>
        public static new MessageComplete Decode(DareEnvelope envelope,
                    IKeyLocate keyCollection = null) =>
                        MeshItem.Decode(envelope, keyCollection) as MessageComplete;



        }

    public partial class MessagePIN {

        /// <summary>
        /// Default constructor used for deserialization.
        /// </summary>
        public MessagePIN() {
            }

        /// <summary>
        /// Construct a <see cref="MessagePIN"/> instance for the PIN value
        /// <paramref name="pin"/> and account address <paramref name="accountAddress"/>
        /// with optional expiry value <paramref name="expires"/>.
        /// </summary>
        /// <param name="pin">The PIN value.</param>
        /// <param name="expires">The expiry time.</param>
        /// <param name="accountAddress">The account address the PIN is issued for.</param>
        public MessagePIN(string pin, DateTime expires, string accountAddress) {
            Account = accountAddress;
            Expires = expires;
            PIN = pin;
            MessageID = RequestConnection.GetPinUDF(pin, accountAddress);

            Console.WriteLine($"Created Pin: {Account} / {PIN} => {MessageID}");
            }

        /// <summary>
        /// Get the 
        /// </summary>
        /// <returns></returns>
        public string GetURI() => MeshUri.ConnectUri(Account, PIN);



        }

    public partial class AcknowledgeConnection {



        ///<summary>Convenience accessor for the inner <see cref="AcknowledgeConnection"/></summary>
        public RequestConnection MessageConnectionRequest => messageConnectionRequest ??
            RequestConnection.Decode(EnvelopedRequestConnection).CacheValue(out messageConnectionRequest);
        RequestConnection messageConnectionRequest;


        /// <summary>
        /// Decode <paramref name="envelope"/> and return the inner <see cref="RespondConnection"/>
        /// </summary>
        /// <param name="envelope">The envelope to decode.</param>
        /// <param name="keyCollection">Key collection to use to obtain decryption keys.</param>
        /// <returns>The decoded profile.</returns>
        public static new AcknowledgeConnection Decode(DareEnvelope envelope,
                    IKeyLocate keyCollection = null) =>
                        MeshItem.Decode(envelope, keyCollection) as AcknowledgeConnection;

        }


    public partial class RequestConnection {


        /// <summary>
        /// Default constructor used for deserialization.
        /// </summary>
        public RequestConnection() {
            }

        /// <summary>
        /// Constructor for a <see cref="RequestConnection"/> instance for connecting the
        /// device <paramref name="profileDevice"/> to the account
        /// <paramref name="accountAddress"/>.
        /// </summary>
        /// <param name="profileDevice">Profile of the device requesting connection.</param>
        /// <param name="accountAddress">The account through which the device is requesting 
        /// a connection.</param>
        /// <param name="pin">Optional PIN value</param>
        /// <param name="clientNonce">Optional client nonce (if null, a nonce will be
        /// generated.</param>
        public RequestConnection(
                ProfileDevice profileDevice,
            string accountAddress,
            string pin=null,
            byte[] clientNonce = null) {
            AccountAddress = accountAddress;
            EnvelopedProfileDevice = profileDevice.DareEnvelope;
            ClientNonce = clientNonce ?? CryptoCatalog.GetBits(128);
            if (pin != null) {
                PinUDF = GetPinUDF(pin, accountAddress);
                PinWitness = GetPinWitness(pin, accountAddress, ClientNonce, profileDevice.UDF);
                }
            }

        /// <summary>
        /// PIN code identifier 
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="accountAddress"></param>
        /// <returns></returns>
        public static string GetPinUDF(
                    string pin, 
                    string accountAddress) => UDF.PinWitnessString(pin, accountAddress.ToUTF8());



        /// <summary>
        /// Witness value calculated as KDF (Device.UDF + AccountAddress+ClientNonce, pin)
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="accountAddress"></param>
        /// <param name="clientNonce"></param>
        /// <param name="deviceUDF"></param>
        /// <returns></returns>
        public static byte[] GetPinWitness(
                    string pin,
                    string accountAddress,
                    byte[] clientNonce,
                    string deviceUDF) {

            //Console.WriteLine($"  {pin} {accountAddress}" +
            //    $"\n{deviceUDF}\n{clientNonce.ToStringBase16FormatHex()}");


            return UDF.PinWitness(pin, accountAddress.ToUTF8(),
                        clientNonce, deviceUDF.ToUTF8());
            }

        /// <summary>
        /// Verify that the witness value is correct for the specified <paramref name="pin"/> and
        /// values of Device UDF and Account Address.
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        public bool Verify(string pin) => throw new NYI();

        ///<summary>Convenience accessor for the inner <see cref="ProfileDevice"/></summary>
        public ProfileDevice ProfileDevice => profileDevice ??
            ProfileDevice.Decode(EnvelopedProfileDevice).CacheValue(out profileDevice);
        ProfileDevice profileDevice;


        /// <summary>
        /// Decode <paramref name="envelope"/> and return the inner <see cref="RequestConnection"/>
        /// </summary>
        /// <param name="envelope">The envelope to decode.</param>
        /// <param name="keyCollection">Key collection to use to obtain decryption keys.</param>
        /// <returns>The decoded profile.</returns>
        public static new RequestConnection Decode(DareEnvelope envelope,
                    IKeyLocate keyCollection = null) =>
                        MeshItem.Decode(envelope, keyCollection) as RequestConnection;



        //public void Authenticate (string pin) => throw new NYI();


        /// <summary>
        /// Verified decoding of the enveloped request <paramref name="envelope"/>
        /// </summary>
        /// <param name="envelope">The envelope to decode.</param>
        /// <returns>The decoded profile (if signature verification succeeds).</returns>
        public static RequestConnection Verify(DareEnvelope envelope) {
            var result = Decode(envelope) as RequestConnection;

            // ToDo: put the verification code in here.


            return result;
            }




        }

    public partial class RespondConnection {



        /// <summary>
        /// Decode <paramref name="envelope"/> and return the inner <see cref="RespondConnection"/>
        /// </summary>
        /// <param name="envelope">The envelope to decode.</param>
        /// <param name="keyCollection">Key collection to use to obtain decryption keys.</param>
        /// <returns>The decoded profile.</returns>
        public static new RespondConnection Decode(DareEnvelope envelope,
                    IKeyLocate keyCollection = null) =>
                        MeshItem.Decode(envelope, keyCollection) as RespondConnection;

        /// <summary>
        /// Validate the RespondConnection message in the context of <paramref name="profileDevice"/>
        /// and <paramref name="keyCollection"/>.
        /// </summary>
        /// <param name="profileDevice">The profile device.</param>
        /// <param name="keyCollection">Key collection to use to obtain decryption keys.</param>
        public void Validate(ProfileDevice profileDevice, IKeyLocate keyCollection) {
            profileDevice.Future();
            keyCollection ??= this.KeyCollection;
            keyCollection.Future();
            
            // Validate the chain for the device to master

            // Profile Master is self Signed
            // Device Profile connection is valid under Profile Master
            // Device Activation for master is valid


            // Foreach Account 
            //  // Validate the chain for the device to account

            //  // Profile Account is self Signed
            //  // Account connection is valid under Profile Master
            //  // Device Account connection is valid under Profile Account
            //  // Device Activation for Account is valid

            }

        }
    }
