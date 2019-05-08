

# device

````
device    Device management commands.
    accept   Accept a pending connection
    auth   Authorize device to use application
    create   Create new device profile
    delete   Remove device from device catalog
    earl   Connect a new device by means of an EARL
    init   Create an initialization 
    list   List devices in the device catalog
    pending   Get list of pending connection requests
    pin   Accept a pending connection
    pre   Create a preconnection request
    reject   Reject a pending connection
    request   Connect to an existing profile registered at a portal
````

# device accept

````
accept   Accept a pending connection
       Fingerprint of connection to accept
       Device identifier
    /auth   Authorize the specified function
    /admin   Authorize device as administration device
    /all   Authorize device for all application catalogs
    /bookmark   Authorize response to confirmation requests
    /calendar   Authorize access to calendar catalog
    /contact   Authorize access to contacts catalog
    /confirm   Authorize response to confirmation requests
    /mail   Authorize access to configure SMTP mail services.
    /network   Authorize access to the network catalog
    /password   Authorize access to the password catalog
    /ssh   Authorize use of SSH
    /mesh   Account identifier (e.g. alice@example.com) or profile fingerprint
    /verbose   Verbose reports (default)
    /report   Report output (default)
    /json   Report output in JSON format
````

Accept a pending connection request.


````
>device accept tbs
ERROR - The feature has not been implemented
````

Specifying the /json option returns a result of type Result:

````
>device accept tbs /json
{
  "Result": {
    "Success": false,
    "Reason": "The feature has not been implemented"}}
````

# device auth

````
auth   Authorize device to use application
       Device identifier
    /auth   Authorize the specified function
    /admin   Authorize device as administration device
    /all   Authorize device for all application catalogs
    /bookmark   Authorize response to confirmation requests
    /calendar   Authorize access to calendar catalog
    /contact   Authorize access to contacts catalog
    /confirm   Authorize response to confirmation requests
    /mail   Authorize access to configure SMTP mail services.
    /network   Authorize access to the network catalog
    /password   Authorize access to the password catalog
    /ssh   Authorize use of SSH
    /mesh   Account identifier (e.g. alice@example.com) or profile fingerprint
    /verbose   Verbose reports (default)
    /report   Report output (default)
    /json   Report output in JSON format
````

The `device auth` command changes the set of authorizations given to the
specified device, adding or removing authorizations according to the 
flags specified on the command line.

The parameter specifies the device being configured by means of either
the UDF of the device profile or the device identifier.

The `/id` option may be used to specify a friendly name for the device.

Specifying the `/all` option causes the device to be granted all the 
available device authorizations except for those explicitly denied 
by means of a negative authorization grant (e.g. `/nobookmark`).

Specifying the `/noall` option causes the device to be granted no 
available device authorizations except for those explicitly granted 
by means of a positive authorization grant (e.g. `/bookmark`).

If neither the `/all` option or the `/noall` option is specified, the 
device authorizations remain unchanged except where explicitly 
granted or denied.

The following authorizations may be granted or denied:

* `bookmark`: Authorize response to confirmation requests
* `calendar`: Authorize access to calendar catalog
* `contact`: Authorize access to contacts catalog
* `confirm`: Authorize response to confirmation requests
* `mail`: Authorize access to configure SMTP mail services.
* `network`: Authorize access to the network catalog
* `password`: Authorize access to password catalog
* `ssh`: Authorize use of SSH


````
>device auth Alice2 /contact
ERROR - The feature has not been implemented
````

Specifying the /json option returns a result of type Result:

````
>device auth Alice2 /contact /json
{
  "Result": {
    "Success": false,
    "Reason": "The feature has not been implemented"}}
````

# device accept

````
accept   Accept a pending connection
       Fingerprint of connection to accept
       Device identifier
    /auth   Authorize the specified function
    /admin   Authorize device as administration device
    /all   Authorize device for all application catalogs
    /bookmark   Authorize response to confirmation requests
    /calendar   Authorize access to calendar catalog
    /contact   Authorize access to contacts catalog
    /confirm   Authorize response to confirmation requests
    /mail   Authorize access to configure SMTP mail services.
    /network   Authorize access to the network catalog
    /password   Authorize access to the password catalog
    /ssh   Authorize use of SSH
    /mesh   Account identifier (e.g. alice@example.com) or profile fingerprint
    /verbose   Verbose reports (default)
    /report   Report output (default)
    /json   Report output in JSON format
````

The `device accept` command accepts the specified connection request.

The command must specify the connection identifier of the request 
being accepted. The connection identifier may be abbreviated provided that
this uniquely identifies the connection being accepted and that at least 
four characters are given.

The `/id` option may be used to specify a friendly name for the device.

The authorizations to be granted to the device may be specified using
the same syntax as for the `device auth` command with the default authorization
being that all authorizations are denied.


