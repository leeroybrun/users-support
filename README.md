----------------------------------------------
 User Support
----------------------------------------------

This software let you view computers of your AD domain. For each computers you can :
- Connect to it through TightVNC (web viewer or standard viewer)
- Know which user is connected on the computer
- Get the IP assigned to a computer

It must be run on a computer/session of an AD administrator.

Prerequistes :
- TightVNC server running on the clients computers (protected by password !)
- Logon script to update computer's description with the user logged in
- WMI enabled on clients computers

## Install

1. Install TightVNC server on clients (don't forget to secure it with a password !)
2. Add the logon script to a new GPO (read Logon-Script/README.md)
3. Open the project in Visual Studio
4. Copy the `Properties/Settings.settings` file to `Properties/LocalSettings.settings`
5. Edit the new `Properties/LocalSettings.settings`
6. Build & enjoy !