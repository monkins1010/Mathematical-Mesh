﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Goedel.IO;
using Goedel.Protocol;
using Goedel.Utilities;

namespace Goedel.Cryptography.Dare {

    #region // enumerations
    /// <summary>
    /// Enumeration describing a container type.
    /// </summary>
    public enum ContainerType {
        /// <summary>The type is not defined.</summary>
        Unknown,
        ///<summary>A double linked list container. It can be read efficiently in
        ///the forward or the reverse direction. It does not provide protection against
        ///an insertion attack or efficient random access.</summary>
        List,
        ///<summary>A double linked list container. It can be read efficiently in
        ///the forward or the reverse direction. It does not provide protection against
        ///an insertion attack or efficient random access. A digest checksum is calculated
        ///over the frame payload value but this is not linked to any other digest.</summary>
        Digest,
        ///<summary>A double linked list container indexed by a binary tree.It can be read efficiently in
        ///the forward or the reverse direction or as a random access file. It does not provide protection against
        ///an insertion attack.</summary>
        Tree,
        ///<summary>A double linked list container. It can be read efficiently in
        ///the forward or the reverse direction and incorporates a digest chain to
        ///provide protection against insertion attacks. It does not supporrt efficient random access.</summary>
        Chain,
        ///<summary>A double linked list container indexed by a binary tree.It can be read efficiently in
        ///the forward or the reverse direction or as a random access file and incorporates a Merkle tree to
        ///provide protection against insertion attacks.</summary>
        MerkleTree,
        }

    /// <summary>File index modes</summary>
    public enum IndexType {
        /// <summary>No index</summary>
        None,
        /// <summary>Index table of frame positions</summary>
        Position,
        /// <summary>There is an index table of positions and an index table for some specified labels.</summary>
        Partial,
        /// <summary>There is an index table of positions and an index table for all labels specified in the file.</summary>
        Complete
        }


    /// <summary>
    /// Class to allow enumeration of container frames
    /// </summary>
    public class ContainerFrame {
        /// <summary>The current write frame index (writes are always
        /// appended to the end of the file.</summary>
        public long FrameCount;

        /// <summary>The byte offset from the start of the file for the 
        /// first byte of the current frame.</summary>
        public long Position;

        /// <summary>The current frame data</summary>
        public byte[] FrameData;

        /// <summary>The current frame header as binary data</summary>
        public byte[] FrameHeader;

        /// <summary>The current frame header as a parsed object.</summary>
        public ContainerHeader Header;

        /// <summary>The current frame trailer as a parsed object.</summary>
        public DareTrailer Trailer;
        }

    #endregion

    
    /// <summary>
    /// Base class for container file implementations
    /// </summary>
    public abstract class container : Disposable, IEnumerable<ContainerDataReader> {

        #region // Properties

        ///<summary>The first frame in the container</summary>
        public DareEnvelope FrameZero;

        /// <summary>The underlying file stream</summary>
        public JBCDStream JBCDStream { get; set; }

        /// <summary>The byte offset from the start of the file for Record 1</summary>
        public virtual long StartOfData { get; protected set; }

        /// <summary>The encoding to use for creating the FrameHeader entry</summary>
        public DataEncoding DataEncoding { get; protected set; }

        /// <summary>The value of the last frame index</summary>
        public virtual long FrameCount { get; protected set; }

        ///<summary>The start of the last frame.</summary>
        public virtual long PositionFinalFrameStart { get; private set; }

        /// <summary>The current frame header as binary data</summary>
        public virtual byte[] FrameHeader {
            get => frameHeader;
            protected set {
                frameHeader = value;
                containerHeader = null;
                }
            }
        byte[] frameHeader = null;

        /// <summary>
        /// The cryptography parameters.
        /// </summary>
        public CryptoParameters CryptoParametersContainer = null;

        /// <summary>
        /// The default cryptographic stack
        /// </summary>
        public CryptoStack CryptoStackContainer = null;

        /// <summary>The current frame header as a parsed object.</summary>
        public virtual ContainerHeader ContainerHeader {
            get {
                if (frameHeader == null) {
                    return null;
                    }
                containerHeader = containerHeader ?? ContainerHeaderFirst.FromJSON(frameHeader.JSONReader(), false);
                return containerHeader;
                }
            }
        ContainerHeader containerHeader = null;

