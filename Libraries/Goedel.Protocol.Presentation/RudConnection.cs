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
/// Base class for presentation connections.
/// </summary>
public abstract class RudConnection : Disposable {

    #region // Properties

    ///<summary>The size of a source ID tag.</summary> 
    public int SourceIdSize { get; } = 8;

    ///<summary>Packet Quantization</summary> 
    public int PacketQuanta { get; set; } = 64;

    ///<summary>If true, the connection is connected to the remote endpoint.</summary> 
    public bool Connected => mutualKeyAgreementResult != null;


    ///<summary>Symmetric key used to encrypt/decrypt mezzanine data sent by the client to 
    ///the host.</summary> 
    public byte[] ClientKeyClientToHost { get; set; }
    ///<summary>Symmetric key used to encrypt/decrypt mezzanine data sent by the host to 
    ///the client.</summary> 
    public byte[] ClientKeyHostToClient { get; set; }
    ///<summary>Symmetric key used to encrypt/decrypt inner data sent by the client to 
    ///the host.</summary> 
    public byte[] MutualKeyClientToHost { get; set; }
    ///<summary>Symmetric key used to encrypt/decrypt inner data sent by the host to 
    ///the client.</summary> 
    public byte[] MutualKeyHostToClient { get; set; }

    ///<summary>Symmetric key used to decrypt received mezzanine data.</summary> 
    public abstract byte[] ClientKeyIn { get; }
    ///<summary>Symmetric key used to encrypt sent mezzanine data.</summary> 
    public abstract byte[] ClientKeyOut { get; }
    ///<summary>Symmetric key used to decrypt received inner data.</summary> 
    public abstract byte[] MutualKeyIn { get; }
    ///<summary>Symmetric key used to encrypt sent inner data.</summary> 
    public abstract byte[] MutualKeyOut { get; }

    ///<summary>Public credential of the counter party.</summary> 
    public virtual ICredentialPublic CredentialOther { get; set; }
    ///<summary>Private credential of self.</summary> 
    public virtual ICredentialPrivate CredentialSelf { get; set; }

    ///<summary>The host credential</summary> 
    public abstract ICredentialPublic HostCredential { get; }

    ///<summary>The client credential</summary> 
    public abstract ICredentialPublic ClientCredential { get; }

    ///<summary>The packet that the connection is a response to.</summary> 
    public Packet PacketIn { get; set; }

    /////<summary>The local stream Id, this is generated localy and MAY contain hidden structure.</summary> 
    //public StreamId LocalStreamId { get; protected set; }

    /////<summary>Remote stream Id, an opaque blob.</summary> 
    //public byte[] RemoteStreamId { get; set; }

    /////<summary>When not null, contains the return address to be sent as an an extension.</summary> 
    //public byte[] ReturnStreamId = null;


    ///<summary>The listener this connection services</summary> 
    public Listener Listener { get; protected init; }

    ///<summary>The object encoding for use in the connection</summary> 
    public ObjectEncoding ObjectEncoding { get; set; } = ObjectEncoding.JSON;

    List<KeyPairAdvanced> ephemeralsOffered;


    ///<summary>The packet writer factory</summary> 
    public PacketWriterFactoryDelegate PacketWriterFactory { get; set; }
                = PacketWriterDebug.Factory;
    ///<summary>The packet reader factory</summary> 
    public PacketReaderFactoryDelegate PacketReaderFactory { get; set; }
                = PacketReader.Factory;


    #endregion



    #region // Methods - Serialization
    /// <summary>
    /// Generate a set of ephemerals for the supported algorithms to offer for 
    /// key agreement and add to <paramref name="extensions"/>.
    /// </summary>
    /// <param name="extensions">List of extensions to add the ephemerals to.</param>
    /// <param name="sourceId">The source identifier assigned to the return packet.</param>
    public virtual void AddEphemerals(
         byte[] sourceId, List<PacketExtension> extensions) =>
                CredentialSelf.AddEphemerals(extensions, ref ephemeralsOffered);

    /// <summary>
    /// Add the credentials specified in <see cref="CredentialSelf"/> to 
    /// <paramref name="extensions"/>
    /// </summary>
    /// <param name="extensions">List of extensions to add the ephemerals to.</param>
    public virtual void AddCredentials(
        List<PacketExtension> extensions) => CredentialSelf.AddCredentials(extensions);

    /// <summary>
    /// Add a challenge value over the current state to <paramref name="extensions"/>
    /// </summary>
    /// <param name="extensions">List of extensions to add the ephemerals to.</param>
    public virtual void AddChallenge(
            List<PacketExtension> extensions) {

        }

