'----------------------------------------------------------------
' Ce script, à executer lors de l'ouverture de session, 
' permet de modifier la description de l'ordinateur en cours
' avec le nom de l'utilisateur qui ouvre la session ainsi que
' la date et l'heure.
'----------------------------------------------------------------

Option Explicit
Const ADS_PROPERTY_UPDATE = 2 
Dim objSysInfo, objComp, objUser, strDescription

Set objSysInfo = CreateObject("ADSystemInfo")
Set objComp = GetObject("LDAP://" & objSysInfo.ComputerName)
Set objUser = GetObject("LDAP://" & objSysInfo.UserName)

'MsgBox objComp.dNSHostName
'MsgBox objUser.cn

strDescription = objUser.cn & " at " & Date & " " & Time

objComp.Put "description", strDescription
objComp.SetInfo