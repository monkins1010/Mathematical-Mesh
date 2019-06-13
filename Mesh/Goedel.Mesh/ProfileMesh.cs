﻿using System;
using System.Collections.Generic;
using System.Text;
using Goedel.Utilities;
using Goedel.Cryptography.Dare;
using Goedel.Cryptography;

namespace Goedel.Mesh {

    public partial class ActivationAccount {
        public AssertionAccountConnection AssertionAccountConnection => assertionAccountConnection ??
            AssertionAccountConnection.Decode(
                    EnvelopedAssertionAccountConnection).CacheValue(out assertionAccountConnection);
        AssertionAccountConnection assertionAccountConnection;

        }
    public partial class AssertionAccountConnection {
        public static new AssertionAccountConnection Decode(DareEnvelope message) {
            var result = FromJSON(message.GetBodyReader(), true);
            result.DareEnvelope = message;
            return result;
            }
        }

    public partial class AssertionAccount {

        public override string _PrimaryKey => UDF;
        public string UDF => AccountEncryptionKey.UDF;
        //public byte[] UDFBytes => ProfileMaster.UDFBytes;


        //public ProfileMaster ProfileMaster => profileMaster ??
        //    ProfileMaster.Decode(MasterProfile).CacheValue(out profileMaster);
        //ProfileMaster profileMaster = null;


        public static new AssertionAccount Decode(DareEnvelope message) {
            var result = FromJSON(message.GetBodyReader(), true);
            result.DareEnvelope = message;
            return result;
            }

        }
    }