        /// <summary>
        /// The first container header. This is read only since it is fixed after
        /// the record is written.
        /// </summary>
        public ContainerHeaderFirst ContainerHeaderFirst { get; protected set; }

        /// <summary>
        /// The underlying stream reader/writer for the container. This will be disposed of when
        /// the container is released.
        /// </summary>
        public JBCDStream DisposeJBCDStream;


        #endregion

        #region // IDisposable
        /// <summary>
        /// The class specific disposal routine.
        /// </summary>
        protected override void Disposing() => DisposeJBCDStream?.Dispose();
        #endregion
        #region // IEnumerable

        /// <summary>
        /// Returns an enumerator over the container contents starting with the
        /// first frame.
        /// </summary>
        /// <returns>The enumerator</returns>
        public virtual IEnumerator<ContainerDataReader> GetEnumerator() =>
            new ContainerEnumerator(this);

        // Must also implement IEnumerable.GetEnumerator, but implement as a private method.
        private IEnumerator GetEnumerator1() => this.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator1();
        #endregion

        #region // Parameterized factory methods

        /// <summary>
        /// Open or create container according to the setting of FileStatus. The underlying 
        /// filestreams will be disposed of automatically when the container is disposed.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="fileStatus">The file access mode.</param>
        /// <param name="keyCollection">The key collection to be used to resolve requests
        /// for decryption keys. If unspecified, the default KeyCollection is used.</param>
        /// <param name="cryptoParameters">Cryptographic parameters specifying defaults
        /// for encoding and authentication of data.</param>
        /// <param name="containerType">The container type to create if the container does
        /// not already exist.</param>
        /// <param name="contentType">The content type to declare if a new container is
        /// created.</param>
        /// <returns>The new container.</returns>
        public static container Open(
                        string fileName,
                        FileStatus fileStatus = FileStatus.Read,
                        KeyCollection keyCollection = null,
                        CryptoParameters cryptoParameters = null,
                        ContainerType containerType = ContainerType.Unknown,
                        string contentType = null) {

            var jbcdStream = new JBCDStream(fileName, fileStatus: fileStatus);

            try {

                //Console.WriteLine($"Open Stream {fileName}");
                // Attempt to open file.
                container Container;

                // Create new container if empty or read the old one.
                if (jbcdStream.Length == 0) {
                    Container = NewContainer(jbcdStream, cryptoParameters,
                        containerType, contentType: contentType);
                    }
                else {
                    keyCollection = keyCollection ?? cryptoParameters?.KeyCollection;
                    Container = OpenExisting(jbcdStream, keyCollection);

                    }
                Container.DisposeJBCDStream = jbcdStream;
                return Container;
                }
            catch {
                jbcdStream?.Dispose();
                return null;
                }
            }

        #endregion
        #region // Open container 

        /// <summary>
        /// Open or create container according to the setting of FileStatus. The underlying 
        /// filestreams will be disposed of automatically when the container is disposed.
        /// </summary>
        /// <param name="jbcdStream">The stream to use to access the container.</param>
        /// <param name="keyCollection">The key collection to be used to resolve requests
        /// for decryption keys. If unspecified, the default KeyCollection is used.</param>
        /// <returns>The new container.</returns>
        public static container Open(
                        JBCDStream jbcdStream,
                        KeyCollection keyCollection = null) {


            var container = OpenExisting(jbcdStream, keyCollection);
            container.DisposeJBCDStream = jbcdStream;

            return container;
            }

        /// <summary>
        /// The default key collection to use for decryption
        /// </summary>
        protected KeyCollection KeyCollection;

