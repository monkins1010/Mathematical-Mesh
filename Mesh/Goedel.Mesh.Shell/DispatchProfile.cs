﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using Goedel.Utilities;
using Goedel.Mesh.Protocol.Client;
using Goedel.IO;
using Goedel.Cryptography;
using Goedel.Cryptography.Dare;
using Goedel.Cryptography.Jose;
using Goedel.Mesh;
using Goedel.Protocol;

namespace Goedel.Mesh.Shell {
    public partial class Shell {

        /// <summary>
        /// Dispatch method
        /// </summary>
        /// <param name="Options">The command line options.</param>
        /// <returns>Mesh result instance</returns>
        public override ShellResult ProfileHello(ProfileHello Options) {
            var meshClient = GetMeshClient(Options);
            var jpcSession = GetJpcSession(Options);

            var helloRequest = new HelloRequest();
            var response = meshClient.Hello(helloRequest, jpcSession);

            return new ResultHello() {
                Success = true,
                Response = response
                };
            }

        /// <summary>
        /// Dispatch method
        /// </summary>
        /// <param name="Options">The command line options.</param>
        /// <returns>Mesh result instance</returns>
        public override ShellResult DeviceCreate(DeviceCreate Options) {
            var context = ContextDevice.Generate(MeshMachine);

            return new ResultDeviceCreate() {
                Success = true,
                DeviceUDF = context.ProfileDevice.UDF,
                Default = context.DefaultDevice
                };

            }

        /// <summary>
        /// Dispatch method
        /// </summary>
        /// <param name="Options">The command line options.</param>
        /// <returns>Mesh result instance</returns>
        public override ShellResult MasterCreate(MasterCreate Options) {
            var account = Options.NewAccountID.Value;


            using (var context = GetContextDeviceUncached(Options)) {
                context.GenerateMaster();
                var result = context.CreateAccount(account);

                return new ResultMasterCreate() {
                    Success = true,
                    DeviceUDF = context.ProfileDevice.UDF,
                    PersonalUDF = context.ProfileMaster.UDF,
                    Default = context.DefaultDevice
                    };
                }
            }




        /// <summary>
        /// Dispatch method
        /// </summary>
        /// <param name="Options">The command line options.</param>
        /// <returns>Mesh result instance</returns>
        public override ShellResult ProfileSync(ProfileSync Options) {
            using (var contextDevice = GetContextDevice(Options)) {
                var result = contextDevice.Sync();

                return new ResultSync() {
                    Success = result.Success
                    };
                }
            }

        /// <summary>
        /// Dispatch method
        /// </summary>
        /// <param name="Options">The command line options.</param>
        /// <returns>Mesh result instance</returns>
        public override ShellResult ProfileEscrow(ProfileEscrow Options) {
            using (var contextDevice = GetContextDevice(Options)) {
                (var escrow, var shares) = contextDevice.Escrow(3, 2);
                return new ResultEscrow() {
                    Success = true
                    };
                }
            }

        void AddIfPresent(List<string> Keys, String Parameter) {
            if (Parameter.Value != null) {
                Keys.Add(Parameter.Value);
                }
            }

        
        /// <summary>
        /// Dispatch method
        /// </summary>
        /// <param name="Options">The command line options.</param>
        /// <returns>Mesh result instance</returns>
        public override ShellResult ProfileRecover(ProfileRecover Options) {
            using (var contextDevice = GetContextDevice(Options)) {

                var recoverShares = new List<string>();
                AddIfPresent(recoverShares, Options.Share1);
                AddIfPresent(recoverShares, Options.Share2);
                AddIfPresent(recoverShares, Options.Share3);
                AddIfPresent(recoverShares, Options.Share4);

                AddIfPresent(recoverShares, Options.Share5);
                AddIfPresent(recoverShares, Options.Share6);
                AddIfPresent(recoverShares, Options.Share7);
                AddIfPresent(recoverShares, Options.Share8);

                var Escrow = new DareMessage(); // Hack, should read the escrow data from the service or file.

                var DeviceAdminRecovered = contextDevice.Recover(Escrow, recoverShares);
                return new ResultRecover() {
                    Success = false
                    };
                }
            }


        /// <summary>
        /// Dispatch method
        /// </summary>
        /// <param name="Options">The command line options.</param>
        /// <returns>Mesh result instance</returns>
        public override ShellResult ProfileConnect(ProfileConnect Options) {
            using (var contextDevice = GetContextDeviceUncached(Options)) {
                var portal = Options.Portal.Value;
                var pin = Options.PIN.Value;

                var result = contextDevice.RequestConnect(portal);

                return new ResultConnect() {
                    Success = true
                    };
                }
            }


