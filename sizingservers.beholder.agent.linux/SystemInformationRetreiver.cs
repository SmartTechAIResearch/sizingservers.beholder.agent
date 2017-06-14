/*
 * 2017 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using sizingservers.beholder.agent.shared;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace sizingservers.beholder.agent.linux {
    public class SystemInformationRetreiver : ISystemInformationRetreiver {
        private static SystemInformationRetreiver _instance = new SystemInformationRetreiver();
        private static string _inxiPath, _tempPath;

        private SystemInformationRetreiver() {
            string thisDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            _inxiPath = Path.Combine(thisDirectory, "inxi");
            _tempPath = Path.Combine(thisDirectory, "temp");

            var startInfo = new ProcessStartInfo("chmod +x '" + _inxiPath + "'") {
                UseShellExecute = true
            };
            var p = Process.Start(startInfo);
            p.WaitForExit();
        }

        public static SystemInformationRetreiver GetInstance() { return _instance; }
        public SystemInformation Retreive() {
            var sysinfo = new SystemInformation();
            var startInfo = new ProcessStartInfo("'" + _inxiPath + "' -SCDMNm -xi -c 0 > '" + _tempPath + "'") {
                UseShellExecute = true
            };
            var p = Process.Start(startInfo);
            p.WaitForExit();

            string output = string.Empty;
            using (var sr = new StreamReader(new FileStream(_tempPath, FileMode.Open)))
                output = sr.ReadToEnd();

            sysinfo.hostname = GetStringBetween(output, "Host: ", " ", "\n", out output);

            var ips = new List<string>();
            while (output.Contains("ip-v4: "))
                ips.Add(GetStringBetween(output, "ip-v4: ", " ", "\n", out output));

            while (output.Contains("ip-v6: "))
                ips.Add(GetStringBetween(output, "ip-v6: ", " ", "\n", out output));

            sysinfo.ips = string.Join("\t", ips);

            sysinfo.os = GetStringBetween(output, "Distro: ", "\n", "\n", out output).Replace(": ", " - ") + " - kernel " + GetStringBetween(output, "Kernel: ", " Desktop", "\n", out output).Replace(": ", " - ");

            sysinfo.system = GetStringBetween(output, "System: ", "\n", "\n", out output).Trim().Replace(": ", " - ");

            sysinfo.baseboard = GetStringBetween(output, "Mobo: ", "\n", "\n", out output).Replace(": ", " - ");

            sysinfo.bios = GetStringBetween(output, "BIOS: ", "\n", "\n", out output).Replace(": ", " - ");

            output = output.Substring(output.IndexOf("CPU:"));

            string cpuSection = GetStringBetween(output, "CPU: ", "Memory: ", "Memory: ", out string outputStub).Trim();

            var processors = new List<string>();
            foreach (string line in cpuSection.Split('\n')) {
                if (line.Contains(" cache: "))
                    processors.Add(line.Trim().Substring(0, line.IndexOf(" cache: ")).Replace(": ", " - "));
            }

            sysinfo.processors = string.Join("\t", processors);

            string memSection = GetStringBetween(output, "Memory: ", "Network: ", "Network: ", out outputStub).Trim();
            var memModules = new List<string>();
            foreach (string line in memSection.Split('\n')) {
                if (line.Contains("dmidecode") || (line.Contains("Device") && !line.Contains("No Module")))
                    memModules.Add(line.Trim().Replace(": ", " - "));
            }

            sysinfo.memoryModules = string.Join("\t", memModules);

            string networkSection = GetStringBetween(output, "Network: ", "Drives: ", "Drives: ", out outputStub).Trim();
            var nics = new List<string>();
            foreach (string line in networkSection.Split('\n')) {
                if (line.Contains("Card: "))
                    nics.Add(line.Trim().Substring("Card: ".Length).Replace(": ", " - "));
            }

            sysinfo.nics = string.Join("\t", nics);

            output = output.Substring(output.IndexOf("Drives: ") + "Drives: ".Length);
            var disks = new List<string>();
            foreach (string line in output.Split('\n'))
                if (line.Length != 0 && !line.Contains("Total Size: "))
                    disks.Add(line.Trim().Replace(": ", " - "));

            sysinfo.disks = string.Join("\t", disks);

            return sysinfo;
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

