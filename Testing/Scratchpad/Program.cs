﻿using System;
using Goedel.XUnit;
using Goedel.Cryptography;
using Goedel.Utilities;
namespace Scratchpad {

    partial class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World");

            Cryptography.Initialize();

            TestService.Test().MeshNewContext();
            //TestService.Test().MeshConnectBase();

            //ShellTests.Test().TestProfileConnectAuth();
            //ShellTests.Test().TestProfileConnectAuthAll();
            //ShellTests.Test().TestProfileConnectEARL();

            // Catalog 
            //ShellTests.Test().TestProfileContact();
            //ShellTests.Test().TestProfileCalendar();
            //ShellTests.Test().TestProfileNetwork();
            //ShellTests.Test().TestProfileCalendar();
            //ShellTests.Test().TestMessageGroup();

            //// Application Catalog
            //ShellTests.Test().TestProfileMail();
            //ShellTests.Test().TestProfileSSHPrivate();
            //ShellTests.Test().TestProfileSSHPublic();

            //// Two messaging apps right now
            //ShellTests.Test().TestMessageContact();
            //ShellTests.Test().TestMessageConfirmation();

            }



        public static void Find(
                    CryptoAlgorithmID cryptoAlgorithmID = CryptoAlgorithmID.SHA_3_512) {
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
