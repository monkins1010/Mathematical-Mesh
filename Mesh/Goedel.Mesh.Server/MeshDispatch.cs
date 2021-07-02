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

using Goedel.Protocol;
using Goedel.Protocol.Presentation;
using Goedel.Utilities;
using Goedel.Mesh.ServiceAdmin;
using Goedel.IO;
using System.IO;

using System.Collections.Generic;
namespace Goedel.Mesh.Server {



    /// <summary>
    /// The session class implements the Mesh session. The implementations in this class are mostly 
    /// stubbs that martial and validate the parameters presented in the request and pass the
    /// work on to the <see cref="Server.MeshPersist"/> instance <see cref="MeshPersist"/>
    /// </summary>
    public class PublicMeshService : MeshService {

        #region // Properties
        
        ///<summary>The Mesh Machine base</summary> 
        public IMeshMachine MeshMachine { get; init; }

        ///<summary>The profile describing the service</summary>
        public ProfileService ProfileService { get; init; }

        ///<summary>The profile describing the host</summary>
        public ProfileHost ProfileHost { get; init; }

        ///<summary>The host activation record.</summary> 
        public ActivationDevice ActivationDevice { get; init; }

        ///<summary>The host connection record.</summary> 
        public ConnectionDevice ConnectionDevice { get; init; }

        ///<summary>The service configuration</summary> 
        public ServiceConfiguration ServiceConfiguration { get; init; }


        public HostConfiguration HostConfiguration { get; init; }
        /// <summary>
        /// The mesh persistence provider.
        /// </summary>
        public MeshPersist MeshPersist  { get; init; }

        ///<summary>The service description.</summary> 
        public static ServiceDescription ServiceDescription => new(WellKnown, Factory);

        #endregion
        #region // Constructors and factories

        /// <summary>
        /// Factory method, the signature is pro tem and will be changed later on.
        /// </summary>
        ///<param name="hostConfiguration">The host configuration.</param>
        ///<param name="serviceConfiguration">The service configuration.</param>
        /// <returns></returns>
        public static RudProvider Factory(
                ServiceConfiguration serviceConfiguration,
                HostConfiguration hostConfiguration) => throw new NYI();

        /// <summary>
        /// The mesh service dispatcher.
        /// </summary>
        /// <param name="domain">The domain of the service provider.</param>
        /// <param name="serviceDirectory">The mesh persistence store filename.</param>
        public PublicMeshService(string domain, string serviceDirectory) {

            Domains ??= new List<string>();
            Domains.Add(domain);

            MeshMachine = new MeshMachineCoreServer(serviceDirectory);

            MeshPersist = new MeshPersist(serviceDirectory, FileStatus.OpenOrCreate);

            // Dummy profiles for the service and host at this point
            ProfileService = ProfileService.Generate(MeshMachine.KeyCollection);

            // here we need to generate the activation record for the host and the connection for that record

            ProfileHost = ProfileHost.CreateHost(MeshMachine);

            // create an activation record and a connection record.

            ActivationDevice = new ActivationHost(ProfileHost);


            //Screen.WriteLine($"$$$$ Seed {ActivationDevice.ActivationSeed}");
            //Screen.WriteLine($"$$$$ Suth {ActivationDevice.ConnectionUser.Authentication.Udf}");
            // activate
            ActivationDevice.Activate(ProfileHost.SecretSeed);

            //Screen.WriteLine($"$$$$ Suth {ActivationDevice.DeviceAuthentication}");



            var connectionDevice = ActivationDevice.Connection;

            // Sign the connection and connection slim

            this.ConnectionDevice = new ConnectionDevice() {
                Account = "@example",
                Subject = connectionDevice.Subject,
                Authority = connectionDevice.Authority,
                Authentication = connectionDevice.Authentication
                };

            this.ConnectionDevice.Strip();

            ProfileService.Sign(this.ConnectionDevice, ObjectEncoding.JSON_B);

            this.ConnectionDevice.DareEnvelope.Strip();
            }

        private PublicMeshService(
                    IMeshMachine meshMachine,
                    ServiceConfiguration serviceConfiguration,
                    HostConfiguration hostConfiguration) {

            // pull out pieces from serviceConfiguration, hostConfiguration.


            // here do something with the domains if desired.
            // might just kill these in favor of the service description.
            }


