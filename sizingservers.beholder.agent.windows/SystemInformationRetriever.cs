/*
 * 2017 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using System;
using System.Collections.Generic;
using System.Management;
using sizingservers.beholder.agent.shared;

namespace sizingservers.beholder.agent.windows {
    public class SystemInformationRetriever : ISystemInformationRetriever {
        private static SystemInformationRetriever _instance = new SystemInformationRetriever();

        private SystemInformationRetriever() { }

        public static SystemInformationRetriever GetInstance() { return _instance; }
        public SystemInformation Retrieve() {
            var sysinfo = new SystemInformation();

            sysinfo.bmcIp = GetBMCIP();

            ManagementScope scope = ConnectScope();
            ManagementObjectCollection col;
            try {
                col = new ManagementObjectSearcher(scope, new ObjectQuery("Select CSName from Win32_OperatingSystem")).Get();
                foreach (ManagementObject mo in col) {
                    sysinfo.hostname = mo["CSName"].ToString().Trim();
                    break;
                }
            }
            catch {
                //Empty catches for when for some reason the requested info is not available on the system.
            }

            try {
                col = new ManagementObjectSearcher(scope, new ObjectQuery("Select IPAddress from Win32_NetworkAdapterConfiguration where IPEnabled='True'")).Get();
                var ips = new List<string>();
                foreach (ManagementObject mo in col)
                    foreach (string ip in mo["IPAddress"] as string[]) ips.Add(ip);

                sysinfo.ips = string.Join("\t", ips.ToArray());
            }
            catch { }

            try {
                col = new ManagementObjectSearcher(scope, new ObjectQuery("Select Version, Name, BuildNumber from Win32_OperatingSystem")).Get();
                foreach (ManagementObject mo in col) {
                    sysinfo.os = string.Format("{0} {1} Build {2}", mo["Name"].ToString().Split("|".ToCharArray())[0].Trim(), mo["Version"].ToString().Trim(), mo["BuildNumber"].ToString().Trim());
                    break;
                }
            }
            catch { }

            try {
                col = new ManagementObjectSearcher(scope, new ObjectQuery("Select Manufacturer, Model from Win32_ComputerSystem")).Get();
                foreach (ManagementObject mo in col) {
                    sysinfo.system = mo["Manufacturer"].ToString().Trim() + " - " + mo["Model"].ToString().Trim();
                    break;
                }
            }
            catch { }

            try {
                col = new ManagementObjectSearcher(scope, new ObjectQuery("Select Domain from Win32_ComputerSystem")).Get();
                foreach (ManagementObject mo in col) {
                    sysinfo.hostname += "." + mo["Domain"].ToString().Trim();
                    break;
                }
            }
            catch { }

            col = new ManagementObjectSearcher(scope, new ObjectQuery("Select * from Win32_BaseBoard")).Get();
            foreach (ManagementObject mo in col) {
                sysinfo.baseboard = string.Empty;
                try {
                    if (mo["Manufacturer"] != null) sysinfo.baseboard += (mo["Manufacturer"] ?? "Unknown manufacturer").ToString().Trim();
                }
                catch { }
                try {
                    if (mo["Model"] != null) sysinfo.baseboard += " - model: " + mo["Model"].ToString().Trim();
                }
                catch { }
                try {
                    if (mo["Product"] != null)
                        sysinfo.baseboard += " - product: " + mo["Product"].ToString().Trim();
                    if (mo["PartNumber"] != null) sysinfo.baseboard += " - part number: " + mo["PartNumber"].ToString().Trim();
                }
                catch { }
            }

            try {
                col = new ManagementObjectSearcher(scope, new ObjectQuery("Select Name from Win32_BIOS WHERE PrimaryBIOS='True'")).Get();
                foreach (ManagementObject mo in col) {
                    sysinfo.bios = mo["Name"].ToString().Trim();
                    break;
                }
            }
            catch { }

            try {
                col = new ManagementObjectSearcher(scope, new ObjectQuery("Select Name from Win32_Processor")).Get();
                var processorsDict = new SortedDictionary<string, int>();
                foreach (ManagementObject mo in col) {
                    string processor = mo["Name"].ToString().Trim();

                    if (processorsDict.ContainsKey(processor))
                        ++processorsDict[processor];
                    else
                        processorsDict.Add(processor, 1);
                }
                sysinfo.processors = Helper.ComponentDictToString(processorsDict); ;
            }
            catch { }

            try {
                col = new ManagementObjectSearcher(scope, new ObjectQuery("Select * from Win32_PhysicalMemory")).Get();
                var memModulesDict = new SortedDictionary<string, int>();
                foreach (ManagementObject mo in col) {
                    string memModule = "";
                    try {
                        memModule = ulong.Parse(mo["Capacity"].ToString().Trim()) / (1024 * 1024 * 1024) + " GB";
                    }
                    catch { }
                    try {
                        if (mo["Manufacturer"] != null) memModule += " - manufacturer: " + mo["Manufacturer"].ToString().Trim();
                    }
                    catch { }
                    try {
                        if (mo["Model"] != null) memModule += " - model: " + mo["Model"].ToString().Trim();
                    }
                    catch { }
                    try {
                        if (mo["PartNumber"] != null) memModule += " - part number: " + mo["PartNumber"].ToString().Trim();
                    }
                    catch { }
                    try {
                        if (mo["Manufacturer"] == null && mo["Model"] == null)
                            memModule += " - unknown manufacturer and model";
                    }
                    catch { }
                    try {
                        memModule += " (" + (mo["Speed"] ?? "?").ToString().Trim() + " Mhz)";
                    }
                    catch { }

                    if (memModulesDict.ContainsKey(memModule))
                        ++memModulesDict[memModule];
                    else
                        memModulesDict.Add(memModule, 1);
                }
                sysinfo.memoryModules = Helper.ComponentDictToString(memModulesDict);
            }
            catch { }

            try {
                col = new ManagementObjectSearcher(scope, new ObjectQuery("Select Size, Model from Win32_DiskDrive where InterfaceType != 'USB'")).Get();
                var disksDict = new SortedDictionary<string, int>();
                foreach (ManagementObject mo in col) {
                    string disk = string.Format("{0} GB - {1}", ulong.Parse(mo["Size"].ToString().Trim()) / (1024 * 1024 * 1024), mo["Model"].ToString().Trim());

                    if (disksDict.ContainsKey(disk))
                        ++disksDict[disk];
                    else
                        disksDict.Add(disk, 1);
                }
                sysinfo.disks = Helper.ComponentDictToString(disksDict);
            }
            catch { }

            try {
                scope = ConnectScope("root\\StandardCimv2");

                col = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT Name, DriverDescription, MediaConnectState FROM MSFT_NetAdapter WHERE HardwareInterface = 'True' AND EndpointInterface = 'False'")).Get();
                var d = new SortedDictionary<uint, SortedSet<string>>();
                foreach (ManagementObject mo in col) {
                    string s = mo["Name"] + " - " + mo["DriverDescription"].ToString().Trim();
                    uint mediaConnectState = uint.Parse(mo["MediaConnectState"].ToString().Trim());

                    uint sortedState = mediaConnectState;
                    if (mediaConnectState == 0) {
                        s += " (unknown status)";
                        sortedState = 3;
                    }
                    else if (mediaConnectState == 1) {
                        s += " (connected)";
                    }
                    else if (mediaConnectState == 2) {
                        s += " (disconnected)";
                    }

                    if (!d.ContainsKey(sortedState)) d.Add(sortedState, new SortedSet<string>());
                    d[sortedState].Add(s);
                }

                var nicsDict = new SortedDictionary<string, int>();
                for (uint j = 1; j != 4; j++)
                    if (d.ContainsKey(j)) {
                        foreach (string nic in d[j]) {
                            if (nicsDict.ContainsKey(nic))
                                ++nicsDict[nic];
                            else
                                nicsDict.Add(nic, 1);
                        }
                    }

                sysinfo.nics = Helper.ComponentDictToString(nicsDict);
            }
            catch { }

            return sysinfo;
        }

        private ManagementScope ConnectScope(string nameSpace = "root\\cimv2") {
            var options = new ConnectionOptions();
            options.Impersonation = ImpersonationLevel.Impersonate;
            options.EnablePrivileges = false;
            options.Username = null;
            options.Password = null;
            var mpath = new ManagementPath(String.Format("\\\\{0}\\{1}", Environment.MachineName, nameSpace));
            var scope = new ManagementScope(mpath, options);

            scope.Connect();

            return scope;
        }

        private static string GetBMCIP() {
            // https://michlstechblog.info/blog/windows-read-the-ip-address-of-a-bmc-board/
            // https://msdn.microsoft.com/en-us/library/cc146163.aspx

            /*
                void RequestResponse(
                  [in]  uint8  Command,
                  [out] uint8  CompletionCode,
                  [in]  uint8  Lun,
                  [in]  uint8  NetworkFunction,
                  [in]  uint8  RequestData[],
                  [out] uint32 ResponseDataSize,
                  [in]  uint32 RequestDataSize,
                  [in]  uint8  ResponderAddress,
                  [out] uint8  ResponseData
                );
            */

            byte bmcResponderAddress = 0x20;
            byte getLANInfoCmd = 0x02;
            byte getChannelInfoCmd = 0x42;
            byte defaultLUN = 0x00;
            //byte ipmiProtocolType = 0x01;
            byte __8023LANMediumType = 0x04;
            byte maxChannel = 0x0b;

            byte[] requestData = null;

            var cls = new ManagementClass("root\\wmi", "microsoft_ipmi", new ObjectGetOptions());

            try {
                if (cls.GetInstances().Count == 0)
                    return "BMC not found";
            }
            catch {
                return "BMC not found";
            }

            ManagementObject clsInstance = null;
            foreach (ManagementObject candidate in cls.GetInstances()) {
                clsInstance = candidate;
                break;
            }

            ManagementBaseObject inParameters = cls.GetMethodParameters("RequestResponse");
            inParameters.SetPropertyValue("Command", getChannelInfoCmd);
            inParameters.SetPropertyValue("Lun", defaultLUN);
            inParameters.SetPropertyValue("NetworkFunction", 0x06);
            inParameters.SetPropertyValue("ResponderAddress", bmcResponderAddress);

            //Get the first lan channel.
            byte lanChannel = 0;
            bool lanFound = false;
            for (; lanChannel <= maxChannel; lanChannel++) {
                requestData = new byte[] { lanChannel };

                inParameters.SetPropertyValue("RequestData", requestData);
                inParameters.SetPropertyValue("RequestDataSize", requestData.Length);

                ManagementBaseObject mo = clsInstance.InvokeMethod("RequestResponse", inParameters, new InvokeMethodOptions());
                if (((byte[])mo.GetPropertyValue("ResponseData"))[2] == __8023LANMediumType) {
                    lanFound = true;
                    break;
                }
            }
            if (!lanFound)
                return "BMC not connected to a LAN";

            //Get the IP
            inParameters.SetPropertyValue("Command", getLANInfoCmd);
            inParameters.SetPropertyValue("NetworkFunction", 0x0c);

            requestData = new byte[] { lanChannel, 3, 0, 0 };

            inParameters.SetPropertyValue("RequestData", requestData);
            inParameters.SetPropertyValue("RequestDataSize", requestData.Length);

            ManagementBaseObject o = clsInstance.InvokeMethod("RequestResponse", inParameters, new InvokeMethodOptions());
            var responseData = (byte[])o.GetPropertyValue("ResponseData");

            return responseData[2] + "." + responseData[3] + "." + responseData[4] + "." + responseData[5];
        }


    }
}