        /// <summary>
        /// Open an existing container according to the information contained in the next frame to be read.
        /// </summary>
        /// <param name="jbcdStream">The frame reader. Since this is passed to the
        /// method to create the class it is not disposed with the container using it.</param>
        /// <param name="keyCollection">The key collection to be used to resolve requests
        /// for decryption keys. If unspecified, the default KeyCollection is used.</param>
        /// <returns></returns>
        public static container OpenExisting(
                        JBCDStream jbcdStream,
                        KeyCollection keyCollection = null) {

            // Initialize frame zero
            var frameZero = jbcdStream.ReadDareEnvelope();

            var found = jbcdStream.ReadFrame(out var Header, out var FrameData, out var FrameTrailer);
            found.AssertTrue();

            var containerHeaderFirst = frameZero.Header as ContainerHeaderFirst;

            var position1 = jbcdStream.PositionRead; // is always positioned after the first record on entry.
            //CryptoProviderDigest DigestProvider = CryptoCatalog.Default.GetDigest(CryptoAlgorithmID.Default);



            long frameCount = 1;
            ContainerHeader finalContainerHeader;
            if (position1 < jbcdStream.Length) {
                finalContainerHeader = jbcdStream.ReadLastFrameHeader();
                frameCount = finalContainerHeader.Index + 1;
                }
            else {
                finalContainerHeader = containerHeaderFirst;
                }

            var cryptoStack = containerHeaderFirst.GetCryptoStack(keyCollection);

            var positionFinalFrameStart = jbcdStream.StartLastFrameRead;

            container container;
            switch (containerHeaderFirst.ContainerType) {
                case ContainerList.Label: {
                    container = new ContainerList() {
                        JBCDStream = jbcdStream,
                        ContainerHeaderFirst = containerHeaderFirst,
                        StartOfData = position1,
                        FrameCount = frameCount,
                        CryptoStackContainer = cryptoStack
                        };
                    break;
                    }
                case ContainerDigest.Label: {
                    cryptoStack.Digest = true;
                    container = new ContainerList() {
                        JBCDStream = jbcdStream,
                        //DigestProvider = DigestProvider,
                        ContainerHeaderFirst = containerHeaderFirst,
                        StartOfData = position1,
                        FrameCount = frameCount,
                        CryptoStackContainer = cryptoStack
                        };
                    break;
                    }
                case ContainerChain.Label: {
                    cryptoStack.Digest = true;
                    container = new ContainerChain() {
                        JBCDStream = jbcdStream,
                        //DigestProvider = DigestProvider,
                        ContainerHeaderFirst = containerHeaderFirst,
                        StartOfData = position1,
                        FrameCount = frameCount,
                        CryptoStackContainer = cryptoStack
                        };
                    break;
                    }
                case ContainerTree.Label: {
                    container = new ContainerTree() {
                        JBCDStream = jbcdStream,
                        //DigestProvider = DigestProvider,
                        ContainerHeaderFirst = containerHeaderFirst,
                        StartOfData = position1,
                        FrameCount = frameCount,
                        CryptoStackContainer = cryptoStack
                        };
                    break;
                    }
                case ContainerMerkleTree.Label: {
                    cryptoStack.Digest = true;
                    container = new ContainerMerkleTree() {
                        JBCDStream = jbcdStream,
                        //DigestProvider = DigestProvider,
                        ContainerHeaderFirst = containerHeaderFirst,
                        StartOfData = position1,
                        FrameCount = frameCount,
                        CryptoStackContainer = cryptoStack
                        };
                    break;
                    }
                default: {
                    throw new NYI();
                    }
                }

            



            // initialize the Frame index dictionary
            container.PositionFinalFrameStart = positionFinalFrameStart;
            container.FillDictionary(finalContainerHeader, position1, positionFinalFrameStart);
            container.KeyCollection = keyCollection;
            jbcdStream.PositionRead = position1;

            return container;
            }

        /// <summary>
        /// Create a new container file of the specified type and write the initial
        /// data record
        /// </summary>
        /// <param name="filename">The file to open</param>
        /// <param name="fileStatus">The file status.</param>
        /// <param name="cryptoParameters">Specifies the cryptographic enhancements to
        /// be applied to this message.</param>
        /// <param name="payload">Optional data payload. </param>
        /// <param name="contentType">Content type of the optional data payload</param>
        /// <param name="containerType">The container type.</param>
        /// <param name="dataEncoding">The data encoding.</param>
        /// <param name="cloaked">Data to be converted to an EDS and presented as a cloaked header.</param>
        /// <param name="dataSequences">Data sequences to be converted to an EDS and presented 
        ///     as an EDSS header entry.</param>
        /// <exception cref="InvalidFileModeException">The file mode specified was not valid.</exception>
        public static container NewContainer(
                        string filename,
                        FileStatus fileStatus,
                        CryptoParameters cryptoParameters = null,
                        ContainerType containerType = ContainerType.Chain,
                        byte[] payload = null,
                        string contentType = null,
                        DataEncoding dataEncoding = DataEncoding.JSON,
                        byte[] cloaked = null,
                        List<byte[]> dataSequences = null
                        ) {

            Assert.True(fileStatus == FileStatus.New | fileStatus == FileStatus.Overwrite,
                InvalidFileModeException.Throw);

            var jbcdStream = new JBCDStream(filename, fileStatus);
            var container = NewContainer(
                jbcdStream, cryptoParameters, containerType, payload, contentType, dataEncoding,
                cloaked, dataSequences);
            container.DisposeJBCDStream = jbcdStream;

            return container;
            }



