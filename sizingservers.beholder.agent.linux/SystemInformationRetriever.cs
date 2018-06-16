/*
 * 2017 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using sizingservers.beholder.agent.shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace sizingservers.beholder.agent.linux {
    public class SystemInformationRetriever : ISystemInformationRetriever {
        private static SystemInformationRetriever _instance = new SystemInformationRetriever();
        private static string _inxiPath, _tempPath;

        private SystemInformationRetriever() {
            string thisDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            _inxiPath = Path.Combine(thisDirectory, "inxi");
            _tempPath = Path.Combine(thisDirectory, "temp");

            GetBashStdOutput("chmod +x '" + _inxiPath + "'");
        }

        public static SystemInformationRetriever GetInstance() { return _instance; }
        public SystemInformation Retrieve() {
            var sysinfo = new SystemInformation();

            string ipmiToolOutput = GetBashStdOutput("ipmitool lan print | grep -i 'ip address' | grep -vi 'source'");
            sysinfo.bmcIp = ipmiToolOutput.Substring(ipmiToolOutput.IndexOf(':') + 1).Trim();

            if (sysinfo.bmcIp.Length == 0) sysinfo.bmcIp = "ipmitool not installed or no BMC available";
            

            string inxiOutput = GetBashStdOutput("'" + _inxiPath + "' -SCDMNm -xi -c 0");
            
            sysinfo.hostname = GetStringBetween(inxiOutput, "Host: ", " Kernel:", "\n", out inxiOutput).Trim();

            var ips = new List<string>();
            while (inxiOutput.Contains("ip-v4: "))
                ips.Add(GetStringBetween(inxiOutput, "ip-v4: ", " ", "\n", out inxiOutput).Trim());

            while (inxiOutput.Contains("ip-v6: "))
                ips.Add(GetStringBetween(inxiOutput, "ip-v6: ", " ", "\n", out inxiOutput).Trim());

            sysinfo.ips = string.Join("\t", ips);

            sysinfo.os = GetStringBetween(inxiOutput, "Distro: ", "\n", "\n", out inxiOutput).Replace(": ", " - ") + " - kernel " + GetStringBetween(inxiOutput, "Kernel: ", " Desktop", "\n", out inxiOutput).Trim().Replace(": ", " - ");

            //Trim output
            inxiOutput = inxiOutput.Split(new string[] { "Machine:" }, StringSplitOptions.None)[1];

            sysinfo.system = GetStringBetween(inxiOutput, "System: ", "\n", "\n", out inxiOutput).Trim().Trim().Replace(": ", " - ");

            string[] moboAndBios = GetStringBetween(inxiOutput, "Mobo: ", "\n", "\n", out inxiOutput).Split(new string[] { "BIOS: " }, StringSplitOptions.None);

            sysinfo.baseboard = moboAndBios[0].Trim().Replace(": ", " - ");
            sysinfo.bios = moboAndBios[1].Trim().Replace(": ", " - ");

            string cpu = "CPU: ";
            int cpuIndex = inxiOutput.IndexOf(cpu);
            if (cpuIndex == -1) {
                cpu = "CPU(s): ";
                cpuIndex = inxiOutput.IndexOf(cpu);
            }

            inxiOutput = inxiOutput.Substring(cpuIndex);

            string cpuSection = GetStringBetween(inxiOutput, cpu, "Memory: ", "Memory: ", out string outputStub).Trim();
            var processorsDict = new SortedDictionary<string, int>();
            foreach (string line in cpuSection.Split('\n')) {
                if (line.Contains(" cache: ")) {
                    string processor = line.Substring(0, line.IndexOf(" cache: ")).Trim().Replace(": ", " - ");

                    if (processorsDict.ContainsKey(processor))
                        ++processorsDict[processor];
                    else
                        processorsDict.Add(processor, 1);
                }
            }
            sysinfo.processors = Helper.ComponentDictToString(processorsDict); ;

            string memSection = GetStringBetween(inxiOutput, "Memory: ", "Network: ", "Network: ", out outputStub).Trim();
            var memModulesDict = new SortedDictionary<string, int>();
            foreach (string line in memSection.Split('\n')) {
                if (line.Contains("dmidecode") || (line.Contains("Device") && !line.Contains("No Module"))) {
                    string memModule = line.Trim().Replace(": ", " - ");

                    if (memModulesDict.ContainsKey(memModule))
                        ++memModulesDict[memModule];
                    else
                        memModulesDict.Add(memModule, 1);
                }
            }
            sysinfo.memoryModules = Helper.ComponentDictToString(memModulesDict);

            string networkSection = GetStringBetween(inxiOutput, "Network: ", "Drives: ", "Drives: ", out outputStub).Trim();
            var nicsDict = new SortedDictionary<string, int>();
            foreach (string line in networkSection.Split('\n')) {
                if (line.Contains("Card: ")) {
                    string nic = line.Substring("Card: ".Length).Trim().Replace(": ", " - ");

                    if (nicsDict.ContainsKey(nic))
                        ++nicsDict[nic];
                    else
                        nicsDict.Add(nic, 1);
                }
            }
            sysinfo.nics = Helper.ComponentDictToString(nicsDict);

            inxiOutput = inxiOutput.Substring(inxiOutput.IndexOf("Drives: ") + "Drives: ".Length);
            var disksDict = new SortedDictionary<string, int>();
            foreach (string line in inxiOutput.Split('\n')) {
                if (line.Length != 0 && !line.Contains("Total Size: ")) {
                    string disk = line.Trim().Replace(": ", " - ");

                    if (disksDict.ContainsKey(disk))
                        ++disksDict[disk];
                    else
                        disksDict.Add(disk, 1);
                }
            }
            sysinfo.disks = Helper.ComponentDictToString(disksDict);

            return sysinfo;
        }

        private static string GetBashStdOutput(string command) {
            var startInfo = new ProcessStartInfo("/bin/sh") {
                Arguments = "-c \"" + command + "\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true

            };
            var p = Process.Start(startInfo);
            string s = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="trimmedInput">delimiters and the text between them removed from the input</param>
        /// <returns></returns>
        private static string GetStringBetween(string input, string begin, string end, string alternativeEnd, out string trimmedInput) {
            trimmedInput = input;

            int startIndex = input.IndexOf(begin);
            int length = input.Substring(startIndex + begin.Length).IndexOf(end);
            int alternativeLength = input.Substring(startIndex + begin.Length).IndexOf(alternativeEnd);
            if (length == -1 || length > alternativeLength) {
                length = alternativeLength;
                end = alternativeEnd;
            }

            if (startIndex == -1 || length == -1)
                return string.Empty;


            trimmedInput = trimmedInput.Substring(0, startIndex) + trimmedInput.Substring(startIndex + begin.Length + length + end.Length);

            return input.Substring(startIndex + begin.Length, length);
        }

    }
}

