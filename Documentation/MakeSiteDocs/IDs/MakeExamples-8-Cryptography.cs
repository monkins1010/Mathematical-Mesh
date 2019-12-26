using  System.Text;
using  Goedel.Mesh;
using  Goedel.Protocol;
using  Goedel.Utilities;
 #pragma warning disable IDE0022
 #pragma warning disable IDE0060
 #pragma warning disable IDE1006
using System;
using System.IO;
using System.Collections.Generic;
using Goedel.Registry;
namespace ExampleGenerator {
	public partial class CreateExamples : global::Goedel.Registry.Script {

		

		//
		// MakeCryptographyExamples
		//
		public void MakeCryptographyExamples (CreateExamples Example) {
			 ExamplesThreshold(Example);
			}
		

		//
		// ExamplesThreshold
		//
		public static void ExamplesThreshold(CreateExamples Example) { /* XFile  */
				using (var _Output = new StreamWriter("Examples\\ExamplesThreshold.md")) {
				var obj = new CreateExamples() { _Output = _Output, _Indent = "", _Filename = "Examples\\ExamplesThreshold.md" };
				obj._ExamplesThreshold(Example);
				}
			}
		public void _ExamplesThreshold(CreateExamples Example) {

				 var threshold = Example.Threshold;
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("## Threshold Key Generation\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("### X25519\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				DescribeKeyGen (threshold.KeyGenX25519);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("### X448\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				DescribeKeyGen (threshold.KeyGenX448);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("### Ed25519\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				DescribeKeyGen (threshold.KeyGenEd25519);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("### Ed448\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				DescribeKeyGen (threshold.KeyGenEd448);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("## Threshold Decryption\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("### Key Splitting X25519\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				DescribeDecryptSplitting (threshold.DecryptX25519);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("### Decryption X25519\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				DescribeDecryptUse (threshold.DecryptX25519);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("### Key Splitting X448\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				DescribeDecryptSplitting (threshold.DecryptX448);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("### Decryption X448\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				DescribeDecryptUse (threshold.DecryptX448);
				_Output.Write ("\n{0}", _Indent);
					}
		

		//
		// DescribeDecryptUse
		//
		public void DescribeDecryptUse (Decrypt Decrypt) {
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("The means of encryption is unchanged. We begin by generating an ephemeral \n{0}", _Indent);
			_Output.Write ("key pair:\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			DescribeKey (Decrypt.KeyE);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("The key agreement result is given by multiplying the public key of the encryption \n{0}", _Indent);
			_Output.Write ("pair by the secret scalar of the ephemeral key to obtain the u-coordinate of the result.\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("{1}: {2}\n{0}", _Indent, Decrypt.KeyEA.XTag, Decrypt.KeyEA.X);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("The u-coordinate is encoded in the usual fashion (i.e. without specifying the sign of v).\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("{1}\n{0}", _Indent, Decrypt.KeyEA.Public.ToStringBase16FormatHex());
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("The first decryption contribution is generated from the secret scalar of the first key\n{0}", _Indent);
			_Output.Write ("share and the public key of the ephemeral.\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("The outputs from the Montgomery Ladder are:\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("x_2 {1}\n{0}", _Indent, Decrypt.KeyE1.X2);
			_Output.Write ("z_2 {1}\n{0}", _Indent, Decrypt.KeyE1.Z2);
			_Output.Write ("x_3 {1}\n{0}", _Indent, Decrypt.KeyE1.X3);
			_Output.Write ("z_3 {1}\n{0}", _Indent, Decrypt.KeyE1.Z3);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("The coordinates of the corresponding point are:\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("u {1}\n{0}", _Indent, Decrypt.KeyE1.X);
			_Output.Write ("v {1}\n{0}", _Indent, Decrypt.KeyE1.Y);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("The encoding of this point specifies the u coordinate and the sign (oddness) of the \n{0}", _Indent);
			_Output.Write ("v coordinate:\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("{1}\n{0}", _Indent, Decrypt.KeyE1.Public.ToStringBase16FormatHex());
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("The second decryption contribution is generated from the secret scalar of the second key\n{0}", _Indent);
			_Output.Write ("share and the public key of the ephemeral in the same way:\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("u {1}\n{0}", _Indent, Decrypt.KeyE2.X);
			_Output.Write ("v {1}\n{0}", _Indent, Decrypt.KeyE2.Y);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("{1}\n{0}", _Indent, Decrypt.KeyE2.Public.ToStringBase16FormatHex());
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("To obtain the key agreement value, we add the two decryption contributions:\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("u {1}\n{0}", _Indent, Decrypt.KeyE12.X);
			_Output.Write ("v {1}\n{0}", _Indent, Decrypt.KeyE12.Y);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("This returns the same u coordinate value as before, allowing us to obtain the encoding \n{0}", _Indent);
			_Output.Write ("of the key agreement value and decrypt the message.\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			}
		

		//
		// DescribeDecryptSplitting
		//
		public void DescribeDecryptSplitting (Decrypt Decrypt) {
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("The encryption key pair is\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			DescribeKey (Decrypt.KeyA);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("To create n key shares we first create n-1 key pairs in the normal fashion. Since \n{0}", _Indent);
			_Output.Write ("these key pairs are only used for decryption operations, it is not necessary to \n{0}", _Indent);
			_Output.Write ("calculate the public components:\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			DescribeKeyPrivate (Decrypt.Key1);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("The secret scalar of the final key share is the secret scalar of the base key minus\n{0}", _Indent);
			_Output.Write ("the sum of the secret scalars of the other shares modulo the group order:\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("Scalar_2 = (Scalar_A - Scalar_1) mod L\n{0}", _Indent);
			_Output.Write ("    = {1}\n{0}", _Indent, Decrypt.Key2.Scalar);
			_Output.Write ("This is encoded as a binary integer in little endian format:\n{0}", _Indent);
			_Output.Write ("{1}\n{0}", _Indent, Decrypt.Key2.Private.ToStringBase16FormatHex());
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			}
		

		//
		// DescribeKeyGen
		//
		public void DescribeKeyGen (KeyGen KeyGen) {
			_Output.Write ("The key parameters of the first key contribution are:\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			DescribeKey (KeyGen.Key1);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("The key parameters of the second key contribution are:\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			DescribeKey (KeyGen.Key2);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("The aggregate private key is:\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("Scalar_A = (Scalar_1 + Scalar_2) mod L\n{0}", _Indent);
			_Output.Write ("  = {1}\n{0}", _Indent, KeyGen.KeyA.Scalar);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("Encoded Aggrgate Private Key:\n{0}", _Indent);
			_Output.Write ("{1}\n{0}", _Indent, KeyGen.KeyA.Private.ToStringBase16FormatHex());
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("The aggregate public key is:\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("Point_A = Point_1 + Point_2\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("{1}: {2}\n{0}", _Indent, KeyGen.KeyA.XTag, KeyGen.KeyA.X);
			_Output.Write ("{1}: {2}\n{0}", _Indent, KeyGen.KeyA.YTag, KeyGen.KeyA.Y);
			_Output.Write ("\n{0}", _Indent);
			_Output.Write ("Encoded Public{1}\n{0}", _Indent, KeyGen.KeyA.Public.ToStringBase16FormatHex());
			_Output.Write ("~~~~\n{0}", _Indent);
			_Output.Write ("\n{0}", _Indent);
			if (  (KeyGen.KeyA.IsCurveX)  ) {
				_Output.Write ("Note that in this case, the unsigned representation of the key is used as\n{0}", _Indent);
				_Output.Write ("the aggregate key is intended for unsigned CurveX key agreement. If the\n{0}", _Indent);
				_Output.Write ("result is intended for use as a key contribution, the signed representation\n{0}", _Indent);
				_Output.Write ("is required.\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				}
			}
		

		//
		// DescribeKeyPrivate
		//
		public void DescribeKeyPrivate (CurveKey Key) {
			_Output.Write ("{1} ({2})\n{0}", _Indent, Key.Name, Key.Curve);
			_Output.Write ("    UDF:        {1}\n{0}", _Indent, Key.UDF);
			_Output.Write ("    Scalar:     {1}\n{0}", _Indent, Key.Scalar);
			_Output.Write ("    Encoded Private{1}\n{0}", _Indent, Key.Private.ToStringBase16FormatHex());
			}
		

		//
		// DescribeKey
		//
		public void DescribeKey (CurveKey Key) {
			DescribeKeyPrivate (Key);
			_Output.Write ("    {1}: {2}\n{0}", _Indent, Key.XTag, Key.X);
			_Output.Write ("    {1}: {2}\n{0}", _Indent, Key.YTag, Key.Y);
			_Output.Write ("    Encoded Public{1}\n{0}", _Indent, Key.Public.ToStringBase16FormatHex());
			}
		

		//
		// ExamplesAdvancedQuantum
		//
		public static void ExamplesAdvancedQuantum (CreateExamples Example) { /* File  */
			using (var _Output = new StreamWriter ("Examples\\ExamplesAdvancedQuantum.md")) {
				var _Indent = ""; 
				_Output.Write ("##Example: Creating a Quantum Resistant Signature Fingerprint\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("Alice decides to add a QRSF to her Mesh Profile. She creates\n{0}", _Indent);
				_Output.Write ("a 256 bit master secret.\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("TBS: {1}\n{0}", _Indent, Example.AdvancedQuantumMasterSecret);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("To enable recovery of the master key, Alice creates five keyshares with a quorum of three:\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("TBS: {1}\n{0}", _Indent, Example.AdvancedQuantumShares);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("Alice uses the master secret to derrive her private key values:\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("TBS: {1}\n{0}", _Indent, Example.AdvancedQuantumPrivate);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("These values are used to generate the public key value:\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("TBS: {1}\n{0}", _Indent, Example.AdvancedQuantumPublic);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("The QRSF contains the UDF fingerprint of the public key\n{0}", _Indent);
				_Output.Write ("value plus the XMSS parameters:\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("TBS: {1}\n{0}", _Indent, Example.AdvancedQuantumPublicUDF);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("Alice adds the QRSF to her profile and publishes it to a Mesh Service that is enrolled\n{0}", _Indent);
				_Output.Write ("in at least one multi-party notary scheme.\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				}
			}
		

		//
		// Blahhh
		//
		public static void Blahhh (CreateExamples Example) { /* File  */
			using (var _Output = new StreamWriter ("Examples\\Blahhh.md")) {
				var _Indent = ""; 
				_Output.Write ("The Recryption entry consists of Bob's address, the recryption key and the decryption\n{0}", _Indent);
				_Output.Write ("key encrypted under Bob's encryption key:\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("TBS: {1}\n{0}", _Indent, Example.AdvancedRecryptionBobRecryptionEntry);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("The group administration tool creates a notification request to tell Bob that\n{0}", _Indent);
				_Output.Write ("he has been made a member of the new group and signs it using the group signature\n{0}", _Indent);
				_Output.Write ("key. The recryption entry and the notification are then sent to the recryption\n{0}", _Indent);
				_Output.Write ("service:\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("TBS: {1}\n{0}", _Indent, Example.AdvancedRecryptionRecryptionAddMemberRequest);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("The notification message contains a link to the test message. When he accepts\n{0}", _Indent);
				_Output.Write ("membership of the group, Bob clicks on the link to test that his membership\n{0}", _Indent);
				_Output.Write ("has been fully provisioned. Providing an explicit test mechanism avoids the problem\n{0}", _Indent);
				_Output.Write ("that might otherwise occur in which the message spool fills up with test messages \n{0}", _Indent);
				_Output.Write ("being posted.\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("Bob's Web browser requests the recryption data for the test message. The request is \n{0}", _Indent);
				_Output.Write ("authenticated and encrypted under Bob's device keys. The plaintext of the message is:\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("TBS: {1}\n{0}", _Indent, Example.AdvancedRecryptionRecryptionRecryptionRequest);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("The plaintext of the response contains the additional information Bob's Web browser\n{0}", _Indent);
				_Output.Write ("needs to complete the decryption process:\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("TBS: {1}\n{0}", _Indent, Example.AdvancedRecryptionRecryptionRecryptionResponse);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("The Web browser decrypts the private key and uses it to calculate the decryption \n{0}", _Indent);
				_Output.Write ("value:\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("TBS: {1}\n{0}", _Indent, Example.AdvancedRecryptionDecryptionValue);
				_Output.Write ("~~~~\n{0}", _Indent);
				_Output.Write ("\n{0}", _Indent);
				_Output.Write ("The key agreement value is obtained by point addition of the recryption and decryption\n{0}", _Indent);
				_Output.Write ("values:\n{0}", _Indent);
				}
			}
		}
	}
