﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Goedel.Utilities;
using Goedel.Cryptography;
using Goedel.Cryptography.Dare;
using Goedel.Cryptography.Jose;
using Goedel.Mesh;

namespace Goedel.Mesh.Client {

    public partial class ContextMesh : Disposable {
        ///<summary>The Machine context.</summary>
        public IMeshMachineClient MeshMachine { get; }

        ///<summary>The master profile</summary>
        public ProfileMesh ProfileMesh { get; }

        ///<summary>The Device Entry in the CatalogHost</summary>
        public CatalogedMachine CatalogedMachine;


        ///<summary>Convenience property returning the device connections</summary>
        CatalogedStandard DeviceConnection => CatalogedMachine as CatalogedStandard;

        ///<summary>For a non administrative device, the CatalogEntryDevice is in the 
        ///connection entry;</summary>
        public CatalogedDevice CatalogedDevice => CatalogedMachine.CatalogedDevice;

        public ConnectionDevice ConnectionDevice =>
            CatalogedDevice.ConnectionDevice;



        /////<summary>The device profile to which the signature key is bound</summary>
        //public ProfileDevice profileDevice { get; }

        ActivationDevice AssertionDevicePrivate => assertionDevicePrivate ??
            ActivationDevice.Decode(
                MeshMachine, CatalogedDevice.EnvelopedActivationDevice).CacheValue(
                    out assertionDevicePrivate);

        ActivationDevice assertionDevicePrivate = null;

        ///<summary>The context as an administration context.</summary>
        public ContextMeshAdmin ContextMeshAdmin => this as ContextMeshAdmin;





        public ContextMesh(IMeshMachineClient meshMachine, CatalogedMachine deviceConnection) {
            Assert.AssertNotNull(deviceConnection, NYI.Throw);

            MeshMachine = meshMachine;
            CatalogedMachine = deviceConnection;

            ProfileMesh = ProfileMesh.Decode(CatalogedMachine.EnvelopedProfileMaster);
            
            }

        // The account activation was not added to activations.

        public ContextAccount GetContextAccount(
                string localName=null,
                string accountName = null) {

            var activation = AssertionDevicePrivate.GetActivation(accountName);

            return new ContextAccount (this, activation);

            }


        public void UpdateDevice(CatalogedDevice catalogedDevice) {

            CatalogedMachine.CatalogedDevice = catalogedDevice;
            MeshMachine.Register(CatalogedMachine);
            }

        public void UpdateAccount(ProfileAccount profileUpdate) {

            CatalogedMachine.EnvelopedProfileAccount = CatalogedMachine.EnvelopedProfileAccount ??
                new List<DareEnvelope>();

            bool found = false;
            foreach (var envelope in CatalogedMachine.EnvelopedProfileAccount) {
                var profileAccount = ProfileAccount.Decode(envelope);
                if (profileAccount.UDF == profileUpdate.UDF) {
                    found = true;
                    profileAccount.ServiceIDs = profileUpdate.ServiceIDs;
                    }
                }
            if (!found) {
                CatalogedMachine.EnvelopedProfileAccount.Add(profileUpdate.DareEnvelope);
                }

            MeshMachine.Register(CatalogedMachine);
            }



        }


    public class ContextMeshPending : ContextMesh {

        public CatalogedPending PendingConnection => CatalogedMachine as CatalogedPending;
        AcknowledgeConnection MessageConnectionResponse => PendingConnection?.MessageConnectionResponse;
        RequestConnection MessageConnectionRequest => MessageConnectionResponse?.MessageConnectionRequest;

        ProfileDevice ProfileDevice => MessageConnectionRequest?.ProfileDevice;

        KeyPair KeyAuthentication;

        public string ServiceID => MessageConnectionRequest?.ServiceID;
        public MeshService MeshClient;

        public ContextMeshPending(IMeshMachineClient meshMachine, CatalogedMachine deviceConnection) :
                    base(meshMachine, deviceConnection) {
            }



        public static ContextMeshPending ConnectService(
                IMeshMachineClient meshMachine,
                string serviceID,
                string localName = null,
                string PIN = null,
                CryptoAlgorithmID algorithmSign = CryptoAlgorithmID.Default,
                CryptoAlgorithmID algorithmEncrypt = CryptoAlgorithmID.Default,
                CryptoAlgorithmID algorithmAuthenticate = CryptoAlgorithmID.Default) {
            var profileDevice = ProfileDevice.Generate(meshMachine,
                algorithmSign: algorithmSign, algorithmEncrypt: algorithmEncrypt,
                algorithmAuthenticate: algorithmAuthenticate);

            return ConnectService(meshMachine, profileDevice, serviceID, localName, PIN);
            }

        

        public static ContextMeshPending ConnectService(
                IMeshMachineClient meshMachine,
                ProfileDevice profileDevice,
                string serviceID,
                string localName = null,
                string PIN = null) {

            // generate MessageConnectionRequestClient
            var messageConnectionRequestClient = new RequestConnection() {
                ServiceID = serviceID,
                EnvelopedProfileDevice = profileDevice.DareEnvelope,
                ClientNonce = CryptoCatalog.GetBits(128),
                PinUDF = UDF.PIN2PinID(PIN)
                };

            

            var keyAuthentication = meshMachine.KeyCollection.LocatePrivate(
                        profileDevice.KeyAuthentication.UDF);

            var messageConnectionRequestClientEncoded = messageConnectionRequestClient.Encode(keyAuthentication);

            // Acquire ephemeral client. This will only be used for the Connect and Complete methods.
            var meshClient = meshMachine.GetMeshClient(serviceID, keyAuthentication, null);

            var connectRequest = new ConnectRequest() {
                MessageConnectionRequestClient = messageConnectionRequestClientEncoded
                };

            var response = meshClient.Connect(connectRequest);

            // create the pending connection here

            var connection = new CatalogedPending() {
                ID = profileDevice.UDF,
                DeviceUDF = profileDevice.UDF,
                EnvelopedMessageConnectionResponse = response.EnvelopedConnectionResponse,
                EnvelopedProfileMaster = response.EnvelopedProfileMaster,
                EnvelopedAccountAssertion = response.EnvelopedAccountAssertion
                };

            meshMachine.Register(connection);

            return new ContextMeshPending(meshMachine, connection);

            }

        /// <summary>
        /// Complete the pending connection request.
        /// </summary>
        /// <returns>If successfull returns an ContextAccountService instance to allow access
        /// to the connected account. Otherwise, a null value is returned.</returns>
        public ContextAccount Complete() {

            KeyAuthentication = KeyAuthentication ?? MeshMachine.KeyCollection.LocatePrivate(
                        ProfileDevice.KeyAuthentication.UDF);

            MeshClient = MeshClient ?? MeshMachine.GetMeshClient(ServiceID, KeyAuthentication, null);

            var completeRequest = new CompleteRequest() {
                DeviceUDF = ProfileDevice.UDF,
                ServiceID = ServiceID
                };

            var statusResponse = MeshClient.Complete(completeRequest);


            throw new NYI();
            }






        //protected MeshService GetMeshClient(string serviceID) => 
        //    MeshMachine.GetMeshClient(serviceID, KeyAuthentication,
        //        ActivationAccount.AssertionAccountConnection, ContextMesh.ProfileMesh);

        }
    }