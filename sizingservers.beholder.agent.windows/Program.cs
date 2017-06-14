/*
 * 2017 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using sizingservers.beholder.agent.shared;
using System;
using System.Threading;

namespace sizingservers.beholder.agent.windows {
    class Program {
        private static Mutex _namedMutex = new Mutex(true, "sizingservers.beholder.agent.windows");

        static void Main(string[] args) {
            if (!_namedMutex.WaitOne()) return;

            Console.WriteLine("SIZING SERVERS LAB WINDOWS BEHOLDER AGENT");
            Console.WriteLine("  Reporting system information every " + Config.GetInstance().reportEvery + " to " + Config.GetInstance().endpoint);
            Console.WriteLine();

            SystemInformationReporter.RegisterRetreiverAndStartReporting(SystemInformationRetreiver.GetInstance());

            Console.ReadLine();
        }
    }
}