        public static PublicMeshService Create(
            IMeshMachine meshMachine,
            string serviceConfig, string serviceDns, string hostIp, string hostDns,
            string admin, string newFile) {

            hostDns ??= "example.com";
            hostIp ??= "127.0.0.1";
            var hostName = System.Environment.MachineName;

            hostDns ??= serviceDns;

            var pathService = Path.Combine(meshMachine.DirectoryMesh, "service", hostName, "mmm");
            var pathHost = Path.Combine(meshMachine.DirectoryMesh, "hosts", hostName, "mmm");

            // Create the initial service application
            var ServiceConfiguration = new ServiceConfiguration() {
                DNS = new List<string> { serviceDns },
                Path = pathService
                };

            // populate with user supplied data




            var hostConfiguration = new HostConfiguration() {
                IP = new List<string> { hostIp},
                DNS = new List<string> { hostDns },
                Services = new List<string> { WellKnown },
                Path = pathHost
                };

            // create the service.
            var service = Create(meshMachine, ServiceConfiguration, hostConfiguration, hostDns);


            var configuration = new Configuration() {
                Entries = new List<ConfigurationEntry> { ServiceConfiguration, hostConfiguration}
                };


            if (admin != null) {
                // create an administrator profile

                // add to the service as an administrator

                }


            // write the configuration out.
            configuration.ToFile(newFile ?? serviceConfig);

            return service;
            }


        /// <summary>
        /// Create new service and host configurations and attach the service to the host.
        /// </summary>
        /// <param name="meshMachine">The mesh machine</param>
        /// <param name="serviceConfiguration">The service configuration</param>
        /// <param name="deviceAddress">The address of the initial host.</param>
        /// <returns>The mesh service interface.</returns>
        public static PublicMeshService Create(
                IMeshMachine meshMachine,
                ServiceConfiguration serviceConfiguration,
                HostConfiguration hostConfiguration,
                string deviceAddress = "@example"
                ) {

            // Create the service profile
            var profileService = ProfileService.Generate(meshMachine.KeyCollection);

            // Create a host profile and add create a connection to the host.
            var profileHost = ProfileHost.CreateHost(meshMachine);
            var activationDevice = new ActivationHost(profileHost);
            activationDevice.Activate(profileHost.SecretSeed);
            var connectionDevice1 = activationDevice.Connection;
            var connectionDevice = new ConnectionDevice() {
                Account = deviceAddress,
                Subject = connectionDevice1.Subject,
                Authority = connectionDevice1.Authority,
                Authentication = connectionDevice1.Authentication
                };

            // Strip and sign the device connection.
            connectionDevice.Strip();
            profileService.Sign(connectionDevice, ObjectEncoding.JSON_B);

            // Update the service configuration to add the service profile
            serviceConfiguration.EnvelopedProfileService = profileService.EnvelopedProfileService;

            hostConfiguration.EnvelopedProfileHost = profileHost.EnvelopedProfileHost;
            hostConfiguration.EnvelopedConnectionDevice = connectionDevice.EnvelopedConnectionDevice;


            // Initialize the persistence store.
            var meshPersist = new MeshPersist(hostConfiguration.Path, FileStatus.OpenOrCreate);

            return new PublicMeshService(
                        meshMachine, serviceConfiguration, hostConfiguration) {
                MeshPersist = meshPersist,
                ProfileService = profileService,
                ProfileHost = profileHost,
                ActivationDevice = activationDevice,
                ConnectionDevice = connectionDevice
                };

            }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="meshMachine"></param>
        /// <param name="serviceConfiguration"></param>
        /// <param name="hostConfiguration"></param>
        /// <returns></returns>
        public static PublicMeshService Load(
                IMeshMachine meshMachine,
                ServiceConfiguration serviceConfiguration,
                HostConfiguration hostConfiguration
                ) {

            // Need to read the host config back from the master catalog here
            ActivationDevice activationDevice = null;

            return new PublicMeshService(meshMachine, serviceConfiguration, hostConfiguration);


            throw new NYI();
            }



        /////<inheritdoc/>
        //public override JpcSession GetSession() => new JpcSessionHost();



        private MeshVerifiedDevice VerifyDevice(IJpcSession jpcSession) =>
            (jpcSession.Credential as MeshCredential).VerifyDevice();


