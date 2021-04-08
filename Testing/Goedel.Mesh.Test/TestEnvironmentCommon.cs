﻿using Goedel.Cryptography;
using Goedel.Cryptography.Dare;
using Goedel.IO;
using Goedel.Mesh;
using Goedel.Mesh.Server;
using Goedel.Protocol;
using Goedel.Utilities;
using Goedel.Test.Core;
using Goedel.Protocol.Service;
using System;
using System.Collections.Generic;
using System.IO;
using Goedel.Mesh.Session;


namespace Goedel.Mesh.Test {


    /// <summary>
    /// Test environment for one test with one service with one or more devices.
    /// </summary>
    public class TestEnvironmentCommon  {

        //static TestEnvironmentCommon() {
        //    Cryptography.Cryptography.Initialize();
        //    var _ = Goedel.Mesh.Client.ConnectionItem.Initialize;
        //    _ = Goedel.Mesh.MeshProtocol.Initialize;
        //    _ = Goedel.Mesh.MeshItem.Initialize;
        //    _ = Goedel.Mesh.Server.CatalogItem.Initialize;


        //    }


        public string ServiceName = "example.com";
        static string TestPath = "TestPath";
        static string TestRoot;

        public static string CommonData => System.IO.Path.Combine(TestRoot, "CommonData");
        public static string WorkingDirectory => System.IO.Path.Combine(TestRoot, "WorkingDirectory");
        public static string Variable => System.IO.Path.Combine(TestRoot, "Variable");

        public string Path => System.IO.Path.Combine(Variable, Test);
        public string ServiceDirectory => System.IO.Path.Combine(Path, "ServiceDirectory");
        public string Test;

        public JpcConnection JpcConnection = JpcConnection.Serialized;

        public PublicMeshService MeshService => meshService ??
            new PublicMeshService(ServiceName, ServiceDirectory).CacheValue (out meshService);
        PublicMeshService meshService;

        Service service;

        public MeshServiceClient GetMeshClient(MeshCredentialTraced meshCredential) {

            if (!JpcConnection.IsDirect()) {
                service ??= StartService();
                }

            JpcSession session = JpcConnection switch  {
                JpcConnection.Direct => new JpcSessionDirect(MeshService, meshCredential.AccountAddress),
                JpcConnection.Serialized => new TestSession(MeshService, 
                        meshCredential.AccountAddress, meshCredential.MeshProtocolMessages),
                //JpcConnection.Http => new JpcSessionHTTP(meshCredential.AccountAddress, Test),
                //JpcConnection.Ticketed => new JpcSessionTicketed(null, meshCredential.AccountAddress),
                _ => throw new NYI()
                };

            return session.GetWebClient<MeshServiceClient>();
            }


        Service StartService() {



            var httpEndpoint = new HttpEndpoint(ServiceName, MeshService.GetWellKnown, Test);
            var udpEndpoint = new UdpEndpoint(MeshService.GetWellKnown, Test);
            var endpoints = new List<Endpoint> { httpEndpoint, udpEndpoint };

            using var provider = new Provider(endpoints, MeshService);

            var providers = new List<Provider> { provider };
            return new MeshHost(null, providers);
            }



        /// <summary>
        /// Perform initialization of the Goedel.Cryptography portable class
        /// with delegates to the .NET framework methods.
        /// </summary>
        /// <param name="testMode">If true, the application will be initialized in
        /// test/debug mode.</param>

        static TestEnvironmentCommon() {
            TestRoot = Environment.GetEnvironmentVariable(TestPath);
            TestRoot.AssertNotNull(EnvironmentVariableRequired.Throw, TestPath);

            Directory.CreateDirectory(WorkingDirectory);
            Directory.SetCurrentDirectory(WorkingDirectory);
            }

        public TestEnvironmentCommon() {
            Test = Unique.Next();
            Path.DirectoryDelete();
            Directory.CreateDirectory(Path);
            Directory.CreateDirectory(ServiceDirectory);
            }


        public MeshMachineTest GetMeshMachine (string device) => new(this, device);

        public string MachinePath(string machineName) => System.IO.Path.Combine(Path, machineName);


        public static KeyCollection MakeKeyCollection() {
            var testEnvironment = new TestEnvironmentCommon();
            //var machineAdmin = new MeshMachineTest(TestEnvironment, "Test");
            return new KeyCollectionTestEnv(testEnvironment.Path);
            }

        public static DarePolicy MakePolicy(
                CryptoAlgorithmId signId = CryptoAlgorithmId.NULL,
                CryptoAlgorithmId encryptId = CryptoAlgorithmId.NULL) =>
            MakePolicy(out _, out _, signId, encryptId);
        public static DarePolicy MakePolicy(
            out KeyPair signKey, out KeyPair encryptKey,
            CryptoAlgorithmId signId = CryptoAlgorithmId.NULL,
            CryptoAlgorithmId encryptId = CryptoAlgorithmId.NULL) {

            encryptKey = null;
            signKey = null;

            var keyCollection = MakeKeyCollection();


            if (encryptId != CryptoAlgorithmId.NULL) {
                encryptKey = KeyPair.Factory(encryptId,
                        KeySecurity.Exportable, keyCollection, keyUses: KeyUses.Encrypt);
                }
            if (signId != CryptoAlgorithmId.NULL) {
                signKey = KeyPair.Factory(encryptId,
                        KeySecurity.Exportable, keyCollection, keyUses: KeyUses.Sign);
                }

            return new DarePolicy(keyCollection, signKey, encryptKey);
            }


        }



    }
