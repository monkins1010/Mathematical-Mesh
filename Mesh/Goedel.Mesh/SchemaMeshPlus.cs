﻿using System;
using System.Collections.Generic;
using System.Text;
using Goedel.Cryptography;
using Goedel.Cryptography.PKIX;
using Goedel.Cryptography.Dare;
using Goedel.Utilities;
using Goedel.Protocol;
namespace Goedel.Mesh {

    public partial class MeshItem {

        /// <summary>
        /// The DareEnvelope encapsulation of this object instance.
        /// </summary>
        public virtual DareEnvelope DareEnvelope { get; protected set; }


        public static object Initialize => null;

        static MeshItem() => AddDictionary(ref _TagDictionary);


        /// <summary>
        /// Deserialize a tagged stream
        /// </summary>
        /// <param name="JSONReader">The input stream</param>
        /// <param name="Tagged">If true, the input is wrapped in a tag specifying the type</param>
        /// <returns>The created object.</returns>		
        public static new MeshItem FromJSON(JSONReader JSONReader, bool Tagged = true) {
            if (JSONReader == null) {
                return null;
                }
            if (Tagged) {
                var Out = JSONReader.ReadTaggedObject(_TagDictionary);
                return Out as MeshItem;
                }
            throw new CannotCreateAbstract();
            }

        public static MeshItem Decode(DareEnvelope message) {
            var result = FromJSON(message.GetBodyReader(), true);
            result.DareEnvelope = message;
            return result;
            }


        }

    public partial class Assertion {
        public virtual DareEnvelope Encode (KeyPair keyPair) {
            DareEnvelope = DareEnvelope.Encode(GetBytes(tag: true),
                signingKey: keyPair);
            return DareEnvelope;
            }

        public DareEnvelope Sign(KeyPair SignatureKey) {
            DareEnvelope = DareEnvelope.Encode(GetBytes(true), signingKey: SignatureKey);
            return DareEnvelope;
            }

        public static new Assertion Decode(DareEnvelope message) {
            var result = FromJSON(message.GetBodyReader(), true);
            result.DareEnvelope = message;
            return result;
            }

        }


    }