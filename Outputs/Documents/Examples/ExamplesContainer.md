The data payloads in all the following examples are identical, only the
authentication and/or encryption is different. 

* Frame 0 is omitted in each case

* Frame 1..n consists of 300 bytes being the byte sequence 00, 01, 02, etc. 
repeating after 256 bytes.

For conciseness, the raw data format is omitted for examples after the first, except
where the data payload has been transformed, (i.e. encrypted).


##Simple sequence

The following example shows a simple sequence with first frame and a single data frame:

~~~~
f4 91 
f0 8b 
f0 00 
f0 00 
91 f4 
f5 01 73 
f0 42 
f1 01 2c 
73 01 f5 
~~~~

Since there is no integrity check, there is no need for trailer entries.
The header values are:


Frame 0

~~~~
{
  "DareHeader":{
    "policy":{},
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "DataEncoding":"JSON",
      "ContainerType":"List",
      "Index":0}}}

[Empty trailer]
~~~~


Frame 1

~~~~
{
  "DareHeader":{
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":1}}}

[Empty trailer]
~~~~



##Payload and chain digests

The following example shows a chain sequence with a first frame and three 
data frames. The headers of these frames is the same as before but the
frames now have trailers specifying the PayloadDigest and ChainDigest values:


Frame 0

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "policy":{},
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "DataEncoding":"JSON",
      "ContainerType":"Chain",
      "Index":0}}}

[Empty trailer]
~~~~


Frame 1

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":1}}}

{
  "DareTrailer":{
    "PayloadDigest":"8dyi62d7MDJlsLm6_w4GEgKBjzXBRwppu6qbtmAl6UjZ
  DlZeaWQlBsYhOu88-ekpNXpZ2iY96zTRI229zaJ5sw",
    "ChainDigest":"T7S1FcrgY3AaWD4L-t5W1K-3XYkPTcOdGEGyjglTD6yMYV
  RVz9tn_KQc6GdA-P4VSRigBygV65OEd2Vv3YDhww"}}
~~~~


Frame 2

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":2}}}

{
  "DareTrailer":{
    "PayloadDigest":"8dyi62d7MDJlsLm6_w4GEgKBjzXBRwppu6qbtmAl6UjZ
  DlZeaWQlBsYhOu88-ekpNXpZ2iY96zTRI229zaJ5sw",
    "ChainDigest":"T7S1FcrgY3AaWD4L-t5W1K-3XYkPTcOdGEGyjglTD6yMYV
  RVz9tn_KQc6GdA-P4VSRigBygV65OEd2Vv3YDhww"}}
~~~~


Frame 3

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":3}}}

{
  "DareTrailer":{
    "PayloadDigest":"8dyi62d7MDJlsLm6_w4GEgKBjzXBRwppu6qbtmAl6UjZ
  DlZeaWQlBsYhOu88-ekpNXpZ2iY96zTRI229zaJ5sw",
    "ChainDigest":"T7S1FcrgY3AaWD4L-t5W1K-3XYkPTcOdGEGyjglTD6yMYV
  RVz9tn_KQc6GdA-P4VSRigBygV65OEd2Vv3YDhww"}}
~~~~


##Merkle Tree

The following example shows a chain sequence with a first frame and six 
data frames. The trailers now contain the TreePosition and TreeDigest
values:


Frame 0

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "policy":{},
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "DataEncoding":"JSON",
      "ContainerType":"Merkle",
      "Index":0}}}

[Empty trailer]
~~~~


Frame 1

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":1,
      "TreePosition":0}}}

{
  "DareTrailer":{
    "PayloadDigest":"8dyi62d7MDJlsLm6_w4GEgKBjzXBRwppu6qbtmAl6UjZ
  DlZeaWQlBsYhOu88-ekpNXpZ2iY96zTRI229zaJ5sw",
    "TreeDigest":"T7S1FcrgY3AaWD4L-t5W1K-3XYkPTcOdGEGyjglTD6yMYVR
  Vz9tn_KQc6GdA-P4VSRigBygV65OEd2Vv3YDhww"}}
~~~~


Frame 2

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":2,
      "TreePosition":392}}}

