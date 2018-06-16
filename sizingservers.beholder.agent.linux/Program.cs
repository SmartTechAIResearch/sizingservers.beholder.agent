/*
 * 2017 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using sizingservers.beholder.agent.shared;
using System;
using System.Threading;

namespace sizingservers.beholder.agent.linux {
    class Program {
        private static Mutex _namedMutex = new Mutex(true, "sizingservers.beholder.agent.linux");

        static void Main(string[] args) {
            if (!_namedMutex.WaitOne()) return;

            Console.WriteLine("SIZING SERVERS LAB LINUX BEHOLDER AGENT");
            Console.WriteLine("  Reporting every " + Config.GetInstance().reportEvery + " to " + Config.GetInstance().endpoint);
            Console.WriteLine("  Listening to TCP port " + Config.GetInstance().requestReportTcpPort + " for \"requestreport\\r\\n\"");
            Console.WriteLine();

            RequestReportHandler.Start(Config.GetInstance().requestReportTcpPort);
            SystemInformationReporter.RegisterRetrieverAndStartReporting(SystemInformationRetriever.GetInstance());

            Console.ReadLine();
            RequestReportHandler.Stop();
        }
    }
}