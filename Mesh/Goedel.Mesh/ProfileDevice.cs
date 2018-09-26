﻿using System;
using System.Collections.Generic;
using System.Text;
using Goedel.Cryptography;
using Goedel.Cryptography.Dare;
using Goedel.Utilities;
using Goedel.Protocol;
namespace Goedel.Mesh {


    public partial class ProfileDevice {

        /// <summary>
        /// The signed device profile
        /// </summary>
        public DareMessage ProfileDeviceSigned { get; private set; }

        /// <summary>
        /// Constructor for use by deserializers.
        /// </summary>
        public ProfileDevice() {
            }

        /// <summary>
        /// Create a new master profile.
        /// </summary>
        /// <param name="AlgorithmSign"></param>
        /// <param name="AlgorithmEncrypt"></param>
        public static ProfileDevice Generate(
                        KeyPair keyPublicSign,
                        KeyPair keyPublicEncrypt,
                        KeyPair keyPublicAuthenticate) {

            var ProfileDevice = new ProfileDevice() {
                DeviceSignatureKey = new PublicKey (keyPublicSign),
                DeviceAuthenticationKey = new PublicKey(keyPublicAuthenticate),
                DeviceEncryptiontionKey = new PublicKey(keyPublicEncrypt)
                };

            var bytes = ProfileDevice.GetBytes(tag:true);

            ProfileDevice.ProfileDeviceSigned = DareMessage.Encode(bytes,
                    SigningKey: keyPublicSign, ContentType: "application/mmm");


            return ProfileDevice;


            }

        



        }
    }