{
  "DareTrailer":{
    "PayloadDigest":"8dyi62d7MDJlsLm6_w4GEgKBjzXBRwppu6qbtmAl6UjZ
  DlZeaWQlBsYhOu88-ekpNXpZ2iY96zTRI229zaJ5sw",
    "TreeDigest":"7fHmkEIsPkN6sDYAOLvpIJn5Dg3PxDDAaq-ll2kh8722kok
  kFnZQcYtjuVC71aHNXI18q-lPnfRkmwryG-bhqQ"}}
~~~~


Frame 3

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":3,
      "TreePosition":392}}}

{
  "DareTrailer":{
    "PayloadDigest":"8dyi62d7MDJlsLm6_w4GEgKBjzXBRwppu6qbtmAl6UjZ
  DlZeaWQlBsYhOu88-ekpNXpZ2iY96zTRI229zaJ5sw",
    "TreeDigest":"T7S1FcrgY3AaWD4L-t5W1K-3XYkPTcOdGEGyjglTD6yMYVR
  Vz9tn_KQc6GdA-P4VSRigBygV65OEd2Vv3YDhww"}}
~~~~


Frame 4

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":4,
      "TreePosition":1676}}}

{
  "DareTrailer":{
    "PayloadDigest":"8dyi62d7MDJlsLm6_w4GEgKBjzXBRwppu6qbtmAl6UjZ
  DlZeaWQlBsYhOu88-ekpNXpZ2iY96zTRI229zaJ5sw",
    "TreeDigest":"vJ6ngNATvZcXSMALi5IUqzl1GBxBnTNVcC87VL_BhMRCbAv
  KSj8gs0VFgxxLkZ2myrtaDIwhHoswiTiBMLNWug"}}
~~~~


Frame 5

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":5,
      "TreePosition":1676}}}

{
  "DareTrailer":{
    "PayloadDigest":"8dyi62d7MDJlsLm6_w4GEgKBjzXBRwppu6qbtmAl6UjZ
  DlZeaWQlBsYhOu88-ekpNXpZ2iY96zTRI229zaJ5sw",
    "TreeDigest":"T7S1FcrgY3AaWD4L-t5W1K-3XYkPTcOdGEGyjglTD6yMYVR
  Vz9tn_KQc6GdA-P4VSRigBygV65OEd2Vv3YDhww"}}
~~~~


Frame 6

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":6,
      "TreePosition":2963}}}

{
  "DareTrailer":{
    "PayloadDigest":"8dyi62d7MDJlsLm6_w4GEgKBjzXBRwppu6qbtmAl6UjZ
  DlZeaWQlBsYhOu88-ekpNXpZ2iY96zTRI229zaJ5sw",
    "TreeDigest":"WgHlz3EHczVPqgtpc39Arv7CFIsCbFVsk8wg0j2qLlEfur9
  SZ0mdr65Ka-HF0Qx8gg_DAoiJwUrwADDXyxVJOg"}}
~~~~


##Signed sequence

The following example shows a tree sequence with a signature in the final record.
The signing key parameters are:



~~~~
{
  "PrivateKeyECDH":{
    "crv":"Ed25519",
    "Private":"_VPiqwO8EMWJQuFLRZbogEdAnVFXhdxfRBSUXgi4irE"}}
~~~~

The sequence headers and trailers are:


Frame 0

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "policy":{},
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "DataEncoding":"JSON",
      "ContainerType":"Merkle",
      "Index":0}}}

[Empty trailer]
~~~~


Frame 1

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":1,
      "TreePosition":0}}}