        /// <summary>
        /// Create a new container file of the specified type and write the initial
        /// data record
        /// </summary>
        /// <param name="jbcdStream">The underlying file stream. This MUST be opened
        /// in a read access mode and should have exclusive write access. All existing
        /// content in the file will be overwritten.</param>
        /// <param name="cryptoParameters">Specifies the cryptographic enhancements to
        /// be applied to this message.</param>
        /// <param name="payload">Optional data payload. </param>
        /// <param name="dataEncoding">The data encoding.</param>
        /// <param name="contentType">Content type of the optional data payload</param>
        /// <param name="containerType">The container type. This determines whether
        /// a tree index is to be created or not and if so, whether </param>

        /// <param name="cloaked">Data to be converted to an EDS and presented as a cloaked header.</param>
        /// <param name="dataSequences">Data sequences to be converted to an EDS and presented 
        ///     as an EDSS header entry.</param>
        public static container NewContainer(
                        JBCDStream jbcdStream,
                        CryptoParameters cryptoParameters,
                        ContainerType containerType = ContainerType.Chain,
                        byte[] payload = null,
                        string contentType = null,
                        DataEncoding dataEncoding = DataEncoding.JSON,
                        byte[] cloaked = null,
                        List<byte[]> dataSequences = null
                        ) {

            cryptoParameters = cryptoParameters ?? new CryptoParameters();
            var container = MakeNewContainer(jbcdStream,
                    cryptoParameters: cryptoParameters, containerType: containerType);

            container.CryptoParametersContainer = cryptoParameters;
            container.DataEncoding = dataEncoding;
            var ContainerHeaderFirst = container.ContainerHeaderFirst;

            ContainerHeaderFirst.DataEncoding = dataEncoding.ToString();
            ContainerHeaderFirst.ContentMeta = new ContentMeta() {
                ContentType = contentType
                };

            ContainerHeaderFirst.ApplyCryptoStack(container.CryptoStackContainer, cloaked, dataSequences);

            payload = ContainerHeaderFirst.EnhanceBody(payload, out var Trailer);
            container.MakeTrailer(ref Trailer);

            // May have issues here because we are not calling thje old append frame.
            var headerBytes = ContainerHeaderFirst.GetBytes(dataEncoding, false);
            var trailerBytes = Trailer?.GetBytes(dataEncoding, false);

            container.AppendFrame(headerBytes, payload, trailerBytes);
            container.FrameCount = 1;

            container.KeyCollection = cryptoParameters.KeyCollection;

            container.FrameZero = new DareEnvelope() {
                Header = ContainerHeaderFirst,
                Body = payload,
                Trailer = Trailer
                };

            return container;
            }

        /// <summary>
        /// Create a new container file of the specified type and write the initial
        /// data record
        /// </summary>
        /// <param name="jbcdStream">The underlying JBCDStream stream. This MUST be opened
        /// in a read access mode and should have exclusive read access. All existing
        /// content in the file will be overwritten.</param>
        /// <param name="containerType">The container type. This determines whether
        /// a tree index is to be created or not and if so, whether </param>
        /// <param name="cryptoParameters">Cryptographic parameters specifying algorithms and keys
        /// for encoding and authentication of data.</param>
        /// <param name="digestAlgorithm">The digest algorithm to be used to calculate the PayloadDigest</param>
        /// <returns>The newly constructed container.</returns>
        public static container MakeNewContainer(
                        JBCDStream jbcdStream,
                        CryptoParameters cryptoParameters,
                        ContainerType containerType = ContainerType.Chain,
                        CryptoAlgorithmID digestAlgorithm = CryptoAlgorithmID.Default) {
            container result;

            switch (containerType) {
                case ContainerType.List: {
                    result = ContainerList.MakeNewContainer(jbcdStream);
                    break;
                    }
                case ContainerType.Digest: {
                    result = ContainerDigest.MakeNewContainer(jbcdStream);
                    break;
                    }
                case ContainerType.Chain: {
                    result = ContainerChain.MakeNewContainer(jbcdStream);
                    break;
                    }
                case ContainerType.Tree: {
                    result = ContainerTree.MakeNewContainer(jbcdStream);
                    break;
                    }
                case ContainerType.MerkleTree: {
                    result = ContainerMerkleTree.MakeNewContainer(jbcdStream);
                    break;
                    }
                default: {
                    throw new InvalidContainerTypeException();
                    }
                }

            result.CryptoStackContainer = result.GetCryptoStack(cryptoParameters);

            return result;

            }

