﻿using System;
using System.Collections.Generic;
using System.IO;
using Goedel.Debug;
using Goedel.Protocol;

namespace Goedel.Mesh {
    class Program {
        static void Main(string[] args) {
            var MeshTest = new MeshTest();
            MeshTest.MeshStoreAPI();
            }
        }

    /// <summary>
    /// 
    /// </summary>

    // Objective - by Friday
    //   Write out personal profile and parse it back
    //   Unpack all the substructures
    //   Unpack everything

    partial class MeshTest {
        public static string UserName = "Alice";
        public static string Service = "prismproof.org";

        public readonly string AccountID = Account.ID(UserName, Service);


        public string Device1 = "Voodoo";
        public string Device1Description = "Windows Desktop";

        public string App1 = "Password";
        public string App2 = "Mail";

        public string MailAccount = "alice@example.com";

        public static List<string> STARTTLS = new List<string> { "STARTTLS" };
        public static List<string> TLS = new List<string>  { "TLS" };
        public Connection ConnectionSubmit = new Connection(
            "smtp.example.com", 587, "_submission._tcp", STARTTLS);
        public Connection ConnectionIMAP = new Connection(
            "imap.example.com", 993, "_imap4._tcp", TLS);

        public string Device2 = "Phone";
        public string Device2Description = "Apple iPhone";

        public string Device3 = "Watch";
        public string Device3Description = "Android Watch";

        public string PWDSite = "www.example.com";
        public string PWDUser = "Alice";
        public string PWDPassword = "Secret1";

        //public string PWDUserResult, PWDPasswordResult;

        public Connection DNS1 = new Connection(
            "10.10.10.10", 53, "_dns._udp", null);
        public Connection DNS2 = new Connection(
            "10.10.5.5", 53, "_dns._udp", null);

        public int shares = 5;
        public int quorum = 3;

        public Mesh Mesh;
        //public PublicMeshServiceHost MeshServiceHost;
        //public JPCSession Session;
        //public MeshService MeshService;

        public string Store = "Tmesh.jlog";
        public string Portal = "Tportal.jlog";
        public MeshTest() {
            
            }


        public void Do() {
            MeshStoreAPI();

            }

        public void MeshStorem() {
            File.Delete(Store);
            File.Delete(Portal);
            Mesh = new Mesh(Service, Store, Portal);
            Mesh.CheckAccount(AccountID);

            var DevProfile = new SignedDeviceProfile(Device1, Device1Description);
            var UserProfile = new PersonalProfile(DevProfile);

            var SignedProfile = UserProfile.Signed;
            Mesh.CreateAccount(UserName, SignedProfile);

            var Account = Mesh.GetAccount(UserName);

            var PasswordProfile = new PasswordProfile(UserProfile);
            var SignedPasswordProfile = PasswordProfile.Signed;

            SignedProfile = UserProfile.Signed;
            Mesh.AddProfile(SignedPasswordProfile);
            Mesh.UpdateProfile(SignedProfile);





            }


        public void MeshStore () {

            File.Delete(Store);
            File.Delete(Portal);
            Mesh = new Mesh(Service, Store, Portal);

            Mesh.CheckAccount(AccountID);

            var DevProfile = new SignedDeviceProfile(Device1, Device1Description);
            var UserProfile = new PersonalProfile(DevProfile);

            var SignedProfile = UserProfile.Signed;
            Mesh.CreateAccount(AccountID, SignedProfile);

            // Add the device to the profile entry in the parent.

            var PasswordProfile = new PasswordProfile(UserProfile);

            var SignedPasswordProfile = PasswordProfile.Signed;

            SignedProfile = UserProfile.Signed;
            Mesh.AddProfile(SignedPasswordProfile); 
            Mesh.UpdateProfile(SignedProfile);

            // ok now pull the profile as a client.

            var Account = Mesh.GetAccount(AccountID); 
            var AccountPersonalProfile = Mesh.GetPersonalProfile(Account.UserProfileUDF);
            AccountPersonalProfile.SignedDeviceProfile = DevProfile;

            var PasswordEntry = AccountPersonalProfile.GetPasswordProfile();
            var SignedPasswordProfile2 = Mesh.GetProfile(PasswordEntry.Identifier);

            var AccountPasswordProfile = PasswordProfile.Get(
                            SignedPasswordProfile2, AccountPersonalProfile);
            AccountPasswordProfile.Add(PWDSite, PWDUser, PWDPassword);

            // Implement the second way to do things, cleaner.
            //var AccountSignedPassword = new SignedPasswordProfile(AccountPasswordProfile);
            var AccountSignedPassword = AccountPasswordProfile.Signed;
            Mesh.UpdateProfile(AccountSignedPassword);

            // Now add a new device

            var DevProfile2 = new SignedDeviceProfile(Device2, Device2Description);

            // Post Connect Request
            var ChainToken = Mesh.GetChainToken();
            var ConnectionRequest = new ConnectionRequest(Account, DevProfile2);

            var Pending = Mesh.PostConnectionRequest(ConnectionRequest.Signed, 
                Account.UniqueID);

            // Get list of pending requests
            var Connections = Mesh.GetPendingRequests(Account.AccountID);

            // Accept pending request


            var ConnectionResult = new ConnectionResult();
            ConnectionResult.Result = "Accept";
            ConnectionResult.Device = DevProfile2;
            var SignedConnectionResult = new SignedConnectionResult(ConnectionResult,
                AccountPersonalProfile.GetAdministrationKey());
            Mesh.CloseConnectionRequest(Account.AccountID, SignedConnectionResult);


            // Pull password data 
            var Status = Mesh.GetRequestStatus(Account.AccountID, DevProfile2.UDF);


            // decrypt using device2 credential
            var SignedPasswordProfile3 = Mesh.GetProfile(PasswordEntry.Identifier);
            var PP3 = PasswordProfile.Get(SignedPasswordProfile3, AccountPersonalProfile);
            var PasswordPrivate = PP3.Private;


            }




