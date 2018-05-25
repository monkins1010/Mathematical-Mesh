﻿//   Copyright © 2015 by Comodo Group Inc.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Goedel.Utilities;

namespace Goedel.Protocol {

    /// <summary>
    /// JSON Writer for JSON-B, a superset of JSON encoding with codes that permit
    /// efficient encoding of binary data and strings and encoding of floating point
    /// values without loss of precision.
    /// </summary>
    public class JSONBWriter : JSONWriter {



        /// <summary>
        /// Create a new JSON Writer.
        /// </summary>
        public JSONBWriter() {
            this.Output = new MemoryStream();
            }

        /// <summary>
        /// Create a new JSON Writer using the specified output buffer. If the buffer has
        /// an output stream defined, text will be written to the stream.
        /// </summary>
        /// <param name="Output">Output buffer</param> 
        public JSONBWriter(MemoryStream Output) {
            this.Output = Output;
            }

        /// <summary>
        /// Create a new JSON Writer using the specified output buffer. If the buffer has
        /// an output stream defined, text will be written to the stream. Note that
        /// the property GetBytes will return null unless the specified stream is a 
        /// memory stream.
        /// </summary>
        /// <param name="Output">Output buffer</param> 
        public JSONBWriter (Stream Output) {
            this.Output = Output;
            }


        /// <summary>
        /// Write out a Tag-Length value using the shortest possible production
        /// </summary>
        /// <param name="Code">Base code.</param>
        /// <param name="Length">Length of data to follow.</param>
        protected void WriteTag(byte Code, long Length) {
            if (Length < 0x100) {
                Output.Write((byte)(Code + JSONBCD.Length8));
                Output.Write((byte)(Length & 0xff));
                }
            else if (Length < 0x10000) {
                Output.Write((byte)(Code + JSONBCD.Length16));
                Output.Write((byte)((Length>>8) & 0xff));
                Output.Write((byte)(Length & 0xff));
                }
            else if (Length < 0x100000000) {
                Output.Write((byte)(Code + JSONBCD.Length32));
                Output.Write((byte)((Length >> 24) & 0xff));
                Output.Write((byte)((Length >> 16) & 0xff));
                Output.Write((byte)((Length >> 8) & 0xff));
                Output.Write((byte)(Length & 0xff));
                }
            else  {
                Output.Write((byte)(Code + JSONBCD.Length64));
                Output.Write((byte)((Length >> 56) & 0xff));
                Output.Write((byte)((Length >> 48) & 0xff));
                Output.Write((byte)((Length >> 40) & 0xff));
                Output.Write((byte)((Length >> 32) & 0xff));
                Output.Write((byte)((Length >> 24) & 0xff));
                Output.Write((byte)((Length >> 16) & 0xff));
                Output.Write((byte)((Length >> 8) & 0xff));
                Output.Write((byte)(Length & 0xff));
                }
            }

        /// <summary>Write integer.</summary>
        /// <param name="Data">Value to write</param>
        protected void WriteInteger(long Data) {
            if (Data >= 0) {
                WriteTag(JSONBCD.PositiveInteger, Data);
                }
            else {
                WriteTag(JSONBCD.NegativeInteger, -Data);
                }
            }

        /// <summary>
        /// Write Tag to the stream
        /// </summary>
        /// <param name="Tag">Tag text.</param>
        /// <param name="IndentIn">Current indent level.</param>
        public override void WriteToken(string Tag, int IndentIn) {
            WriteTag(JSONBCD.TagString, Tag.Length);
            Output.Write(Tag);
            }

        /// <summary>Write 32 bit integer.</summary>
        /// <param name="Data">Value to write</param>
        public override void WriteInteger32(int Data) {
            WriteInteger(Data);
            }

        /// <summary>Write 64 bit integer.</summary>
        /// <param name="Data">Value to write</param>
        public override void WriteInteger64(long Data) {
            WriteInteger(Data);
            }

        /// <summary>Write float32</summary>
        /// <param name="Data">Value to write</param>
        public override void WriteFloat32(float Data) {
            Output.Write(Data.ToString());
            }

        /// <summary>Write float64</summary>
        /// <param name="Data">Value to write</param>
        public override void WriteFloat64(double Data) {
            Output.Write(Data.ToString());
            }

        /// <summary>Write boolean.</summary>
        /// <param name="Data">Value to write</param>
        public override void WriteBoolean(bool Data) {
            if (Data) {
                Output.Write(JSONBCD.True);
                }
            else {
                Output.Write(JSONBCD.False);
                }
            }

        /// <summary>Write string without escaping.</summary>
        /// <param name="Data">Value to write</param>
        public override void WriteString(string Data) {
            WriteTag(JSONBCD.StringTerm, Data.Length);
            Output.Write(Data);
            }

        /// <summary>Write binary data as length-data item.</summary>
        /// <param name="Data">Value to write</param>
        public override void WriteBinary(byte[] Data) {
            WriteTag(JSONBCD.DataTerm, Data.Length);
            Output.Write(Data);
            }

        long PartLength = 0;
        /// <summary>Write binary data as length-data item.</summary>
        public override void WriteBinaryBegin (long Length, bool Terminal=true) {
            WriteTag(Terminal ? JSONBCD.DataTerm : JSONBCD.DataChunk, Length);
            PartLength = Length;
            }

        /// <summary>Write binary data as length-data item.</summary>
        /// <param name="Data">Value to write</param>
        /// <param name="First">The index position of the first byte in the input data to process</param>
        /// <param name="Length">The number of bytes to process</param>
        public override void WriteBinaryPart (byte[] Data, long First = 0, long Length = -1) {
            PartLength -= Data.LongLength;
            Assert.True(PartLength >= 0, BadPartLength.Throw);
            Output.Write(Data);
            }


        /// <summary>Mark start of array element</summary>
        public override void WriteArrayStart() {
            Output.Write("[");
            }

        /// <summary>Mark middle of array element</summary>
        /// <param name="first">If true, this is the first element. 
        /// The value is set false on each call</param>
        public override void WriteArraySeparator(ref bool first) {
            }


        /// <summary>Mark end of array element</summary>
        public override void WriteArrayEnd() {
            Output.Write("]");
            }

        /// <summary>Mark start of object element</summary>
        // Mark the start, middle and end of object elements
        public override void WriteObjectStart() {
            Output.Write("{");
            }

        /// <summary>Mark middle of object element</summary>
        /// <param name="first">If true, this is the first element. 
        /// The value is set false on each call</param>
        public override void WriteObjectSeparator(ref bool first) {
            }

        /// <summary>Mark end of object element</summary>
        public override void WriteObjectEnd() {
            Output.Write("}");
            }
        }
    }
