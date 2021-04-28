﻿using Goedel.Protocol;
using Goedel.Test.Core;
using Goedel.Mesh.Client;
using Goedel.Utilities;
using Goedel.Protocol.Presentation;
using Goedel.Cryptography;
using Goedel.Cryptography.Jose;
using System;
using System.Collections;
using System.Collections.Generic;
using Goedel.Mesh.Test;
using System.Threading;
using Goedel.Mesh;
using Goedel.Mesh.Session;
using Goedel.Mesh.Management;
using Goedel.Protocol.Service;

using Xunit;
using System.IO;

namespace Goedel.XUnit {
    public partial class TestPresentationMesh {


        public const string Domain = "localhost";
        //public string Protocol => ServiceManagementService.WellKnown;
        public const string Instance = "69";

        ServiceManagementService testProvider;

        [Fact]
        public void TestMeshService() {

            var clientCredential = GetInitiatorCredential();
            var hostCredential = GetResponderCredential();


            testProvider = new TestServiceStatus();
            var provider = new RudProvider(testProvider, TransportType.All, Domain, Instance);

            var providers = new List<RudProvider> { provider };
            using var server = new Goedel.Protocol.Service.RudService(providers, hostCredential);


            var meshServiceBinding = new ConnectionInitiator(clientCredential, Domain, Instance,
                        TransportType.Http);
            //meshServiceBinding.Initialize(null, null);

            //var x = ServiceManagementServiceClient.WellKnown;

            var clientAlice = meshServiceBinding.GetClient<ServiceManagementServiceClient>();


            var request = new ServiceStatusRequest();
            var response = clientAlice.ServiceStatus(request);



            var response2 = clientAlice.ServiceStatus(request);

            var response3 = clientAlice.ServiceStatus(request);


            }

        [Fact]
        public void TestMeshMultiService() {


            var testEnvironmentCommon = new TestEnvironmentCommon();

            var clientCredential = GetInitiatorCredential();
            var hostCredential = GetResponderCredential();

            var meshService = testEnvironmentCommon.MeshService;

            testProvider = new TestServiceStatus();

            var meshProvider = new RudProvider(meshService, TransportType.All, Domain, Instance);
            var provider = new RudProvider(testProvider, TransportType.All, Domain, Instance);

            var providers = new List<RudProvider> { meshProvider, provider };

            using var server = new Goedel.Protocol.Service.RudService(providers, hostCredential);


            var Connection = new ConnectionInitiator(clientCredential, Domain, Instance, TransportType.Http);
            //Connection.Initialize(null, null);


            var statusClient = Connection.GetClient<ServiceManagementServiceClient>();
            var meshClient = Connection.GetClient<MeshServiceClient>();

            // no messages sent out until here where we initialize the stream and the protocol at
            // the same time.
            var helloRequest = new HelloRequest() { };
            var response1 = meshClient.Hello(helloRequest);

            // An ordinary request
            var response2 = meshClient.Hello(helloRequest);

            // Try to start a different service 
            var serviceStatusRequest = new ServiceStatusRequest() { };
            var response3 = statusClient.ServiceStatus(serviceStatusRequest);

            // Another ordinary request
            var response4 = meshClient.Hello(helloRequest);

            // An ordinary request on the new client.
            // failing because we are not fishing out the session id on the stream assignment...
            var response5 = statusClient.ServiceStatus(serviceStatusRequest);


            }
        [Fact]

        public void TestCreateAccount() {
            var testEnvironmentCommon = new TestEnvironmentRdp();
            var contextAccountAlice = MeshMachineTest.GenerateAccountUser(testEnvironmentCommon,
                    DeviceAliceAdmin, AccountAlice, "main");

            var contextOnboardPending = MeshMachineTest.Connect(testEnvironmentCommon, "device3",
                    AccountAlice, "device2");

            contextAccountAlice.Sync();
            var connectRequest = contextAccountAlice.GetPendingMessageConnectionRequest();
            contextAccountAlice.Process(connectRequest);

            var contextOnboarded = TestCompletionSuccess(contextOnboardPending);
            }

        static ContextUser TestCompletionSuccess(ContextMeshPending contextMeshPending) {
            var contextUser = contextMeshPending.Complete();
            contextUser.Sync(); // Will fail if cannot complete

            return contextUser;
            }
        [Fact]

        public void TestMultipleServices() {

            // create a Mesh client
            // create a status client to same endpoint.

            throw new NYI();
            }

        [Fact]

        public void TestImpersonation() {


            var testEnvironmentCommon = new TestEnvironmentRdp();
            var contextAccountAlice = MeshMachineTest.GenerateAccountUser(testEnvironmentCommon,
                    DeviceAliceAdmin, AccountAlice, "main");

            var contextAccountMallet = MeshMachineTest.GenerateAccountUser(testEnvironmentCommon,
                    DeviceAliceAdmin, AccountMallet, "mallet");
            contextAccountMallet.ProfileUser = contextAccountAlice.ProfileUser;


            Xunit.Assert.Throws<NYI>(() => contextAccountMallet.Sync());


            }


        [Fact]

        public void TestPreconnect() {
            var testEnvironmentCommon = new TestEnvironmentRdp();
            var contextAccountAlice = MeshMachineTest.GenerateAccountUser(testEnvironmentCommon,
                    DeviceAliceAdmin, AccountAlice, "main");

            var contextOnboardPending = MeshMachineTest.Connect(testEnvironmentCommon, "device3",
                    AccountAlice, "device2");

            Xunit.Assert.Throws<ConnectionStillPending>(() => contextOnboardPending.Sync());

            }



        }


    public class TestServiceStatus : ServiceManagementService {
        public override JpcSession GetSession() => throw new NotImplementedException();
        public override ServiceStatusResponse ServiceStatus(ServiceStatusRequest request, IJpcSession session) => new() {
            Start = DateTime.Now
            };

        }


    //public partial class MeshServiceSession : JpcRemoteSession {

    //    public  string Protocol { get; }
    //    public  string Instance { get; }

    //    Session Session { get; }


    //    //public MeshServiceSession(PresentationType presentationTypes,
    //    //            string domain, string protocol, string instance = null) : base (null) {

    //    //    Domain = domain;
    //    //    Protocol = protocol;
    //    //    Instance = instance;

    //    //    Session = new TestConnectionClient(null);
    //    //    }




    //    //public static JpcSession JpcSession(
    //    //            string domain, string protocol, string instance=null) =>
    //    //     new MeshServiceSession(PresentationType.Http, domain, protocol, instance);

    //    //public void BindCredential(Credential credential) {
    //    //    }


    //    ///<inheritdoc/>
    //    public override JsonObject Post(string tag, JsonObject request) {

    //        // Get the Web service client


    //        // serialize the request



    //        // Make a presentation of the request


    //        // send the request and await the response



    //        // extract the response data


    //        // parse the response data.


    //        throw new NYI();
    //        }

        //}
    }