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

namespace Goedel.Cryptography.Dare;

/// <summary>
/// Simple container that supports the append and index functions but does not 
/// provide for cryptographic integrity.
/// </summary>
/// <threadsafety static="true" instance="false"/>
public class ContainerTree : ContainerList {

    /// <summary>
    /// Default constructor
    /// </summary>
    public ContainerTree(bool decrypt) : base(decrypt) {
        }


    /// <summary>
    /// Create a new container file of the specified type and write the initial
    /// data record
    /// </summary>
    /// <param name="jbcdStream">The underlying JBCDStream stream. This MUST be opened
    /// in a read access mode and should have exclusive read access. All existing
    /// content in the file will be overwritten.</param>
    /// <returns>The newly constructed container.</returns>
    /// <param name="decrypt">If true, decrypt the container payload contents.</param>
    public static new Sequence MakeNewContainer(
                    JbcdStream jbcdStream,
                    bool decrypt) {


        var containerInfo = new SequenceInfo() {
            ContainerType = DareConstants.SequenceTypeTreeTag,
            Index = 0
            };


        var containerHeader = new DareHeader() {
            SequenceInfo = containerInfo
            };


        var container = new ContainerTree(decrypt) {
            JbcdStream = jbcdStream,
            HeaderFirst = containerHeader
            };

        return container;
        }

    //readonly static byte[] EmptyBytes = new byte[0];

    /// <summary>
    /// Append the header to the frame. This is called after the payload data
    /// has been passed using AppendPreprocess.
    /// </summary>
    public override void PrepareFrame(SequenceWriter contextWrite) {


        PrepareFrame(contextWrite.SequenceInfo);


        base.PrepareFrame(contextWrite);
        }

    /// <summary>
    /// Prepare the ContainerInfo data for the frame.
    /// </summary>
    /// <param name="containerInfo">The frame to prepare.</param>
    protected override void PrepareFrame(SequenceInfo containerInfo) {
        if (containerInfo.Index == 0) {
            containerInfo.ContainerType = DareConstants.SequenceTypeTreeTag;
            }
        else {
            containerInfo.TreePosition =
                (int)PreviousFramePosition(containerInfo.Index);
            }



        //Console.WriteLine($"Prepare #{containerInfo.Index} @{JBCDStream.PositionWrite} Tree={containerInfo.TreePosition}");


        }




    #region // Container navigation

    /// <summary>
    /// Initialize the dictionaries used to manage the tree by registering the set
    /// of values leading up to the apex value.
    /// </summary>
    /// <param name="containerInfo">Final frame header</param>
    /// <param name="firstPosition">Position of frame 1</param>
    /// <param name="positionLast">Position of the last frame</param>
    protected override void FillDictionary(SequenceInfo containerInfo, long firstPosition, long positionLast) {
        FrameIndexToPositionDictionary.Add(0, 0);
        if (containerInfo.Index == 0) {
            return;
            }

        FrameIndexToPositionDictionary.Add(1, firstPosition);
        if (containerInfo.Index == 1) {
            return;
            }

        var position = positionLast;
        var index = containerInfo.Index;
        var treePosition = containerInfo.TreePosition;

        while (!IsApex(index)) {
            RegisterFrame(containerInfo, position);

            // Calculate position of previous node in tree.
            position = treePosition;
            index = (int)PreviousFrame(index);

            // 
            JbcdStream.PositionRead = treePosition;
            containerInfo = JbcdStream.ReadFrameHeader().SequenceInfo;

            if (index != containerInfo.Index) {

                }


            // This is failing because the container index is set to 2 when it should be 1.
            Assert.AssertTrue(index == containerInfo.Index, SequenceDataCorrupt.Throw);
            treePosition = containerInfo.TreePosition;
            }
        if (containerInfo.Index != 1) {
            RegisterFrame(containerInfo, position);
            }
        }

    static bool IsApex(long index) {
        if (index == 0) {
            return true;
            }

        while (index != 1) {
            if ((index & 1) == 0) {
                return false;
                }
            index >>= 1;
            }

        return true;
        }

    /// <summary>
    /// Move to the specified frame index.
    /// </summary>
    /// <param name="index">Frame index to move to</param>
    /// <returns>If the move to the specified index succeeded, returns <code>true</code>.
    /// Otherwise, returns <code>false</code>.</returns>
    public bool Move(long index) {
        MoveToIndex(index);
        return Next();
        }