        private MeshVerifiedAccount VerifyAccount(IJpcSession jpcSession) =>
            (jpcSession.Credential as MeshCredential).VerifyAccount();


        /// <summary>
        /// Respond with the 'hello' version and encoding info. This request does not 
        /// require authentication or authorization since it is the method a client
        /// calls to determine what the requirements for these are.
        /// </summary>		
        /// <param name="request">The request object to send to the host.</param>
        /// <param name="jpcSession">The connection authentication context.</param>
		/// <returns>The response object from the service</returns>
        public override MeshHelloResponse Hello(
                HelloRequest request, IJpcSession jpcSession) {

            var HelloResponse = new MeshHelloResponse() {
                Version = new Goedel.Protocol.Version() {
                    Major = 3,
                    Minor = 0,
                    Encodings = new List<Goedel.Protocol.Encoding>(),
                    },
                EnvelopedProfileService = ProfileService.EnvelopedProfileService,
                EnvelopedProfileHost = ProfileHost.EnvelopedProfileHost,
                Status = 201 // Must specify this explicitly since not derrived from MeshResponse.
                };

            var Encoding = new Goedel.Protocol.Encoding() {
                ID = new List<string> { "application/json" }
                };
            HelloResponse.Version.Encodings.Add(Encoding);

            return HelloResponse;
            }

        /// <summary>
		/// Server method implementing the transaction CreateAccount.
        /// </summary>
        /// <param name="request">The request object to send to the host.</param>
        /// <param name="jpcSession">The connection authentication context.</param>
		/// <returns>The response object from the service</returns>
        public override BindResponse BindAccount(
                BindRequest request, IJpcSession jpcSession) {


            try {

                Screen.WriteLine($"Bind {request.AccountAddress}");

                var verifiedDevice = VerifyDevice(jpcSession);

                var accountEntry = new AccountUser(request);
                MeshPersist.AccountAdd(jpcSession, verifiedDevice, accountEntry);
                return new BindResponse();
                }
            catch (System.Exception exception) {
                return new BindResponse(exception);
                }
            }


        /// <summary>
		/// Server method implementing the transaction  Connect.
        /// </summary>
        /// <param name="request">The request object to send to the host.</param>
        /// <param name="jpcSession">The connection authentication context.</param>
		/// <returns>The response object from the service</returns>
        public override ConnectResponse Connect(
                ConnectRequest request, IJpcSession jpcSession) {

            // decode MessageConnectionRequestClient with verification
            var requestConnection = request.EnvelopedRequestConnection.Decode();

            try {
                var connectResponse = MeshPersist.Connect(jpcSession, VerifyDevice(jpcSession), requestConnection);
                return connectResponse;
                }
            catch (System.Exception exception) {
                return new ConnectResponse(exception);

                }

            throw new NYI();
            }

        /// <summary>
        /// Server method implementing the transaction Download.
        /// </summary>
        /// <param name="request">The request object to send to the host.</param>
        /// <param name="jpcSession">The connection authentication context.</param>
        /// <returns>The response object from the service</returns>
        public override CompleteResponse Complete(
                CompleteRequest request, IJpcSession jpcSession ) {
            try {
                return MeshPersist.AccountComplete(jpcSession, VerifyDevice(jpcSession), request);
                }
            catch (System.Exception exception) {
                return new CompleteResponse(exception);

                }

            }

        /// <summary>
        /// Server method implementing the transaction Download.
        /// </summary>
        /// <param name="request">The request object to send to the host.</param>
        /// <param name="jpcSession">The connection authentication context.</param>
        /// <returns>The response object from the service</returns>
        public override StatusResponse Status(
                StatusRequest request, IJpcSession jpcSession) {
            try {
                return MeshPersist.AccountStatus(jpcSession, VerifyAccount(jpcSession));
                }
            catch (System.Exception exception) {
                return new StatusResponse(exception);

                }

            }


        /// <summary>
        /// Server method implementing the transaction  DeleteAccount.
        /// </summary>
        /// <param name="request">The request object to send to the host.</param>
        /// <param name="jpcSession">The connection authentication context.</param>
        /// <returns>The response object from the service</returns>
        public override UnbindResponse UnbindAccount(
                UnbindRequest request, IJpcSession jpcSession) {

            try {
                MeshPersist.AccountDelete(jpcSession, VerifyAccount(jpcSession), request.Account);
                return new UnbindResponse();
                }
            catch (System.Exception exception) {
                return new UnbindResponse(exception);

                }


            }


