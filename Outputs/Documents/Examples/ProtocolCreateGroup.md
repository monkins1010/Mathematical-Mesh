
The request payload:


~~~~
{
  "BindAccount": {
    "AccountAddress": "groupw@example.com",
    "EnvelopedProfileAccount": [{
        "EnvelopeID": "MAXH-HSGW-JFNX-HQMR-A3EF-4ST3-DXJU",
        "dig": "S512",
        "ContentMetaData": "ewogICJVbmlxdWVJRCI6ICJNQVhILUhTR1ctSkZOWC1
  IUU1SLUEzRUYtNFNUMy1EWEpVIiwKICAiTWVzc2FnZVR5cGUiOiAiUHJvZmlsZ
  Udyb3VwIiwKICAiY3R5IjogImFwcGxpY2F0aW9uL21tbS9vYmplY3QiLAogICJ
  DcmVhdGVkIjogIjIwMjAtMTAtMjhUMjM6MDE6NDBaIn0"},
      "ewogICJQcm9maWxlR3JvdXAiOiB7CiAgICAiUHJvZml
  sZVNpZ25hdHVyZSI6IHsKICAgICAgIlVkZiI6ICJNQVhILUhTR1ctSkZOWC1IU
  U1SLUEzRUYtNFNUMy1EWEpVIiwKICAgICAgIlB1YmxpY1BhcmFtZXRlcnMiOiB
  7CiAgICAgICAgIlB1YmxpY0tleUVDREgiOiB7CiAgICAgICAgICAiY3J2IjogI
  kVkNDQ4IiwKICAgICAgICAgICJQdWJsaWMiOiAiOHZfYktKTERkdVFHbVFkNl9
  la25iM21jNlJXV09teVVHczhMUUtjMEg2bVU3bW0zSzE1UwogIDM3TVhsZmlVc
  zdZSUV5N2hIemdZTS1jQSJ9fX0sCiAgICAiQWNjb3VudEFkZHJlc3MiOiAiZ3J
  vdXB3QGV4YW1wbGUuY29tIiwKICAgICJBY2NvdW50RW5jcnlwdGlvbiI6IHsKI
  CAgICAgIlVkZiI6ICJNQUJTLUVUTDYtNlRNQS1JRlNVLU5JSEMtTUxHVi1WQ0R
  DIiwKICAgICAgIlB1YmxpY1BhcmFtZXRlcnMiOiB7CiAgICAgICAgIlB1YmxpY
  0tleUVDREgiOiB7CiAgICAgICAgICAiY3J2IjogIlg0NDgiLAogICAgICAgICA
  gIlB1YmxpYyI6ICJHTXhvMUY2a192SWdHc0w5Q01YeFdZMEE4czdkM2ZyMVc2b
  Eh1M280U1pvVF8zOEJ4T1ZICiAgbFRNNGNpUU52bGRmeTZiTDBHejlmdnlBIn1
  9fSwKICAgICJBZG1pbmlzdHJhdG9yU2lnbmF0dXJlIjogewogICAgICAiVWRmI
  jogIk1BWEgtSFNHVy1KRk5YLUhRTVItQTNFRi00U1QzLURYSlUiLAogICAgICA
  iUHVibGljUGFyYW1ldGVycyI6IHsKICAgICAgICAiUHVibGljS2V5RUNESCI6I
  HsKICAgICAgICAgICJjcnYiOiAiRWQ0NDgiLAogICAgICAgICAgIlB1YmxpYyI
  6ICI4dl9iS0pMRGR1UUdtUWQ2X2VrbmIzbWM2UldXT215VUdzOExRS2MwSDZtV
  TdtbTNLMTVTCiAgMzdNWGxmaVVzN1lJRXk3aEh6Z1lNLWNBIn19fX19",
      {
        "signatures": [{
            "alg": "S512",
            "kid": "MAXH-HSGW-JFNX-HQMR-A3EF-4ST3-DXJU",
            "signature": "7hIc5HvdlwpV6Y9MG9Ghf0EV4ODLHyMf94qrNFzyg_dFesBO1
  Iv6COuAItVtJeR8kc9Zxv4ryC4A3qeViUyPS2cuWwYyBsHaHIiYIjjxLYtDCW7
  f-LHtq_y8L4cwS4DTVIW7r9G5fEgU7WBbCHzVJTcA"}],
        "PayloadDigest": "i4ugMq2-bw3lfir-R7EE7IEtncH_vahnRqovuS6Dv3AxA
  YfhJk0BHQSwCduvKq18tLyTOE52xUcLEO5S1OT6gQ"}]}}
~~~~

The response payload:


~~~~
{
  "BindResponse": {
    "Status": 201,
    "StatusDescription": "Operation completed successfully"}}
~~~~