{
  "DareTrailer":{
    "PayloadDigest":"8dyi62d7MDJlsLm6_w4GEgKBjzXBRwppu6qbtmAl6UjZ
  DlZeaWQlBsYhOu88-ekpNXpZ2iY96zTRI229zaJ5sw",
    "TreeDigest":"T7S1FcrgY3AaWD4L-t5W1K-3XYkPTcOdGEGyjglTD6yMYVR
  Vz9tn_KQc6GdA-P4VSRigBygV65OEd2Vv3YDhww"}}
~~~~


Frame 2

~~~~
{
  "DareHeader":{
    "dig":"S512",
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":2,
      "TreePosition":392}}}

{
  "DareTrailer":{
    "PayloadDigest":"8dyi62d7MDJlsLm6_w4GEgKBjzXBRwppu6qbtmAl6UjZ
  DlZeaWQlBsYhOu88-ekpNXpZ2iY96zTRI229zaJ5sw",
    "TreeDigest":"7fHmkEIsPkN6sDYAOLvpIJn5Dg3PxDDAaq-ll2kh8722kok
  kFnZQcYtjuVC71aHNXI18q-lPnfRkmwryG-bhqQ"}}
~~~~


##Encrypted sequence

The following example shows a sequence in which all the frame payloads are encrypted 
under the same base seed established in a key agreement specified in the first frame.


Frame 0

~~~~
{
  "DareHeader":{
    "enc":"A256CBC",
    "kid":"EBQO-NKRC-HHKM-H62G-B2GN-BJLU-43RE",
    "Salt":"6ZmIGWV80_sx37jkV2mrCA",
    "recipients":[{
        "kid":"MBPQ-HJXB-LUTS-HZEC-NPCR-Q5FG-HQ37",
        "epk":{
          "PublicKeyECDH":{
            "crv":"Ed25519",
            "Public":"E5SluiIuvwGc-vSRWkfmzph2iChLHGk_VJNU4q6jW9U"}},
        "wmk":"eN3raQ9HUGkRMwrbHF4DU2etj7xpQolnrwfD0b9J9dSv0FnzI4
  XlCw"}
      ],
    "policy":{
      "enc":"none",
      "dig":"none",
      "EncryptKeys":[{
          "PublicKeyECDH":{
            "crv":"Ed25519",
            "Public":"P90U44SjLcR9eZBsXt9Dvi7R_U9iGv4-zGEfNuoYVHg"}}
        ],
      "Sealed":true},
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "DataEncoding":"JSON",
      "ContainerType":"List",
      "Index":0}}}

[Empty trailer]
~~~~


Frame 1

~~~~
{
  "DareHeader":{
    "enc":"A256CBC",
    "kid":"EBQK-FJWL-OTO2-BK2K-CT32-BIAX-6C4U",
    "Salt":"W1FJ846pZBi7Ry0biJYPYQ",
    "recipients":[{
        "kid":"MBPQ-HJXB-LUTS-HZEC-NPCR-Q5FG-HQ37",
        "epk":{
          "PublicKeyECDH":{
            "crv":"Ed25519",
            "Public":"cVhzNmlideaOCFOOqYqh4L0UP3LCAT0uD6enk-_Xa3E"}},
        "wmk":"DORBIok05nkBCFQWpFg_76_tmUC71EQjeCNcqB_fRfJ7Brqvo2
  EbIQ"}
      ],
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":1}}}

[Empty trailer]
~~~~


Frame 2

~~~~
{
  "DareHeader":{
    "enc":"A256CBC",
    "kid":"EBQC-RE6B-N6CP-XPQV-NQJL-PLAG-KDKQ",
    "Salt":"yt2E6BiEHOpKqnviJgZKZA",
    "recipients":[{
        "kid":"MBPQ-HJXB-LUTS-HZEC-NPCR-Q5FG-HQ37",
        "epk":{
          "PublicKeyECDH":{
            "crv":"Ed25519",
            "Public":"z9572OP7p1q_A_zd4tj4BQO6QAF-VE5JLJ0Ug2WpHVQ"}},
        "wmk":"TrIkSJXnh_1tDRZaTm647X6hj33n5pujjkWFEkTBu_5r8dwGnG
  1zKw"}
      ],
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":2}}}

[Empty trailer]
~~~~


Here are the sequence bytes. Note that the content is now encrypted and has expanded by
25 bytes. These are the salt (16 bytes), the AES padding (4 bytes) and the 
JSON-B framing (5 bytes).

~~~~
f5 02 ef 
f1 02 d8 
f0 10 
f0 00 
ef 02 f5 
f5 02 f9 
f1 01 c3 
f1 01 30 
f9 02 f5 
f5 02 f9 
f1 01 c3 
f1 01 30 
f9 02 f5 


~~~~


The following example shows a sequence in which all the frame payloads are encrypted 
under separate key agreements specified in the payload frames.


Frame 0

~~~~
{
  "DareHeader":{
    "policy":{
      "enc":"none",
      "dig":"none",
      "Sealed":true},
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "DataEncoding":"JSON",
      "ContainerType":"List",
      "Index":0}}}

[Empty trailer]
~~~~


Frame 1

~~~~
{
  "DareHeader":{
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":1}}}

[Empty trailer]
~~~~


Frame 2

~~~~
{
  "DareHeader":{
    "ContentMetaData":"e30",
    "SequenceInfo":{
      "Index":2}}}

[Empty trailer]
~~~~


