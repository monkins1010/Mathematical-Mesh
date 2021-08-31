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


using System;

using Goedel.Cryptography;
using Goedel.Utilities;
using Goedel.XUnit;


namespace Scratchpad {

    // ToDo: Server check auth permissions
    // ToDo: Server logging
    // ToDo: Server status info
    // ToDo: Client decrypt files with threshold key

    // ToDo: Remove application from Mesh

    // ToDo: Test for delected device being denied access to stores etc.
    // ToDo: Test for threshold device being unable to decrypt after deletion

    // ToDo: Test write validator, read in all catalogs and check encryption etc. status



    // ToDo: Test Clear all unit tests

    // ToDo: Documentation - clear all examples in IDs.

    // ToDo: RUD Documentation - Grab examples from connection formation
    // ToDo: RUD Documentation - Describe stream lifecycle
    // ToDo: RUD Documentation - Describe account binding




    // Refactor: Convert all DareEnvelope to Enveloped<type> and use Decode for cached accessor


    partial class Program {
        static void Main() {
            Screen.WriteLine($"Start test  {DateTime.Now}");


            //TestService.Test().ProtocolHello();
            //TestService.Test().MeshDeviceSsh();
            //TestService.Test().MeshDeviceMail();

            //TestService.Test().MeshDeviceDirectKey();
            TestService.Test().MeshDeviceThresholdKey();


            //ShellTestsHTTP.Test().TestMessageGroup();
            //TestPresentationMesh.Test().TestMeshService();


            //// This is failing as a result of Operate having a lock that is acquired but not released.
            //ShellTestsAdmin.Test().TestAccount();


            //TestPresentationMesh.Test().TestAccountDeletion();
            //TestPresentationMesh.Test().TestDeviceDeletion();

            //TestPresentationMesh.Test().TestImpersonationConfirm();

            //TestPresentationMesh.Test().TestImpersonationGroup();


            }





        public static void Find(
                    CryptoAlgorithmId cryptoAlgorithmID = CryptoAlgorithmId.SHA_3_512) {
            //  Magic number for SHA2 is 4187123
            //  Magic number for SHA3 is  774665
            for (var i = 0; true; i++) {
                if ((i % 1000) == 0) {
                    Console.WriteLine(i);
                    }

                var document = $"UDF Compressed Document {i}";
                var contentType = "text/plain";
                var utf8 = document.ToUTF8();
                var digest = Platform.SHA3_512.Process(utf8);

                var buffer = UDF.UDFBuffer(digest, contentType);
                var UDFData = buffer.GetDigest(cryptoAlgorithmID);

                if (UDF.GetCompression(UDFData) > 0) {
                    Console.WriteLine($"!!!!!!!!!!!!!!!!  {i}");
                    return;
                    }

                }
            }

        }
    }