        #endregion
        #region // Navigation and enumeration methods


        /// <summary>
        /// Return an enumerator with the specified selectors.
        /// </summary>
        /// <param name="minIndex">The minimum index.</param>
        /// <param name="reverse">If true, read the container from the end.</param>
        /// <returns>The enumerator.</returns>
        public ContainerEnumeratorRaw Select(int minIndex, bool reverse = false) =>
            new ContainerEnumeratorRaw(this, minIndex, reverse);


        /// <summary>
        /// Dictionary of frame index to frame position.
        /// </summary>
        public Dictionary<long, long> FrameIndexToPositionDictionary =
            new Dictionary<long, long>();

        /// <summary>
        /// Register a frame in the container access dictionaries.
        /// </summary>
        /// <param name="header">Frame header</param>
        /// <param name="position">Position of the frame</param>
        protected virtual void RegisterFrame(ContainerHeader header, long position) {
            var index = header.Index;
            FrameIndexToPositionDictionary.AddSafe(index, position);
            }

        /// <summary>
        /// Get the frame position.
        /// </summary>
        /// <param name="frame">The frame index</param>
        /// <returns>The frame position.</returns>
        public virtual long GetFramePosition(long frame) {
            var found = FrameIndexToPositionDictionary.TryGetValue(frame, out var position);
            return position;
            }

        /// <summary>
        /// The number of bytes to be reserved for the trailer.
        /// </summary>
        /// <returns>The number of bytes to reserve</returns>
        public virtual DareTrailer FillDummyTrailer(CryptoStack cryptoStack) => null;

        /// <summary>
        /// The dummy trailer to add to the end of the frame.
        /// </summary>
        /// <returns></returns>
        public virtual void MakeTrailer(ref DareTrailer trailer) {
            }


        /// <summary>
        /// Append a new data frame payload to the end of the file.
        /// </summary>
        /// <param name="payload">The frame payload data value.</param>
        /// <param name="header">The frame header value.</param>
        /// <param name="trailer">The frame trailer value.</param>
        /// <returns>The number of bytes written.</returns>
        public long AppendFrame(byte[] header, byte[] payload = null, byte[] trailer = null) {
            // Write the frame ensuring the results get written out.
            var length = JBCDStream.WriteWrappedFrame(header, payload, trailer);

            return length;
            }


        /// <summary>
        /// Obtain a reader stream for the current frame data.
        /// </summary>
        /// <param name="index">The container index to be read.</param>
        /// <param name="position">The byte offset within the file from which to read.</param>
        /// <returns>The reader stream created.</returns>
        public abstract ContainerFrameReader GetFrameDataReader(
                long index = -1, long position = -1);

        #endregion
        #region // Convenience methods to read/write to containers.

        /// <summary>
        /// Append a new data frame payload to the end of the file.
        /// </summary>
        /// <param name="data">Ciphertext data to append.</param>
        /// <param name="containerHeader">Container header data.</param>
        /// <param name="cryptoParameters">Specifies the cryptographic enhancements to
        /// be applied to this message.</param>
        /// <param name="contentType">The payload content type.</param>
        /// <param name="cloaked">Data to be converted to an EDS and presented as a cloaked header.</param>
        /// <param name="dataSequences">Data sequences to be converted to an EDS and presented </param>
        /// <returns>The number of bytes written.</returns>
        public void Append(byte[] data,
            CryptoParameters cryptoParameters = null,
            ContainerHeader containerHeader = null,

                        string contentType = null,
                        byte[] cloaked = null,
                        List<byte[]> dataSequences = null) {

            using (var InputStream = new MemoryStream(data)) {
                var ContentLength = InputStream.Length;
                AppendFromStream(InputStream, ContentLength, containerHeader, cryptoParameters,
                        contentType, cloaked, dataSequences);
                }
            }


