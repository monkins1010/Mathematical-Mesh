
For example, because Alice granted the device the Web role, she can now access her credential 
catalog and decrypt the file she encrypted on her first device from the new device:


~~~~
<div="terminal">
<cmd>Alice2> meshman password get ftp.example.com
<rsp>alice1@ftp.example.com = [password]

<cmd>Alice2> meshman dare decode ciphertext.dare plaintext2.txt
<cmd>Alice2> meshman type plaintext2.txt
<rsp>This is a test
</div>
~~~~