````
>device accept tbs
ERROR - The feature has not been implemented
````

Specifying the /json option returns a result of type Result:

````
>device accept tbs /json
{
  "Result": {
    "Success": false,
    "Reason": "The feature has not been implemented"}}
````

# device create

````
create   Create new device profile
       Device identifier
       Device description
    /alg   List of algorithm specifiers
    /default   Make the new device profile the default
````

The `device create` command creates a new device profile without attempting
to connect the profile to a Mesh service account or profile.

This command allows a device to be preconfigured during manufacture or
site configuration before delivery or assignment to an indivdual user.

The `/id` and `/dd` options allow a device identifier and description to be 
assigned to the device.

The profile is made the default profile for the device if either there is
no previous default device profile or the`/default` option is specified.

**Missing Example***

# device delete

````
delete   Remove device from device catalog
       Device identifier
    /mesh   Account identifier (e.g. alice@example.com) or profile fingerprint
    /verbose   Verbose reports (default)
    /report   Report output (default)
    /json   Report output in JSON format
````

The `device delete` command removes the specified device from the catalog.

The parameter specifies the device being configured by means of either
the UDF of the device profile or the device identifier.


````
>device delete tbs
ERROR - Object reference not set to an instance of an object.
````

Specifying the /json option returns a result of type Result:

````
>device delete tbs /json
{
  "Result": {
    "Success": false,
    "Reason": "Object reference not set to an instance of an object."}}
````

# device earl

````
earl   Connect a new device by means of an EARL
       The EARL locator
       Device identifier
    /auth   Authorize the specified function
    /admin   Authorize device as administration device
    /all   Authorize device for all application catalogs
    /bookmark   Authorize response to confirmation requests
    /calendar   Authorize access to calendar catalog
    /contact   Authorize access to contacts catalog
    /confirm   Authorize response to confirmation requests
    /mail   Authorize access to configure SMTP mail services.
    /network   Authorize access to the network catalog
    /password   Authorize access to the password catalog
    /ssh   Authorize use of SSH
    /mesh   Account identifier (e.g. alice@example.com) or profile fingerprint
````

The `device earl` command attempts to connect a device to a personal profile
by means of an EARL UDF.

The EARL is typically presented to the administration device in the form of
a QR code which is decoded and passed to the meshman application.

The `/id` option may be used to specify a friendly name for the device if the
connection attempt succeeds.

The authorizations to be granted to the device may be specified using
the same syntax as for the `device auth` command with the default authorization
being that all authorizations are denied.


````
>device earl udf://example.com/EB4V-7FPA-OQ5G-IEMZ-XXKE-IYYE-KVCH-D3
ERROR - The feature has not been implemented
````

Specifying the /json option returns a result of type Result:

````
>device earl udf://example.com/EB4V-7FPA-OQ5G-IEMZ-XXKE-IYYE-KVCH-D3 /json
{
  "Result": {
    "Success": false,
    "Reason": "The feature has not been implemented"}}
````

# device list

````
list   List devices in the device catalog
       Recryption group name in user@example.com format
    /mesh   Account identifier (e.g. alice@example.com) or profile fingerprint
    /verbose   Verbose reports (default)
    /report   Report output (default)
    /json   Report output in JSON format
````

The `device list` command lists the device profiles in the device catalog.


````
>device list
ERROR - Object reference not set to an instance of an object.
````

Specifying the /json option returns a result of type Result:

````
>device list /json
{
  "Result": {
    "Success": false,
    "Reason": "Object reference not set to an instance of an object."}}
````

# device pending

````
pending   Get list of pending connection requests
    /mesh   Account identifier (e.g. alice@example.com) or profile fingerprint
    /verbose   Verbose reports (default)
    /report   Report output (default)
    /json   Report output in JSON format
````

The `device pending` command lists the pending device connection requests in
the inbound message spool.


````
>device pending
````

Specifying the /json option returns a result of type ResultPending:

````
>device pending /json
{
  "ResultPending": {
    "Success": true,
    "Messages": [{
        "MessageID": "NDTT-QY2Q-62HE-2ZWJ-L5C2-UDNE-64VS-U6OJ-M3YW-F4JK-3I",
        "Account": "alice@example.com",
        "DeviceProfile": [{
            "dig": "S512",
            "cty": "application/mmm"},
          "ewogICJQcm9maWxlRGV2aWNlIjogewogICAgIlNpZ25hdHVyZUtleSI
  6IHsKICAgICAgIlVERiI6ICJNREFYLTZURjUtUzRFNC1JQUJMLVZFWjItTlNDN
  S1WQUNaIiwKICAgICAgIlB1YmxpY1BhcmFtZXRlcnMiOiB7CiAgICAgICAgIlB
  1YmxpY0tleUVDREgiOiB7CiAgICAgICAgICAiY3J2IjogIkVkNDQ4IiwKICAgI
  CAgICAgICJQdWJsaWMiOiAiR0dMd1BWOE9FTWYxWXdwcTJvbGdDeU93eVk3UDh
  ZSFk5MGtoM1ZPUkFQZ0Q0cTlUQ01FSwogIEhsVjN5Zk12akZwUUszQmo0V0YtM
  l9ZQSJ9fX0sCiAgICAiRGV2aWNlQXV0aGVudGljYXRpb25LZXkiOiB7CiAgICA
  gICJVREYiOiAiTUQ0Ti1URDZELU80RkwtQVZTWC00UkEzLU5CTFYtT1JZRSIsC
  iAgICAgICJQdWJsaWNQYXJhbWV0ZXJzIjogewogICAgICAgICJQdWJsaWNLZXl
  FQ0RIIjogewogICAgICAgICAgImNydiI6ICJFZDQ0OCIsCiAgICAgICAgICAiU
  HVibGljIjogImJkMnJnR0wzUHVtczNiNUxWemZ1d1ZJZGtuZm5MWmZ1cDhBRWJ
  4TDBmMEVka1AxY1NJNWYKICBjNU13czE2d0lYUWZVNnBfTXZFQmFGS0EifX19L
  AogICAgIkRldmljZUVuY3J5cHRpb25LZXkiOiB7CiAgICAgICJVREYiOiAiTUN
  ENy1LNUpELVFSWlMtNktOWi1JU1kzLTVENVgtUzJUTyIsCiAgICAgICJQdWJsa
  WNQYXJhbWV0ZXJzIjogewogICAgICAgICJQdWJsaWNLZXlFQ0RIIjogewogICA
  gICAgICAgImNydiI6ICJFZDQ0OCIsCiAgICAgICAgICAiUHVibGljIjogIkJXR
  DJZVUZ6YzZTeGNBZVNnMGVBbDk1Sk1JVW1GWk03NGUzb0Y2LWd3ZkkwTDRyZGc
  xWTIKICBCOThUVHBQNHF6enp4bElYaXVRemxtNEEifX19fX0",
          {
            "signatures": [{
                "signature": "9eeTL-Q7npEA51YPbcaiZz5k9ZxhEfF8M7IHjlTdesqHJDRY7
  xCGSVt6h8jgG0CGtABslCW9p2UAcFJKDVtH20k1XdmBNKPKCc3vGhurvCyIdTd
  Ft5nNjOiJvqsPPx9SUgg4C0UV0vdkB_p_oIJgHiYA"}],
            "PayloadDigest": "V4MkF08a9l60bJb03ohYtS2DBANtYOhmSSVT-MUQSCetP
  B4uZNe2une9mzLXCAOeypG14SWDk6gZkb50BenRUQ"}],
        "ClientNonce": "3FWj3SlEMndfSzG7N03CTw",
        "ServerNonce": "Uk1MMaHYOqij8BxAKTG0VA",
        "Witness": "VH74-S44N-BJZH-ABPG-QXAL-PQON-JJFL"},
      {
        "MessageID": "NDK2-WE2P-SZNK-ZPEI-HZCO-S4D4-IED7-UCI7-FW2E-BGQZ-N4",
        "Recipient": "alice@example.com",
        "Contact": [{
            "dig": "S512",
            "cty": "application/mmm"},
          "ewogICJDb250YWN0IjogewogICAgIklkZW50aWZpZXIiOiAiTUFWVC1
  TQUk2LTVOS1ktR0NCWS00SUhMLVBIR0ctTVhZUiIsCiAgICAiQWNjb3VudCI6I
  CJib2JAZXhhbXBsZS5jb20ifX0",
          {
            "signatures": [{
                "signature": "jNGcydDBxL0h8vSDIhlwzD8X_E5Z1isUL22imlIHdVyE6GTbr
  i3Qfcsp8EqgY9jDe6ig1ly-bwYAs9a7BOROW_HrekQYbWBIjK-C-sbhae7TapQ
  VVxjOkFugViGpc7B7Q1PsofDb2l7-LiVkwFRr-TgA"}],
            "PayloadDigest": "pKYz04TShS0xhrCz7TpyLqP3QraZzUoq9o_aQEdxMGujF
  vT8Boc7UNBvwF9m1aHY_uhSuOJ65oTZu-awjFnW8A"}]}]}}
````

# device reject

````
reject   Reject a pending connection
       Fingerprint of connection to reject
    /mesh   Account identifier (e.g. alice@example.com) or profile fingerprint
    /verbose   Verbose reports (default)
    /report   Report output (default)
    /json   Report output in JSON format
````

The `device reject` command rejects the specified connection request.

The command must specify the connection identifier of the request 
being accepted. The connection identifier may be abbreviated provided that
this uniquely identifies the connection being accepted and that at least 
four characters are given.


````
>device reject tbs
ERROR - The feature has not been implemented
````

Specifying the /json option returns a result of type Result:

````
>device reject tbs /json
{
  "Result": {
    "Success": false,
    "Reason": "The feature has not been implemented"}}
````

# device pin

````
pin   Accept a pending connection
    /length   Length of PIN to generate (default is 8 characters)
    /expire   <Unspecified>
    /mesh   Account identifier (e.g. alice@example.com) or profile fingerprint
    /verbose   Verbose reports (default)
    /report   Report output (default)
    /json   Report output in JSON format
````

The `device pin` command generates and registers a new PIN code that may be used
to authenticate a device connection request.

The `/length` option specifies the length of the generated PIN in (significant)
characters.

The '/expire' option specifies an expiry time for the request as an integer 
followed by the letter m, h or d for minutes, hours and days respectively.


````
>device pin
````

Specifying the /json option returns a result of type ResultPIN:

````
>device pin /json
{
  "ResultPIN": {
    "Success": true,
    "MessageConnectionPIN": {
      "MessageID": "NCXB-4ELG-LIOO-NG3S-YCNY-O2B4-AKPX-PFLD-IUMW-CJCV-JU",
      "Account": "alice@example.com",
      "Expires": "2019-04-15T22:08:04Z",
      "PIN": "NDJ2-IF3R-GAZY-GN3O-2W2C-HYR7-YAT"}}}
````

# device pre

````
pre   Create a preconnection request
       New portal account
    /key   Encryption key for use in generating an EARL connector.
    /export   Export the device configuration information to the specified file
    /verbose   Verbose reports (default)
    /report   Report output (default)
    /json   Report output in JSON format
    /cty   Content Type
    /encrypt   Encrypt data for specified recipient
    /sign   Sign data with specified key
    /hash   Compute hash of content
    /new   Force creation of new device profile
    /dudf   Device profile fingerprint
    /did   Device identifier
    /dd   Device description
    /auth   Authorize the specified function
    /admin   Authorize device as administration device
    /all   Authorize device for all application catalogs
    /bookmark   Authorize response to confirmation requests
    /calendar   Authorize access to calendar catalog
    /contact   Authorize access to contacts catalog
    /confirm   Authorize response to confirmation requests
    /mail   Authorize access to configure SMTP mail services.
    /network   Authorize access to the network catalog
    /password   Authorize access to the password catalog
    /ssh   Authorize use of SSH
````

The `device pre \<account\>` command requests connection of a device to a mesh hailing
account supporting the EARL connection profile.

The \<account\> parameter specifies the hailing account for which the connection request is
made.

The `/key` option may be used to generate the encryption key to be used.

If the `/export` option is specified, the device profile and private keys are written to
a DARE container archive under the specified encryption options rather than the device 
on which the command is issued. This allows a host machine to be used to perform 
offline initialization of device profiles in batch mode during manufacture.


````
>device pre devices@example.com /key=udf://example.com/EB4V-7FPA-OQ5G-IEMZ-XXKE-IYYE-KVCH-D3
ERROR - Object reference not set to an instance of an object.
````

Specifying the /json option returns a result of type Result:

````
>device pre devices@example.com /key=udf://example.com/EB4V-7FPA-OQ5G-IEMZ-XXKE-IYYE-KVCH-D3 /json
{
  "Result": {
    "Success": false,
    "Reason": "Object reference not set to an instance of an object."}}
````

# device request

````
request   Connect to an existing profile registered at a portal
       New portal account
    /pin   One time use authenticator
    /verbose   Verbose reports (default)
    /report   Report output (default)
    /json   Report output in JSON format
    /new   Force creation of new device profile
    /dudf   Device profile fingerprint
    /did   Device identifier
    /dd   Device description
    /auth   Authorize the specified function
    /admin   Authorize device as administration device
    /all   Authorize device for all application catalogs
    /bookmark   Authorize response to confirmation requests
    /calendar   Authorize access to calendar catalog
    /contact   Authorize access to contacts catalog
    /confirm   Authorize response to confirmation requests
    /mail   Authorize access to configure SMTP mail services.
    /network   Authorize access to the network catalog
    /password   Authorize access to the password catalog
    /ssh   Authorize use of SSH
````

The `device request \<account\>` command requests connection of a device to a mesh profile.

The \<account\> parameter specifies the account for which the connection request is
made.

If the account holder has generated an authentication code, this is specified by means of 
the `/pin` option.




````
>device request alice@example.com
````

Specifying the /json option returns a result of type ResultConnect:

````
>device request alice@example.com /json
{
  "ResultConnect": {
    "Success": true}}
````