        /// <summary>
        /// Append a new data frame payload to the end of the file.
        /// </summary>
        /// <param name="data">Data to append.</param>
        /// <param name="cryptoParameters">Specifies the cryptographic enhancements to
        /// be applied to this message.</param>
        /// <param name="contentType">The payload content type.</param>
        /// <param name="cloaked">Data to be converted to an EDS and presented as a cloaked header.</param>
        /// <param name="dataSequences">Data sequences to be converted to an EDS and presented 
        ///     as an EDSS header entry.</param>
        /// <returns>The number of bytes written.</returns>
        public virtual void Append(
                        JSONObject data,
                        CryptoParameters cryptoParameters = null,
                        string contentType = null,
                        byte[] cloaked = null,
                        List<byte[]> dataSequences = null) =>
            Append(data.GetJson(), cryptoParameters, null, contentType, cloaked, dataSequences);

        /// <summary>
        /// Read data from the specified file and append to the container.
        /// </summary>
        /// <param name="fileName">The file to append</param>
        /// <param name="containerHeader">Container header data.</param>
        /// <param name="cryptoParameters">Specifies the cryptographic enhancements to
        /// be applied to this message.</param>
        /// <param name="contentType">The payload content type.</param>
        /// <param name="cloaked">Data to be converted to an EDS and presented as a cloaked header.</param>
        /// <param name="dataSequences">Data sequences to be converted to an EDS and presented 
        ///     as an EDSS header entry.</param>
        /// <returns>The number of bytes written.</returns>
        public void AppendFile(string fileName,
                ContainerHeader containerHeader = null,
                CryptoParameters cryptoParameters = null,
                string contentType = null,
                byte[] cloaked = null,
                List<byte[]> dataSequences = null) {

            using (var FileStream = fileName.OpenFileRead()) {
                var ContentLength = FileStream.Length;
                AppendFromStream(FileStream, ContentLength, containerHeader, cryptoParameters,
                        contentType, cloaked, dataSequences);
                }
            }

        /// <summary>
        /// Read data from the specified file and append to the container.
        /// </summary>
        /// <param name="input">The stream to be read.</param>
        /// <param name="contentLength"> The number of bytes to read from <paramref name="input"/>.</param>
        /// <param name="containerHeader">Container header data.</param>
        /// <param name="cryptoParameters">Specifies the cryptographic enhancements to
        /// be applied to this message.</param>
        /// <param name="contentType">The payload content type.</param>
        /// <param name="cloaked">Data to be converted to an EDS and presented as a cloaked header.</param>
        /// <param name="dataSequences">Data sequences to be converted to an EDS and presented 
        ///     as an EDSS header entry.</param>
        /// <returns>The number of bytes written.</returns>
        /// <remarks>At present, the file stream MUST support the seek operation
        /// which is an issue that has to be removed.</remarks>
        public void AppendFromStream(Stream input,
                long contentLength,
                ContainerHeader containerHeader = null,
                CryptoParameters cryptoParameters = null,
                string contentType = null,
                byte[] cloaked = null,
                List<byte[]> dataSequences = null) {
            AppendBegin(contentLength, out var CryptoStack, cryptoParameters, containerHeader,
                    contentType, cloaked, dataSequences);

            input.ProcessRead(AppendProcess);
            AppendEnd();
            }

        /// <summary>
        /// Write a Dare Message to the container directly without validating any entries in the
        /// header or trailer. This is used for direct access to the container in persistence
        /// stores such as catalogs and spools.
        /// </summary>
        /// <param name="dareMessage"></param>
        public void AppendDirect(DareEnvelope dareMessage) {
            AppendContainerHeader = new ContainerHeader(dareMessage.Header) {
                Index = (int)FrameCount++
                };

            CompleteHeader(); // Perform container type specific processing.

            var headerBytes = AppendContainerHeader.GetBytes(false);
            var trailerBytes = dareMessage.Trailer?.GetBytes(false);

            AppendFrame(headerBytes, dareMessage.Body, trailerBytes);
            Console.WriteLine($"Position Written {JBCDStream.PositionWrite}");

            //JBCDStream.WriteWrappedFrame(headerBytes, dareMessage.Body,
            //    trailerBytes);


            }

