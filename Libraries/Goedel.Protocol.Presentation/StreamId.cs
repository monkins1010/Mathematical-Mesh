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


using System.Threading;

using Goedel.Utilities;


namespace Goedel.Protocol.Presentation;

/// <summary>
/// Structure managing stream ids
/// </summary>
public struct StreamId {
    #region // Properties


    ///<summary>Counter used to allocate unique IDs</summary> 
    static ulong Counter = 0;

    ///<summary>Size of fixed length Ids generated by this struct.</summary> 
    public const int SourceIdSize = 8;


    ///<summary>The stream Id value as an unsigned 64 bit integer.</summary> 
    public ulong Value { get; }


    ///<summary>The stream Id value as an unsigned 64 bit integer.</summary> 
    public byte[] Bytes => Value.BigEndian();

    readonly static byte[] ClientCompleteDeferred =
        ((ulong)InitiatorMessageType.InitiatorExchange).BigEndian();

    //readonly static byte[] ClientComplete =
    //    ((ulong)PlaintextPacketType.ClientComplete).BigEndian();

    ///<summary>Factory method, creates a packet extension wrapping the stream identifier.</summary> 
    public PacketExtension PacketExtension => new() {
        Tag = Constants.ExtensionTagsStreamIdTag,
        Value = Bytes
        };

    #endregion


    #region // Constructors


    /// <summary>
    /// Create an instance with the stream Id value <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The stream identifier value.</param>

    public StreamId(ulong value) => Value = value;

    #endregion


    #region // Methods 

    /// <summary>
    /// Return a new stream identifier that is identifiable as being for a packet of type 
    /// <see cref="ClientCompleteDeferred"/>
    /// </summary>
    /// <returns>The stream identifier.</returns>
    public static byte[] GetClientCompleteDeferred() => ClientCompleteDeferred;

    /// <summary>
    /// Return the stream identifier value as a byte sequence.
    /// </summary>
    /// <returns></returns>
    public byte[] GetValue() => Value.BigEndian();

    /// <summary>
    /// Process the initial bytes of the buffer to get the source ID value according to the 
    /// source ID processing mode specified for the session.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns>The retrieved sourceId and position in the buffer.</returns>
    public static (StreamId, int) GetSourceId(byte[] buffer) {
        var streamId = new StreamId(buffer.BigEndianInt(StreamId.SourceIdSize));
        //Screen.WriteLine($"Received Stream ID {streamId.Value}");

        return (streamId, StreamId.SourceIdSize);
        }


    /// <summary>
    /// Factory method returing a new unique stream identifier.
    /// </summary>
    /// <returns>The stream identifier created.</returns>
    public static StreamId GetStreamId() =>
            new(Interlocked.Increment(ref Counter));


    #endregion

    }
