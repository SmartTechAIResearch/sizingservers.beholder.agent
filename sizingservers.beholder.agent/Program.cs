/*
 * 2017 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace sizingservers.beholder.agent {
    class Program {
        static void Main(string[] args) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Console.WriteLine("Launching Windows Beholder Agent");
                Console.WriteLine();
                var p = Process.Start(Path.Combine(AppContext.BaseDirectory, "Windows", "sizingservers.beholder.agent.windows.exe"));
                p.WaitForExit();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Console.WriteLine("Launching Linux Beholder Agent");
                Console.WriteLine();
                var p = Process.Start("dotnet", Path.Combine(AppContext.BaseDirectory, "Linux", "sizingservers.beholder.agent.linux.dll"));
                p.WaitForExit();
            }
            else {
                Console.WriteLine("No Beholder Agent found for this operating system: " + RuntimeInformation.OSDescription);
                Console.WriteLine();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}