        /// <summary>
        /// Move to the start of the previous frame and save the reader position.
        /// Then read the frame and return the reader position to the start of
        /// the frame.
        /// </summary>
        /// <returns></returns>
        public DareEnvelope ReadDirectReverse() {
            Console.WriteLine($"Position Read {JBCDStream.PositionRead}");

            var position = JBCDStream.MoveFrameReverse();
            if (position <= 0) {
                return null; // Exclude the first frame from reverse enumeration.
                }

            Console.WriteLine($"Position ReadII {position}");

            var message = ReadDirect();
            JBCDStream.PositionRead = position;
            return message;
            }

        /// <summary>
        /// Return the current container frame as a DareEnvelope.
        /// </summary>
        /// <returns>The container data.</returns>
        public DareEnvelope ReadDirect() => JBCDStream.ReadDareEnvelope();

        /// <summary>
        /// Write the remaining unread frame data to the specified output file.
        /// </summary>
        /// <param name="fileName">The file to write.</param>
        public void WriteFrameToFile2(string fileName) {

            using (var containerDataReader = GetFrameDataReader()) {
                using (var outputStream = fileName.OpenFileWrite()) {
                    containerDataReader.CopyTo(outputStream);
                    }
                }
            }


        #endregion

        #region // Methods to append data from a stream of known length


        /// <summary>
        /// Header of the framer being written
        /// </summary>
        protected ContainerHeader AppendContainerHeader;

        /// <summary>
        /// Trailer of the frame being written
        /// </summary>
        protected DareTrailer AppendContainerTrailer;


        Stream BodyWrite;

        /// <summary>
        /// Begin appending a data frame.
        /// </summary>
        /// <remarks>This call is not thread safe. It is the responsibility of the caller
        /// to ensure that only one process writes to the container at once and that no other
        /// process has access.</remarks>
        /// <param name="cryptoStack">The generated set of cryptographic parameters</param>
        /// <param name="contentLength">The plaintext payload data length. the final payload
        /// length may be longer as a result of padding.</param>
        /// <param name="containerHeader">Pre-populated container header.</param>
        /// <param name="cryptoParametersFrame">Specifies the cryptographic enhancements to
        /// be applied to this message.</param>
        /// <param name="contentType">The payload content type.</param>
        /// <param name="cloaked">Data to be converted to an EDS and presented as a cloaked header.</param>
        /// <param name="dataSequences">Data sequences to be converted to an EDS and presented 
        ///     as an EDSS header entry.</param>
        public void AppendBegin(
                        long contentLength,
                        out CryptoStack cryptoStack,
                        CryptoParameters cryptoParametersFrame = null,
                        ContainerHeader containerHeader = null,
                        string contentType = null,
                        byte[] cloaked = null,
                        List<byte[]> dataSequences = null) {


            cryptoStack = cryptoParametersFrame == null ? new CryptoStack(this.CryptoStackContainer) :
                            GetCryptoStack(cryptoParametersFrame);

            AppendContainerHeader = containerHeader ?? new ContainerHeader();
            AppendContainerHeader.Index = (int)FrameCount++;
            AppendContainerHeader.ApplyCryptoStack(cryptoStack, cloaked, dataSequences);

            CompleteHeader(); // Perform container type specific processing.

            var payloadLength = AppendContainerHeader.OutputLength(contentLength);
            var dummyTrailer = FillDummyTrailer(cryptoStack);
            var lengthTrailer = dummyTrailer == null ? -1 : dummyTrailer.GetBytes(false).Length;

            //Console.WriteLine($"And the dummy trailer is {DummyTrailer}");

            var dataPayload = AppendContainerHeader.GetBytes(false);
            JBCDStream.WriteWrappedFrameBegin(dataPayload, payloadLength, lengthTrailer);
            BodyWrite = AppendContainerHeader.BodyWriter(JBCDStream.StreamWrite);
            //Console.WriteLine($"Append frame at {JBCDStream.PositionWrite}\n{AppendContainerHeader}");
            }


 