    /// <summary>
    /// Add a response value over the current state to <paramref name="extensions"/>
    /// </summary>
    /// <param name="extensions">List of extensions to add the ephemerals to.</param>
    public virtual void AddResponse(
            List<PacketExtension> extensions) { }

    /// <summary>
    /// Create a new stream identifier for the connection.
    /// </summary>
    /// <returns>The stream identifier.</returns>
    public virtual StreamId GetStreamId() => StreamId.GetStreamId();



    #endregion
    #region // Methods - PacketData Serializer/Deserializer


    /// <summary>
    /// Quantize the packet length so it is a fixed multiple of 64 bits.
    /// </summary>
    /// <param name="length">The minimum length to return.</param>
    /// <returns>The Quantized length.</returns>
    public int QuantizePacketLength(int length) =>
        length < PresentationConstants.MinimumPacketSize ? PresentationConstants.MinimumPacketSize :
                PacketQuanta * ((length + 200 + PacketQuanta) / PacketQuanta);


    /// <summary>
    /// Serialize and mutually encrypt a data packet.
    /// </summary>
    /// <param name="destinationStream">The remote stream identifier.</param>
    /// <param name="payload"></param>
    /// <param name="ciphertextExtensions"></param>
    /// <param name="packetSize">The number of bytes in the packet to be created.</param>
    /// <param name="buffer">Optional buffer passed in for use by the method.</param>
    /// <param name="position">Start point for writing to the buffer.</param>
    public virtual byte[] SerializePacketData(
            byte[] destinationStream,

            byte[] payload = null,
            List<PacketExtension> ciphertextExtensions = null,
            int packetSize = -1,
            byte[] buffer = null,
            int position = 0) {

        //packetSize = payload == null ? Constants.MinimumPacketSize :
        //    QuantizePacketLength(payload.Length);

        using var writer = PacketWriterFactory();
        writer.WriteExtensions(ciphertextExtensions);
        writer.Write(payload);


        // encrypt the result and return.
        return writer.Wrap(destinationStream, MutualKeyOut);
        }

    /// <summary>
    /// Parse the data in <paramref name="packet"/> and return the resulting packet.
    /// </summary>
    /// <param name="packet">The encrypted packet</param>
    /// <param name="offset">Offset at which to begin reading.</param>
    /// <param name="last">Last byte in the buffer to parse.</param>
    /// <returns>Packet specifying the decrypted payload and extensions (if specified).</returns>

    public virtual PacketData ParsePacketData(byte[] packet, int offset, int last) {
        var innerReader = PacketReader.Unwrap(MutualKeyIn, packet, offset, last);

        var result = new PacketData() {
            //SourcePortId = sourceId,
            };

        result.CiphertextExtensions = innerReader.ReadExtensions();
        result.Payload = innerReader.ReadBinary();

        return result;
        }
    #endregion
    #region // Methods - Streams



    #endregion
    #region // Methods - Key Agreement

    KeyAgreementResult clientKeyAgreementResult;
    KeyAgreementResult mutualKeyAgreementResult;
    /// <summary>
    /// Perform a key exchange to the host credential only. 
    /// </summary>
    /// <param name="privateKey">The private key</param>
    /// <param name="keyPublic">The public key.</param>
    public void ClientKeyExchange(
                    KeyPairAdvanced privateKey,
                    KeyPairAdvanced keyPublic) {
        clientKeyAgreementResult = privateKey.Agreement(keyPublic);

        var keyDerive = clientKeyAgreementResult.KeyDerive;

        //Screen.WriteLine($"Key Agreement {privateKey.KeyIdentifier}.{keyPublic.KeyIdentifier}");
        //Screen.WriteLine($"     {clientKeyAgreementResult.IKM.ToStringBase16()}");

        ClientKeyClientToHost = keyDerive.Derive(PresentationConstants.ByteKeyInitiatorResponder, PresentationConstants.SizeKeyAesGcm * 8);
        ClientKeyHostToClient = keyDerive.Derive(PresentationConstants.ByteKeyResponderInitiator, PresentationConstants.SizeKeyAesGcm * 8);
        }


    /// <summary>
    /// Perform a client key exchange to the host credential using an ephemeral chosen from the
    /// set of ephemerals chosen by the client.
    /// </summary>
    /// <param name="keyId">Host key identifier</param>
    public void ClientKeyExchange(out string keyId) {
        var (privateKey, publicEphemeral) = CredentialSelf.SelectKey(PacketIn.PlaintextExtensions);
        keyId = privateKey.KeyIdentifier;
        ClientKeyExchange(privateKey, publicEphemeral);
        }

    /// <summary>
    /// Perform a client key exchange to the host credential using the ephemeral chosen by the
    /// client.
    /// </summary>
    /// <param name="ephemeral"></param>
    /// <param name="keyId"></param>
    public virtual void ClientKeyExchange(byte[] ephemeral, string keyId) {
        var (privateKey, publicEphemeral) = CredentialSelf.SelectKey(keyId, ephemeral);
        ClientKeyExchange(privateKey, publicEphemeral);
        }


