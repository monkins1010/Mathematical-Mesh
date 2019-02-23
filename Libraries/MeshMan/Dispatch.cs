﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Goedel.Utilities;
using Goedel.Command;
using Goedel.Protocol;
using Goedel.Mesh;
using Goedel.Mesh.Platform;
using Goedel.Mesh.Portal.Client;

namespace Goedel.Mesh.MeshMan {

    public partial class Shell {

        public List<ConnectionRequest> PendingRequests;

        MeshMachine _MeshMachine = null;
        public MeshMachine MeshMachine {
            get {

                _MeshMachine = _MeshMachine ?? MeshMachine.Current;
                return _MeshMachine;
                }
            set => _MeshMachine = value;
            }

        string PortalID;
        string AccountID;

        public MeshClient MeshClient;

        CommandLineInterpreter CommandLineInterpreter = new CommandLineInterpreter();

        public string DefaultID {
            get => _PersonalCreate._DescribeCommand.GetDefault("did");
            set => _PersonalCreate._DescribeCommand.SetDefault("did", value);
            }
        public string DefaultDescription {
            get => _PersonalCreate._DescribeCommand.GetDefault("dd");
            set => _PersonalCreate._DescribeCommand.SetDefault("dd", value);
            }

        public Shell() => Mesh.Initialize(true);  // Hack is in test mode right now

        public string CommandLine { get; set; }
        public string Dispatch (string Command) {
            CommandLine = "meshman " + Command;
            CommandLineInterpreter.MainMethod(this, CommandSplitLex.Split(Command));
            return CommandLine;
            }


        public SessionDevice RegistrationDevice  => MeshMachine.Device;
        public DeviceProfile DeviceProfile => RegistrationDevice.DeviceProfile;

        /// <summary>
        /// Erase all test profiles
        /// </summary>
        /// <param name="Options">Command line parameters</param>
        public override void Reset(Reset Options) => MeshMachine.EraseTest();


        /// <summary>
        /// Create a new device profile
        /// </summary>
        /// <param name="Options">Command line parameters</param>
        public override void Device (Device Options) {
            var DeviceID = Options.DeviceID.Value ?? "Default";
            var DeviceDescription = Options.DeviceDescription.Value ?? "Unknown";
            bool? Default = Options.Default.Value;

            MeshMachine.CreateDevice(DeviceID, DeviceDescription, Default);
            }
        }
    }