        /// <summary>
        /// Create a set of master keys and other cryptographic parameters from the
        /// specified profile.
        /// </summary>
        /// <param name="cryptoParameters">The cryptographic algorithms to use</param>
        /// <returns>The master parameters.</returns>
        protected virtual CryptoStack GetCryptoStack(CryptoParameters cryptoParameters) =>
            cryptoParameters.GetCryptoStack();

        /// <summary>
        /// Append the header to the frame. This is called after the payload data
        /// has been passed using AppendPreprocess.
        /// </summary>
        public virtual void CompleteHeader() {
            }


        /// <summary>
        /// Process record data. This method may be called any number
        /// of times but the total count of the number of items must match
        /// the Content Length specified in the original call.
        /// </summary>
        /// <param name="data">The data to procees</param>
        /// <param name="offset">Index of first byte to process.</param>
        /// <param name="count">Number of bytes to process.</param>
        public virtual void AppendProcess(byte[] data, int offset, int count) => 
            BodyWrite.Write(data, offset, count);

        /// <summary>
        /// Complete appending a record.
        /// </summary>
        public virtual void AppendEnd() {
            AppendContainerHeader.CloseBodyWriter(out var trailer);
            MakeTrailer(ref trailer);
            var trailerData = trailer?.GetBytes(false);
            //Console.WriteLine($"Trailer ({TrailerData?.Length}) \n {TrailerData?.ToUTF8()}");

            JBCDStream.WriteWrappedFrameEnd(trailerData);
            }

        #endregion 

        #region // Abstract and Virtual methods


        /// <summary>
        /// Initialize the dictionaries used to manage the tree by registering the set
        /// of values leading up to the apex value.
        /// </summary>
        /// <param name="header">Final frame header</param>
        /// <param name="firstPosition">Position of frame 1</param>
        /// <param name="positionLast">Position of the last frame</param>
        protected abstract void FillDictionary(
                    ContainerHeader header, long firstPosition, long positionLast);


        /// <summary>
        /// Perform sanity checking on a list of container headers.
        /// </summary>
        /// <param name="headers">List of headers to check</param>
        public abstract void CheckContainer(List<ContainerHeader> headers);

        /// <summary>
        /// Read the data in the current file 
        /// </summary>
        /// <param name="direction">Direction in which to perform check.
        /// <list type="bullet"><item>1 = forward</item><item>-1 = forward</item>
        /// <item>0 = forward then backward.</item></list></param>
        /// <returns>True if the validation succeded, otherwise false.</returns>
        public virtual bool Validate(int direction) => throw new NYI();

        /// <summary>
        /// Move read pointer to Frame 1.
        /// </summary>
        /// <returns>True if a next frame exists, otherwise false</returns>
        public virtual bool Start () {
            JBCDStream.Begin();
            return JBCDStream.EOF;
            }

        /// <summary>
        /// Begin reading record data. This method is called before ReadData
        /// to move the read pointer to the start of the payload data.
        /// </summary>
        /// <returns></returns>
        public abstract long ReadDataBegin();

        /// <summary>
        /// Read the next frame in the file.
        /// </summary>
        /// <returns>True if a next frame exists, otherwise false</returns>
        public abstract bool NextFrame();

        /// <summary>
        /// Read the next frame in the file.
        /// </summary>
        /// <returns>True if a next frame exists, otherwise false</returns>
        public abstract bool PreviousFrame();

        /// <summary>
        /// Read the previous frame in the file.
        /// </summary>
        /// <returns>True if a previous frame exists, otherwise false</returns>
        public abstract bool Previous ();


        /// <summary>
        /// Move to the frame with index Position in the file. 
        /// <para>If the tree positioning mechanism is in use, the
        /// time complexity for this operation is log2(n) where n is
        /// the difference between the current position and the new 
        /// position.</para>
        /// </summary>
        /// <param name="frameIndex">Frame index to move to.</param>
        /// <returns>True if the position exists.</returns>
        public abstract bool MoveToIndex (long frameIndex);

        /// <summary>
        /// Move to begin reading the last frame in the container.
        /// </summary>
        /// <returns></returns>
        public bool MoveToLast() {
            JBCDStream.End();
            return JBCDStream.PositionRead > 0;
            }

        /// <summary>
        /// Verify container contents by reading every frame starting with the first and checking
        /// for integrity. This is likely to take a very long time.
        /// </summary>
        public virtual void VerifyContainer() {
            }



        #endregion


        }
    }