    /// <summary>
    /// Perform a client key exchange to the key <paramref name="keyId"/> using the first compatible 
    /// ephemeral previously offered.
    /// </summary>
    /// <param name="keyId">Host key identifier</param>
    public virtual void ClientKeyExchange(string keyId) {
        var (privateEphemeral, publickey) = HostCredential.SelectKey(ephemeralsOffered, keyId);
        ClientKeyExchange(privateEphemeral, publickey);
        }


    /// <summary>
    /// Perform a client key exchange to the host credential selecting a key and generating a
    /// compatible ephemeral returned as <paramref name="ephemeral"/>.
    /// </summary>
    /// <param name="ephemeral">The ephemeral generated.</param>
    /// <param name="keyId">Host key identifier</param>
    public virtual void ClientKeyExchange(out byte[] ephemeral, out string keyId) {
        var (privateEphemeral, publickey) = HostCredential.SelectKey();

        //Screen.WriteLine($"Client key exchange at client Ephemeral={privateEphemeral} Host={publickey}");

        ClientKeyExchange(privateEphemeral, publickey);

        keyId = publickey.KeyIdentifier;
        ephemeral = privateEphemeral.IKeyAdvancedPublic.Encoding;
        }



    /// <summary>
    /// Complete a mutual key exchange to the client credential and previous client exchange. 
    /// </summary>
    /// <param name="privateKey">The private key</param>
    /// <param name="keyPublic">The public key.</param>
    public void MutualKeyExchange(
                    KeyPairAdvanced privateKey,
                    KeyPairAdvanced keyPublic) {

        //Screen.WriteLine($"Keys {privateKey.KeyIdentifier}.{keyPublic.KeyIdentifier}");


        mutualKeyAgreementResult = privateKey.Agreement(keyPublic);

        var ikm = clientKeyAgreementResult.IKM.Concatenate(mutualKeyAgreementResult.IKM);

        var keyDerive = new KeyDeriveHKDF(ikm);

        MutualKeyClientToHost = keyDerive.Derive(PresentationConstants.ByteKeyInitiatorResponder, PresentationConstants.SizeKeyAesGcm * 8);
        MutualKeyHostToClient = keyDerive.Derive(PresentationConstants.ByteKeyResponderInitiator, PresentationConstants.SizeKeyAesGcm * 8);
        }


    /// <summary>
    /// Complete a mutual key exchange to the client credential using an ephemeral chosen from the
    /// set of nonces chosen by the host to complete a mutual key exchange.
    /// </summary>
    /// <param name="keyId">Client key identifier</param>
    public virtual void MutualKeyExchange(out string keyId) {
        var (privateKey, publicEphemeral) = CredentialSelf.SelectKey(PacketIn.PlaintextExtensions);
        keyId = privateKey.KeyIdentifier;
        MutualKeyExchange(privateKey, publicEphemeral);
        }

    /// <summary>
    /// Complete a mutual key exchange to the host credential using the ephemeral chosen by the
    /// host.
    /// </summary>
    /// <param name="ephemeral"></param>
    /// <param name="keyId"></param>
    public virtual void MutualKeyExchange(byte[] ephemeral, string keyId) {
        var (privateKey, publicEphemeral) = CredentialSelf.SelectKey(keyId, ephemeral);
        MutualKeyExchange(privateKey, publicEphemeral);
        }

    /// <summary>
    /// Complete a mutual key exchange to the key <paramref name="keyId"/> using the first compatible 
    /// ephemeral previously offered.
    /// </summary>
    /// <param name="keyId">Host key identifier</param>
    public virtual void MutualKeyExchange(string keyId) {
        var (privateEphemeral, publickey) = ClientCredential.SelectKey(ephemeralsOffered, keyId);
        MutualKeyExchange(privateEphemeral, publickey);
        }

    /// <summary>
    /// Complete a mutual key exchange to the client credential selecting a key and generating a
    /// compatible ephemeral returned as <paramref name="ephemeral"/> to complete a 
    /// mutual key exchange..
    /// </summary>
    /// <param name="ephemeral">The ephemeral generated.</param>
    /// <param name="keyId">Client key identifier</param>
    public virtual void MutualKeyExchange(out byte[] ephemeral, out string keyId) {
        var (privateEphemeral, publickey) = ClientCredential.SelectKey();
        MutualKeyExchange(privateEphemeral, publickey);

        keyId = publickey.KeyIdentifier;
        ephemeral = privateEphemeral.IKeyAdvancedPublic.Encoding;
        }


    #endregion

    }
