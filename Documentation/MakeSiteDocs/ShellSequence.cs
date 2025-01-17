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

using System.Collections.Generic;

using Goedel.Mesh.Test;

namespace ExampleGenerator;

public class ShellSequence : ExampleSet {

    public string TestSequence = "Sequence.dcon";
    public string TestSequenceEncrypt = "SequenceEncrypt.dcon";
    public string TestSequenceArchive = "SequenceArchive.dcon";
    public string TestSequenceArchiveEnhance = "SequenceArchiveEncrypt.dcon";

    public string TestSequence2 = "Sequence2.dcon";
    public string TestSequenceEncrypt2 = "SequenceEncrypt2.dcon";
    public string TestSequenceArchive2 = "SequenceArchive2.dcon";
    public string TestSequenceArchiveEnhance2 = "SequenceArchiveEncrypt2.dcon";

    public List<ExampleResult> SequenceCreate;
    public List<ExampleResult> SequenceCreateEncrypt;
    public List<ExampleResult> SequenceArchive;
    public List<ExampleResult> SequenceArchiveEnhance;
    public List<ExampleResult> SequenceArchiveVerify;
    public List<ExampleResult> SequenceArchiveExtractAll;
    public List<ExampleResult> SequenceArchiveExtractFile;

    public List<ExampleResult> SequenceAppend;
    public List<ExampleResult> SequenceDelete;
    public List<ExampleResult> SequenceIndex;
    public List<ExampleResult> SequenceArchiveCopy;
    public List<ExampleResult> SequenceArchiveCopyDecrypt;
    public List<ExampleResult> SequenceArchiveCopyPurge;


    public List<ExampleResult> SequenceList;


    public ShellSequence(CreateExamples createExamples) :
    base(createExamples) {
        SequenceCreate = Alice1.Example($"dare create {TestSequence}");
        SequenceCreateEncrypt = Alice1.Example($"dare create {TestSequenceEncrypt} /encrypt={GroupAccount}");
        SequenceArchive = Alice1.Example($"~dare archive {TestSequenceArchive} {TestDir1}");
        SequenceArchiveEnhance = Alice1.Example($"~dare archive {TestSequenceArchiveEnhance} {TestDir1}" +
                                                        $" /encrypt={GroupAccount} /sign={AliceAccount}");
        SequenceArchiveVerify = Alice1.Example($"~dare verify {TestSequenceArchiveEnhance}");
        SequenceArchiveExtractAll = Alice1.Example($"~dare extract {TestSequence} {TestDir2}");
        SequenceArchiveExtractFile = Alice1.Example($"~dare extract {TestSequence} /file={TestFile4}");
        SequenceAppend = Alice1.Example($"~dare append {TestSequence} {TestFile1}",
                                                        $"~dare append {TestSequence} {TestFile2}",
                                                        $"~dare append {TestSequence} {TestFile3}");
        SequenceList = Alice1.Example($"~dare list {TestSequence}");


        SequenceDelete = Alice1.Example($"~dare delete {TestSequence}  {TestFile2}");
        SequenceIndex = Alice1.Example($"~dare index {TestSequence}");
        SequenceArchiveCopy = Alice1.Example($"~dare copy {TestSequence2}");
        SequenceArchiveCopyDecrypt = Alice1.Example($"~dare copy {TestSequenceArchiveEnhance} /decrypt");
        SequenceArchiveCopyPurge = Alice1.Example($"~dare copy {TestSequence2} /purge");

        }
    }
