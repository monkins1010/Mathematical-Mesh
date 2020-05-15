﻿using Goedel.Protocol;
using Goedel.Utilities;
using System;
using System.IO;
using System.Security.Cryptography;
namespace Goedel.Cryptography.Dare {


    /// <summary>
    /// Packing formats
    /// </summary>
    public enum PackagingFormat {
        /// <summary>
        /// Package directly without padding
        /// </summary>
        Direct,

        /// <summary>
        /// Package as an Enhanced Data Sequence.
        /// </summary>
        EDS,

        /// <summary>
        /// Package as a container payload entry.
        /// </summary>
        Container

        }


    /// <summary>
    /// Tracks the cryptography providers used to compute MACs and Digests.
    /// </summary>
    public class CryptoStackStream : Stream {

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => Stream != null;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// The externally accessible stream.
        /// </summary>
        protected Stream Stream { get; }

        /// <summary>
        /// The Massage Authentication Code Transform.
        /// </summary>
        protected HashAlgorithm Mac;

        /// <summary>
        /// The Digest Transform.
        /// </summary>
        protected HashAlgorithm Digest;

        /// <summary>
        /// The computed MAC value.
        /// </summary>
        public byte[] MacValue { get; protected set; }

        /// <summary>
        /// The computed Digest value.
        /// </summary>
        public byte[] DigestValue { get; protected set; }



        /// <summary>
        /// Create a CryptoStack
        /// </summary>
        /// <param name="Mac">The Message Authentication Code Transform.</param>
        /// <param name="Digest">The Digest Transform.</param>
        protected CryptoStackStream(
                    HashAlgorithm Mac,
                    HashAlgorithm Digest) {
            this.Mac = Mac;
            this.Digest = Digest;
            }

        /// <summary>
        /// Creates a dummy stream. This may be a sink that simply discards the data (for 
        /// calculating digest values) or a passthrough that keeps the target stream open
        /// when the encryption stream is closed.
        /// </summary>
        /// <param name="Stream">The target stream. If null, output is simply discarded.</param>
        public CryptoStackStream(Stream Stream = null) => this.Stream = Stream;

        #region IDisposable boilerplate code.

        bool disposed = false;
        /// <summary>
        /// Dispose method, frees resources when disposing, 
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; 
        /// false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing) {
            if (disposed) {
                return;
                }

            if (disposing) {
                Disposing();
                }

            disposed = true;
            }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~CryptoStackStream() {
            Dispose(false);
            }
        #endregion

        /// <summary>
        /// The class specific disposal routine.
        /// </summary>
        protected virtual void Disposing() => Close();

        /// <summary>
        /// Copies bytes from the current buffered stream to an array (not supported).
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the 
        /// specified byte array with the values between <paramref name="offset"/> and 
        /// (<paramref name="offset"/> + <paramref name="count"/> - 1) 
        /// replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing 
        /// the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes 
        /// requested if that many bytes are not currently available, or zero (0) if the end of the stream 
        /// has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count) =>
                    Stream == null ? 0 : Stream.Read(buffer, offset, count);


        /// <summary>
        /// Write data to the output stream.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from 
        /// <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/>
        /// at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count) => Stream?.Write(buffer, offset, count);

        /// <summary>
        /// Closes the current stream, completes calculation of cryptographic values (MAC/Digest)
        /// associated with the current stream. Does not close the target stream because that would
        /// be stupid.
        /// </summary>
        public override void Close() {
            }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written 
        /// to the underlying device.
        /// </summary>
        public override void Flush() => Stream?.Flush();

        #region // Boilerplate implementations

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking(is always false).
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Gets the position within the current stream. The set operation is not supported.
        /// </summary>
        public override long Position {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
            }