        //public void InitDirect () {
        //    MeshServiceHost = new PublicMeshServiceHost (Service, Store, Portal);
        //    Session = new DirectSession(UserName);
        //    MeshService = new MeshServiceSession(MeshServiceHost, Session);
        //    }


        //JHost JHost;
        //public void InitRemote() {
        //    // Create the service instance
        //    MeshServiceHost = new PublicMeshServiceHost(Service, Store, Portal);

        //    // Create a host and create the port;
        //    JHost = new JHost();
        //    var HostService = JHost.AddService(MeshServiceHost);
        //    var HostPort = JHost.AddHTTP(Service);
        //    HostService.AddPort(HostPort);

        //    // Create a client to connect to the service
        //    Session = new RemoteSession(Service, UserName);
        //    MeshService = new MeshServiceClient(Session);
        //    }


        //public void Do1() {
        //    var DevProfile = new SignedDeviceProfile(Device1, Device1Description);
        //    Console.WriteLine(DevProfile.ToString());


        //    // Create the Mesh Service
        //    Mesh = new Mesh(Service);

        //    DoCreateProfile();
        //    DoEscrowMasters();

        //    DoAddDevice();
        //    DoAddApp();
        //    DoAddDevice3();
        //    DoAddPassword();
        //    DoGetPassword();
        //    DoAddNetwork();
        //    }



        //public void DoCreateProfile() {
        //    var DevProfile = new SignedDeviceProfile(Device1, Device1Description);
        //    var UserProfile = new PersonalProfile(DevProfile);
        //    var PasswordProfile = new PasswordProfile(UserProfile);
        //    var SignedProfile = new SignedPersonalProfile(UserProfile);
        //    PasswordProfile.AddDevice(DevProfile);

        //    Mesh.AddProfile(SignedProfile);

        //    var SignedProfile2 = Mesh.GetSignedPersonalProfile(UserName);

        //    Console.WriteLine(SignedProfile2.ToString());


        //    var TheProfile = SignedProfile2.Signed;

        //    }


        //public void DoEscrowMasters( ) {
        //    var UserProfile = Mesh.GetPersonalProfile(UserName);           
        //    }


        //public void DoAddDevice( ) {
        //    var UserProfile = Mesh.GetPersonalProfile(UserName);
        //    var Dev = new SignedDeviceProfile (Device2, Device2Description);

        //    foreach (var Application in UserProfile.Applications) {
        //        Application.AddDevice(Dev);
        //        }

        //    UserProfile.Add(Dev);
        //    Mesh.UpdateProfile(UserProfile);
        //    }

        //public void DoAddApp( ) {
        //    var UserProfile = Mesh.GetPersonalProfile(UserName);
        //    var AppProfile = new MailProfile(UserProfile, MailAccount);
        //    AppProfile.Add(ConnectionSubmit);
        //    AppProfile.Add(ConnectionIMAP);

        //    foreach (var Device in UserProfile.Devices) {
        //        AppProfile.AddDevice(Device);
        //        }

        //    Mesh.UpdateProfile(UserProfile);
        //    }

        //public void DoAddPassword( ) {
        //    var UserProfile = Mesh.GetPersonalProfile(UserName);
        //    var PasswordApplication =
        //            PasswordProfile.Get(UserProfile);
        //    PasswordApplication.Add(PWDSite, PWDUser, PWDPassword);
        //    Mesh.UpdateProfile(UserProfile);
        //    }

        //public void DoAddDevice3( ) {
        //    var UserProfile = Mesh.GetPersonalProfile(UserName);
        //    var DeviceProfile = new SignedDeviceProfile(Device3, Device3Description);
        //    UserProfile.Add(DeviceProfile);
        //    foreach (var Application in UserProfile.Applications) {
        //        Application.AddDevice(DeviceProfile);
        //        }

        //    Mesh.UpdateProfile(UserProfile);
        //    }

        //public void DoGetPassword( ) {
        //    var UserProfile = Mesh.GetPersonalProfile(UserName);
        //    var PasswordApplicationProfile = PasswordProfile.Get(UserProfile);
        //    PasswordApplicationProfile.GetEntry(PWDSite,
        //        out PWDUserResult, out PWDPasswordResult);
        //    }

        //public void DoAddNetwork() {
        //    var UserProfile = Mesh.GetPersonalProfile(UserName);
        //    var AppProfile = new NetworkProfile(UserProfile);
        //    AppProfile.AddDNS(DNS1);
        //    AppProfile.AddDNS(DNS2);
        //    foreach (var Device in UserProfile.Devices) {
        //        AppProfile.AddDevice(Device);
        //        }

        //    Mesh.UpdateProfile(UserProfile);
        //    }

        }
    }