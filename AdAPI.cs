using System;
using System.Collections.Generic;
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

namespace UsersSupport
{
    class Computer {
        public String name {get;set;}
        public IPAddress ip { get; set; }
        public DateTime lastLogon { get; set; }
        public String lastLoggedUser { get; set; }

        public Computer(String name, String lastLoggedUser, DateTime? lastLogon)
        {
            this.name = name;
            this.lastLoggedUser = lastLoggedUser;
            this.lastLogon = lastLogon ?? DateTime.FromOADate(0);

            this.ip = this.GetIP();
        }

        public void OpenTightVNC()
        {
            String tvncViewerFile = @"C:\Program Files\TightVNC\tvnviewer.exe";
            if (this.IsOnline() && this.IsPortOpen(5800))
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
                    Process.Start("http://" + this.ip + ":5800");
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
                    Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", this.name), null);

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

                /*ObjectQuery Query = new ObjectQuery("SELECT LogonId  FROM Win32_LogonSession Where LogonType=2");
                ManagementObjectSearcher Searcher = new ManagementObjectSearcher(Scope, Query);

                foreach (ManagementObject WmiObject in Searcher.Get())
                {
                    Console.WriteLine("{0,-35} {1,-40}", "LogonId", WmiObject["LogonId"]);// String

                    ObjectQuery LQuery = new ObjectQuery("Associators of {Win32_LogonSession.LogonId=" + WmiObject["LogonId"] + "} Where AssocClass=Win32_LoggedOnUser Role=Dependent");
                    ManagementObjectSearcher LSearcher = new ManagementObjectSearcher(Scope, LQuery);
                    foreach (ManagementObject LWmiObject in LSearcher.Get())
                    {
                        System.Diagnostics.Debug.Write(LWmiObject);
                        Console.WriteLine("{0,-35} {1,-40}", "Name", LWmiObject["Name"]);
                    }
                }*/
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
        public ObservableCollection<Computer> GetComputers()
        {
            ObservableCollection<Computer> list = new ObservableCollection<Computer>();

            try
            {

                PrincipalContext pc = new PrincipalContext(ContextType.Domain, "BATIPLUS");
                PrincipalSearcher ps = new PrincipalSearcher(new ComputerPrincipal(pc));
                PrincipalSearchResult<Principal> psr = ps.FindAll();
                foreach (ComputerPrincipal cp in psr)
                {
                    if (cp.Enabled == true)
                    {
                        Computer computer = new Computer(cp.Name, cp.Description, cp.LastLogon);

                        list.Add(computer);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " - " + e.StackTrace);
            }

            return list;
        }

        
    }
}
