using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Diagnostics;
using System.Windows;
using System.IO;
using System.Runtime.InteropServices;
using ActiveDs; 

namespace UsersSupport
{
    class Computer {
        public String name {get;set;}
        public IPAddress ip { get; set; }
        public DateTime lastLogon { get; set; }
        public String lastLoggedUser { get; set; }

        public Computer(String name, String lastLoggedUser, DateTime lastLogon)
        {
            this.name = name;
            this.lastLoggedUser = lastLoggedUser;
            this.lastLogon = lastLogon;

            this.ip = this.GetIP();
        }

        public void OpenTightVNC()
        {
            String tvncViewerFile = (string)Settings.Get("TvncViewerPath");
            if (this.IsOnline() && this.IsPortOpen((int)Settings.Get("TvncPort")))
            {
                if (File.Exists(tvncViewerFile))
                {
                    // Prefered method is to use the TightVNC Viewer
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = tvncViewerFile;
                    startInfo.Arguments = ""+ this.ip +"";
                    process.StartInfo = startInfo;
                    process.Start();
                }
                else
                {
                    // Tight VNC Viewer does not exists, launch web viewer
                    Process.Start("http://" + this.ip + ":" + (int)Settings.Get("TvncPort"));
                }
            }
            else
            {
                MessageBox.Show("Soit le PC est éteint, soit TightVNC n'est pas installé.");
            }
        }

        public Boolean IsPortOpen(int port)
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(this.ip, port);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public bool IsOnline()
        {
            if (String.IsNullOrEmpty(this.ip.ToString()) == false)
            {
                try
                {
                    Ping ping = new Ping();
                    PingReply pingReply = ping.Send(this.ip);

                    return pingReply.Status == IPStatus.Success;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public IPAddress GetIP()
        {
            try
            {
                IPAddress[] addresslist = Dns.GetHostAddresses(this.name);

                return addresslist[0];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " - " + e.StackTrace);
                return null;
            }
        }

        public String GetConnectedUser()
        {
            string owner = "";

            if (this.IsOnline() == false)
            {
                return null;
            }

            try
            {
                ManagementScope Scope;

                if (!this.name.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                {
                    ConnectionOptions Conn = new ConnectionOptions();
                    /*Conn.Username = "Administrateur";
                    Conn.Password = "ADMIN_PASSWORD";
                    Conn.Authority = "ntlmdomain:DOMAIN";*/
                    Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", this.name), Conn);
                }
                else
                {
                    Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", this.name), null);
                }

                Scope.Connect();

                ObjectQuery Query = new ObjectQuery("Select * from Win32_Process Where Name = \"explorer.exe\"");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(Scope, Query);
                ManagementObjectCollection processList = searcher.Get();

                foreach (ManagementObject obj in processList)
                {
                    string[] argList = new string[] { string.Empty, string.Empty };
                    int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                    if (returnVal == 0)
                    {
                        owner = argList[0];
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(this.name +" : cannot connect to wmi.");
            }

            return owner;
        }
    }

    class DataAPI
    {
        public List<Computer> GetComputers()
        {
            List<Computer> list = new List<Computer>();

            try
            {
                StringCollection ldapPaths = (StringCollection)Settings.Get("LdapComputerPaths");
                foreach (string lapPath in ldapPaths)
                {
                    addComputersFromLDAPpath(list, lapPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " - " + e.StackTrace);
            }

            return list;
        }

        public void addComputersFromLDAPpath(List<Computer> list, string ldapPath)
        {
            DirectoryEntry entry = new DirectoryEntry("LDAP://" + (string)Settings.Get("LdapDomain"));
            entry.Path = ldapPath;
            DirectorySearcher mySearcher = new DirectorySearcher(entry);
            mySearcher.Filter = ("(objectClass=computer)");
            mySearcher.SizeLimit = int.MaxValue;
            mySearcher.PageSize = int.MaxValue;

            foreach (SearchResult resEnt in mySearcher.FindAll())
            {
                DirectoryEntry cp = resEnt.GetDirectoryEntry();
                if (cp.NativeGuid != null)
                {
                    int flags = (int)cp.Properties["userAccountControl"].Value;
                    bool enabled = !Convert.ToBoolean(flags & 0x0002);

                    Console.WriteLine((string)cp.Properties["Name"][0]);

                    if (enabled)
                    {
                        Int64 lastLogonThisServer = new Int64();
                        if (cp.Properties["lastLogon"].Value != null)
                        {
                            IADsLargeInteger lgInt = (IADsLargeInteger)cp.Properties["lastLogon"].Value;
                            lastLogonThisServer = ((long)lgInt.HighPart << 32) + lgInt.LowPart;
                        }

                        DateTime lastLogon = DateTime.FromFileTime(lastLogonThisServer);
                        String name = (cp.Properties.Contains("Name") && cp.Properties["Name"].Count > 0) ? (string)cp.Properties["Name"][0] : "";
                        String desc = (cp.Properties.Contains("Description") && cp.Properties["Description"].Count > 0) ? (string)cp.Properties["Description"][0] : "";

                        Computer computer = new Computer(name, desc, lastLogon);

                        list.Add(computer);
                    }
                }
            }

            mySearcher.Dispose();
            entry.Dispose();
        }
    }
}