        /// <summary>
        /// Sets the position within the current buffered stream (not supported).
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        /// <summary>
        /// Sets the length of the output frame.
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value) => throw new NotImplementedException();

        /// <summary>
        /// Gets the frame length in bytes. 
        /// </summary>
        public override long Length => throw new NotImplementedException();



        #endregion

        }

    #region //reader
    /// <summary>
    /// Tracks the cryptography providers used to compute MACs and Digests.
    /// </summary>
    public class CryptoStackStreamReader : CryptoStackStream {

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading (is always false).
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing(is always true).
        /// </summary>
        public override bool CanWrite => false;

        JsonBcdReader jbcdReader;

        /// <summary>
        /// Create a CryptoStack
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="mac"></param>
        /// <param name="digest"></param>
        public CryptoStackStreamReader(
                    JsonBcdReader stream,
                    HashAlgorithm mac,
                    HashAlgorithm digest) : base(mac, digest) {
            this.jbcdReader = stream;
            stream.ReadBinaryToken();
            }


        /// <summary>
        /// Copies bytes from the current buffered stream to an array 
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the 
        /// specified byte array with the values between <paramref name="offset"/> and 
        /// (<paramref name="offset"/> + <paramref name="count"/> - 1) 
        /// replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing 
        /// the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes 
        /// requested if that many bytes are not currently available, or zero (0) if the end of the stream 
        /// has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count) =>
            jbcdReader.ReadBinaryData(buffer, offset, count);




        /// <summary>
        /// Write data to the output stream.(not supported).
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from 
        /// <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/>
        /// at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        }


    /// <summary>
    /// Tracks the cryptography providers used to compute MACs and Digests.
    /// </summary>
    public class CryptoStackJBCDStreamReader : CryptoStackStream {

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading (is always false).
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing(is always true).
        /// </summary>
        public override bool CanWrite => false;

        Stream inputStream;

        /// <summary>
        /// Create a CryptoStack
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="Mac"></param>
        /// <param name="Digest"></param>
        public CryptoStackJBCDStreamReader(
                    Stream inputStream,
                    HashAlgorithm Mac,
                    HashAlgorithm Digest) : base(Mac, Digest) => this.inputStream = inputStream;


        /// <summary>
        /// Copies bytes from the current buffered stream to an array 
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the 
        /// specified byte array with the values between <paramref name="offset"/> and 
        /// (<paramref name="offset"/> + <paramref name="count"/> - 1) 
        /// replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing 
        /// the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes 
        /// requested if that many bytes are not currently available, or zero (0) if the end of the stream 
        /// has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count) {
            var length = inputStream.Read(buffer, offset, count);

            //Console.WriteLine($"Read {buffer} bytes");
            return length;
            }




        /// <summary>
        /// Write data to the output stream.(not supported).
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from 
        /// <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/>
        /// at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        }

    #endregion

    #region //writer
    /// <summary>
    /// Tracks the cryptography providers used to compute MACs and Digests.
    /// </summary>
    public class CryptoStackStreamWriter : CryptoStackStream {

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading (is always false).
        /// </summary>
        public override bool CanRead => false;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing(is always true).
        /// </summary>
        public override bool CanWrite => true;

        CryptoStream streamMac;
        CryptoStream streamDigest;
        Stream output;
        PackagingFormat packagingFormat;
        long payloadLength;
        CryptoStream CryptoStream { get; set; }


        /// <summary>
        /// The stream writer.
        /// </summary>
        public Stream Writer {
            get => CryptoStream ?? (Stream)this;
            set => CryptoStream = value as CryptoStream;
            }

        /// <summary>
        /// Create a CryptoStack
        /// </summary>
        /// <param name="output">The target stream to be written to. This is wrapped in a pipe to prevent
        /// it being closed when the encryption stream is closed.</param>
        /// <param name="mac">The Message Authentication Code Transform.</param>
        /// <param name="digest">The Digest Transform.</param>
        /// <param name="packagingFormat">The packing format to use on the output.</param>
        /// <param name="payloadLength">The payload length including cryptographic
        /// enhancements.</param>
        public CryptoStackStreamWriter(
                    Stream output,
                    PackagingFormat packagingFormat,
                    HashAlgorithm mac,
                    HashAlgorithm digest,
                    long payloadLength) : base(mac, digest) {

            //this.JSONWriter = JSONWriter;


            //Console.Write($"Payload length is {PayloadLength}");

            this.packagingFormat = packagingFormat;

            Writer = this;

            this.payloadLength = payloadLength;
            if (payloadLength >= 0 & packagingFormat != PackagingFormat.Direct) {
                JSONBWriter.WriteTag(output, JSONBCD.DataTerm, payloadLength);
                //Console.Write($"Written tag for {PayloadLength}");
                }

            streamMac = mac == null ? null : new CryptoStream(new CryptoStackStream(), mac, CryptoStreamMode.Write);



            if (digest != null) {
                streamDigest = new CryptoStream(
                new CryptoStackStream(output), digest, CryptoStreamMode.Write);
                output = streamDigest;
                }
            this.output = output;

            }


        /// <summary>
        /// Copies bytes from the current buffered stream to an array (not supported).
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the 
        /// specified byte array with the values between <paramref name="offset"/> and 
        /// (<paramref name="offset"/> + <paramref name="count"/> - 1) 
        /// replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing 
        /// the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes 
        /// requested if that many bytes are not currently available, or zero (0) if the end of the stream 
        /// has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        bool final = false;
        /// <summary>
        /// Write data to the output stream.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from 
        /// <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/>
        /// at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count) {
            streamMac?.Write(buffer, offset, count);

            if (payloadLength > 0 | packagingFormat == PackagingFormat.Direct) {
                payloadLength -= count;
                output.Write(buffer, offset, count);

                //Console.Write($"  Have {count} bytes to stream");
                }
            else {
                JSONBWriter.WriteTag(output, final ? JSONBCD.DataTerm : JSONBCD.DataChunk,
                    count);
                output.Write(buffer, offset, count);

                //Console.Write($"  Have {count} chunk (final:{Final}) to stream");
                }
            }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written 
        /// to the underlying device.
        /// </summary>
        public override void Flush() => output?.Flush();

        readonly static byte[] Empty = new byte[0];

        bool closed = false;

        /// <summary>
        /// Closes the current stream, completes calculation of cryptographic values (MAC/Digest)
        /// associated with the current stream. Does not close the target stream because that would
        /// be stupid.
        /// </summary>
        public override void Close() {
            if (closed) {
                return;
                }
            closed = true;

            final = true;
            if (CryptoStream == null) {
                Writer.Write(Empty, 0, 0);
                //Console.Write($"  Written end marker");
                }
            else {
                CryptoStream.FlushFinalBlock();
                }

            if (Digest != null) {
                streamDigest?.Dispose();
                streamDigest = null;
                DigestValue = Digest?.Hash;
                Digest?.Dispose();
                Digest = null;
                }


            if (Mac != null) {
                streamMac?.Dispose();
                streamMac = null;
                MacValue = Mac?.Hash;
                Mac?.Dispose();
                if (packagingFormat == PackagingFormat.EDS) {
                    JSONBWriter.WriteBinary(output, MacValue);
                    }
                }
            }
        }
    #endregion

    #region JsonDecryptingReader

    //public class JsonDecryptingReader : JsonBcdReader {


    //    public JsonDecryptingReader(CryptoStackStreamReader input) : base(input) {
    //        }


    //    public bool Close() {

    //        throw new NYI();
    //        }

    //    }

    #endregion
    }