        /// <summary>
		/// Server method implementing the transaction  Download.
        /// </summary>
        /// <param name="request">The request object to send to the host.</param>
        /// <param name="jpcSession">The connection authentication context.</param>
		/// <returns>The response object from the service</returns>
        public override DownloadResponse Download(
                DownloadRequest request, IJpcSession jpcSession) {
            try {
                var Updates = MeshPersist.AccountDownload(jpcSession, VerifyAccount(jpcSession), request.Select);
                return new DownloadResponse() { Updates = Updates };
                }
            catch (System.Exception exception) {
                return new DownloadResponse(exception);

                }
            }

        /// <summary>
		/// Server method implementing the transaction  Upload.
        /// </summary>
        /// <param name="request">The request object to send to the host.</param>
        /// <param name="jpcSession">The connection authentication context.</param>
		/// <returns>The response object from the service</returns>
        public override TransactResponse Transact(
                TransactRequest request, IJpcSession jpcSession) {
            try {
                var account = VerifyAccount(jpcSession);
                MeshPersist.AccountUpdate(jpcSession, account,
                        request.Updates, request.Inbound, request.Outbound, request.Local, request.Accounts); ;
                return new TransactResponse();
                }
            catch (System.Exception exception) {
                return new TransactResponse(exception);

                }


            }
        /// <summary>
		/// Server method implementing the transaction  Post.
        /// </summary>
        /// <param name="request">The request object to send to the host.</param>
        /// <param name="jpcSession">The connection authentication context.</param>
		/// <returns>The response object from the service</returns>
        public override PostResponse Post(   
                PostRequest request, IJpcSession jpcSession) {

            try {
                //if (request.Outbound!= null) {
                //    Assert.AssertTrue(request.Outbound.Count == 1, NYI.Throw); // Hack: Support multiple messages in one post
                //    Mesh.MessagePost(jpcSession, jpcSession.VerifiedAccount, request.Accounts, request.Outbound[0]);
                //    }
                //if (request.Local != null) {
                //    Assert.AssertTrue(request.Local.Count == 1, NYI.Throw); // Hack: Support multiple messages in one post
                //    Mesh.MessagePost(jpcSession, jpcSession.VerifiedAccount, null, request.Local[0]);
                //    }
                ////if (request.Inbound != null) {
                ////    throw new NYI();
                ////    //Assert.AssertTrue(request.Self.Count == 1, NYI.Throw); // Hack: Support multiple messages in one post
                ////    //Mesh.MessagePost(jpcSession, jpcSession.VerifiedAccount, null, request.Self[0]);
                ////    }


                return new PostResponse();
                }
            catch (System.Exception exception) {
                return new PostResponse(exception);

                }

            }



        /// <summary>
		/// Server method implementing the transaction  Claim.
        /// </summary>
        /// <param name="request">The request object to send to the host.</param>
		/// <param name="session">The authentication binding.</param>
		/// <returns>The response object from the service</returns>
        public override ClaimResponse Claim(
                    ClaimRequest request,
                    IJpcSession session = null) => 
            MeshPersist.Claim(session, request.EnvelopedMessageClaim);

        /// <summary>
        /// Server method implementing the transaction  PollClaim.
        /// </summary>
        /// <param name="request">The request object to send to the host.</param>
        /// <param name="session">The authentication binding.</param>
        /// <returns>The response object from the service</returns>
        public override PollClaimResponse PollClaim(
                    PollClaimRequest request,
                    IJpcSession session = null) =>
            MeshPersist.PollClaim(session, request.TargetAccountAddress, request.PublicationId);


        /// <summary>
        /// Server method implementing the transaction Operate
        /// </summary>
        /// <param name="request">The request object to send to the host.</param>
        /// <param name="session">The authentication binding.</param>
        /// <returns>The response object from the service</returns>
        public override OperateResponse Operate(
                    OperateRequest request,
                    IJpcSession session = null) =>
            MeshPersist.Operate(session, request.AccountAddress, request.Operations);

        #endregion
        }
    }