    /// <summary>
    /// Move to the frame with index Position in the file. 
    /// <para>Since the file format only supports sequential access, this is slow.</para>
    /// </summary>
    /// <param name="index">The frame index to move to</param>
    /// <returns>If success, the frame index.</returns>
    public override bool MoveToIndex(long index) {
        if (index == FrameCount) {
            JbcdStream.PositionRead = JbcdStream.Length;
            return false;
            }

        if (FrameIndexToPositionDictionary.TryGetValue(index, out var position)) {
            JbcdStream.PositionRead = position;
            return true;
            //return Next();
            }

        //Obtain the position of the very last record in the file, this must be known.
        var Record = FrameCount - 1;
        Assert.AssertTrue(FrameIndexToPositionDictionary.TryGetValue(Record, out position), SequenceDataCorrupt.Throw);

        long nextRecord;
        bool found = true;

        //Console.WriteLine("Move to {0}", Index);

        while (Record > index) {
            DareHeader frameHeader;
            long nextPosition;

            if (PreviousFrame(Record) < index) {
                // The record we want is the one before the current frame
                nextRecord = Record - 1;

                if (!FrameIndexToPositionDictionary.TryGetValue(nextRecord, out nextPosition)) {
                    // we do not know the position of the next frame
                    if (!found) {
                        JbcdStream.PositionRead = position;
                        frameHeader = JbcdStream.ReadFrameHeader();
                        RegisterFrame(frameHeader.SequenceInfo, position);
                        }

                    PositionRead = position;
                    JbcdStream.Previous();
                    nextPosition = JbcdStream.PositionRead;

                    frameHeader = JbcdStream.ReadFrameHeader();
                    Assert.AssertTrue(frameHeader.SequenceInfo.Index == nextRecord, SequenceDataCorrupt.Throw);

                    found = false;
                    }
                else {
                    found = true;
                    }

                }
            else {
                nextRecord = PreviousFrame(Record);
                if (!FrameIndexToPositionDictionary.TryGetValue(nextRecord, out nextPosition)) {
                    // we do not know the position of the next frame

                    PositionRead = position;
                    frameHeader = JbcdStream.ReadFrameHeader();
                    nextPosition = frameHeader.SequenceInfo.TreePosition;

                    if (!found) {
                        RegisterFrame(frameHeader.SequenceInfo, position);
                        }
                    found = false;
                    }
                else {
                    found = true;
                    }

                }

            position = nextPosition;
            Record = nextRecord;

            //Console.WriteLine("    {0}: {1}", Record, Position);
            }

        PositionRead = position;
        return true;
        //return Next();
        }


    /// <summary>
    /// Get the frame position.
    /// </summary>
    /// <param name="frame">The frame index</param>
    /// <returns>The frame position.</returns>
    public override long GetFramePosition(long frame) {


        var Found = FrameIndexToPositionDictionary.TryGetValue(frame, out var Position);

        if (!Found) {

            }

        return Position;

        }


    /// <summary>
    /// Returns the start position of the prior frame in the tree.
    /// </summary>
    /// <param name="frameIndex">The Frame Index</param>
    /// <returns>The position of the frame.</returns>
    public long PreviousFramePosition(long frameIndex) {
        var Previous = PreviousFrame(frameIndex);

        //Console.WriteLine($"  Previous {FrameIndex} = {Previous}");

        return GetFramePosition(Previous);
        }

    /// <summary>
    /// Returns the index of the prior frame in the tree.
    /// </summary>
    /// <param name="frame">The frame index</param>
    /// <returns>The preceding frame index.</returns>
    public static long PreviousFrame(long frame) {
        long x2 = frame + 1;
        long d = 1;

        while (x2 > 0) {
            if ((x2 & 1) == 1) {
                return x2 == 1 ? (d / 2) - 1 : frame - d;
                }
            d *= 2;
            x2 /= 2;
            }
        return 0;
        }

    #endregion

    #region // Verification
    /// <summary>
    /// Perform sanity checking on a list of container headers.
    /// </summary>
    /// <param name="headers">List of headers to check</param>
    public override void CheckSequence(List<DareHeader> headers) {
        var index = 1;
        foreach (var Header in headers) {
            Assert.AssertTrue(Header.SequenceInfo.Index == index,
                    SequenceDataCorrupt.Throw);
            Assert.AssertNotNull(Header.PayloadDigest,
                    SequenceDataCorrupt.Throw);

            index++;
            }
        }


    /// <summary>
    /// Verify container contents by reading every frame starting with the first and checking
    /// for integrity. This is likely to take a very long time.
    /// </summary>
    public override void VerifySequence() {

        SortedDictionary<long, DareHeader> headerDictionary = new();

        // Check the first frame
        JbcdStream.PositionRead = 0;
        var header = JbcdStream.ReadFirstFrameHeader();
        Assert.AssertTrue(header.SequenceInfo.Index == 0, SequenceDataCorrupt.Throw);
        headerDictionary.Add(0, header);

        // Check subsequent frames
        int index = 1;
        while (!JbcdStream.EOF) {
            var Position = JbcdStream.PositionRead;
            header = JbcdStream.ReadFrameHeader();
            headerDictionary.Add(Position, header);

            Assert.AssertTrue(header.SequenceInfo.Index == index, SequenceDataCorrupt.Throw);
            if (index > 1) {
                var Previous = PreviousFrame(index);
                Assert.AssertTrue(headerDictionary.TryGetValue(header.SequenceInfo.TreePosition, out var PreviousHeader),
                    SequenceDataCorrupt.Throw);
                Assert.AssertTrue(PreviousHeader.SequenceInfo.Index == Previous,
                    SequenceDataCorrupt.Throw);
                }


            index++;
            }


        }

    #endregion
    }