        public override ShellResult ProfilePending(ProfilePending Options) {
            using (var contextDevice = GetContextDevice(Options)) {

                // sync
                contextDevice.Sync();

                var messages = new List<MeshMessage>();
                var result = new ResultPending() {
                    Success = true,
                    Messages = messages
                    };

                // get the inbound spool
                var completed = new Dictionary<string, MeshMessage>();

                foreach (var message in contextDevice.SpoolInbound.Select(1, true)) {
                    var meshMessage = MeshMessage.FromJSON(message.GetBodyReader());
                    if (!completed.ContainsKey(meshMessage.MessageID)) {
                        switch (meshMessage) {
                            case MeshMessageComplete meshMessageComplete: {
                                foreach (var reference in meshMessageComplete.References) {
                                    completed.Add(reference.MessageID, meshMessageComplete);
                                    }
                                break;
                                }
                            default: {
                                messages.Add(meshMessage);
                                break;
                                }
                            }
                        }
                    }
                return result;
                }
            }

        

        public override ShellResult ProfileAccept(ProfileAccept Options) =>
            ProcessRequest(Options, Options.CompletionCode.Value, true);

        public override ShellResult ProfileReject(ProfileReject Options) =>
            ProcessRequest(Options, Options.CompletionCode.Value, false);


        ShellResult ProcessRequest(IAccountOptions Options, string messageID, bool accept) {
            using (var contextDevice = GetContextDevice(Options)) {
                contextDevice.Sync();

                var messageConnectionRequest = GetConnectionRequest(contextDevice, messageID);
                messageConnectionRequest.AssertNotNull();

                contextDevice.ProcessConnectionRequest(messageConnectionRequest, accept);

                return new ResultConnectProcess() {
                    Success = true,
                    Accepted = accept,
                    Witness = messageConnectionRequest.Witness
                    };
                }
            }

        MessageConnectionRequest GetConnectionRequest(
                ContextDevice contextDevice,
                string messageID) {
            contextDevice.Sync();
            var completed = new Dictionary<string, MeshMessage>();

            foreach (var message in contextDevice.SpoolInbound.Select(1, true)) {
                var meshMessage = MeshMessage.FromJSON(message.GetBodyReader());
                if (!completed.ContainsKey(meshMessage.MessageID)) {
                    switch (meshMessage) {
                        case MeshMessageComplete meshMessageComplete: {
                            foreach (var reference in meshMessageComplete.References) {
                                completed.Add(reference.MessageID, meshMessageComplete);
                                }
                            break;
                            }
                        case MessageConnectionRequest messageConnectionRequest: {
                            if (messageConnectionRequest.Witness == messageID |
                                    messageConnectionRequest.MessageID == messageID) {
                                return messageConnectionRequest;

                                }

                            break;
                            }

                        }
                    }
                }

            throw new NYI();
            }


        public override ShellResult ProfileGetPIN(ProfileGetPIN Options) {
            using (var contextDevice = GetContextDevice(Options)) {

                // create a random PIN

                // add pin to the device catalog

                // syncing the device catalog will now cause an admin device to automatically
                // reate a profile.


                throw new NYI();
                }
            }

        public override ShellResult ProfileList(ProfileList Options) {
            // pull the Catalog Host

            // list out all the data 

            throw new NYI();
            }

        public override ShellResult ProfileDump(ProfileDump Options) {
            using (var contextDevice = GetContextDevice(Options)) {
                // pull the Catalog Host

                // list out all the data for the default profile and connection state

                throw new NYI();
                }
            }

        #region // Import and export of profiles - punt on this for now

        /// <summary>
        /// Register a profile to a new service. This is not currently supported.
        /// </summary>
        /// <param name="Options">The command line options.</param>
        /// <returns>Mesh result instance</returns>
        public override ShellResult ProfileRegister(ProfileRegister Options) {
            throw new NYI();
            //var context = GetContextMaster(Options);

            //return new ResultConnect() {
            //    Success = true
            //    };
            }

        /// <summary>
        /// Dispatch method
        /// </summary>
        /// <param name="Options">The command line options.</param>
        /// <returns>Mesh result instance</returns>
        public override ShellResult ProfileExport(ProfileExport Options) {
            // pull the Catalog Host

            // dump the default profile to a file

            throw new NYI();
            }

        /// <summary>
        /// Dispatch method
        /// </summary>
        /// <param name="Options">The command line options.</param>
        /// <returns>Mesh result instance</returns>
        public override ShellResult ProfileImport(ProfileImport Options) {
            // pull the Catalog Host

            // add the profile to the catalog

            throw new NYI();
            }
        #endregion
        }
    }
