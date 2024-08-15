# SharpSMBTakeover
C# inspired version of the smbtakeover project by @zyn3rgy (https://github.com/zyn3rgy/smbtakeover/) for running locally on a Windows machine.

### What does this do?
This is a C# inspired project that aims to kill the services running SMB on port 445 of a Windows machine to allow easier NTLM style attacks without requiring the need to reboot. Heavily inspired by the work by @zyn3rgy. Specifically this tool can be used to enable, kill or check if the LanmanServer, srv2 and srvnet services are running locally on this machine.

### Usage
Note: Local Administrator privileges are required to modify/stop the services.

- Attempt to disable the 3 above mentioned services:
`SharpSMBTakeover.exe /disable [/reverse]`
This will attempt to disable the three services and set LanmanServer startup type to Disabled to stop it from restarting. The default order of operations to stop the services is: LanmanServer, srv2, srvnet. However as I found out and pointed out by @zyn3rgy the order of stopping the services can be different on each Windows machine. The flag /reverse can be used to stop the services in the following order: srvnet, LanmanServer, srv2.
If successful the Windows machine should no longer have anything bound to port 445.
Note: Sometimes you will have to run this command twice to officially stop the services.

- Re-enable the 3 services
`SharpSMBTakeover.exe /enable`
This will attempt to enable the three services and set the LanmanServer startup type to Automatic. If successfully, TCP port 445 (SMB) should be working.

- Check if the services are running
`SharpSMBTakeover.exe /check`
This will check if the three services are running, indicating that TCP port 445 (SMB) should be working.


