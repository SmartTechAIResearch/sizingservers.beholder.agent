/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace sizingservers.beholder.agent.shared {
    /// <summary>
    /// Listens for requestreport\r\n, echo's the message back and instructs the reporter to send sys info over http to the api.
    /// </summary>
    public static class RequestReportHandler {
        private static TcpListener _listener, _ipv6Listener;
        private static bool _started;


        public static void Start(int port) {
            try {
                Stop();

                _listener = new TcpListener(IPAddress.Any, port);
                _ipv6Listener = new TcpListener(IPAddress.IPv6Any, port);


                _listener.Start();
                _ipv6Listener.Start();

                ThreadPool.QueueUserWorkItem((state) => { AcceptClient(_listener); });

                ThreadPool.QueueUserWorkItem((state) => { AcceptClient(_ipv6Listener); });

                _started = true;
            }
            catch (Exception ex) {
                Stop();

                ConsoleColor c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(DateTime.Now.ToString("yyyy\"-\"MM\"-\"dd\" \"HH\":\"mm\":\"ss") + " - Starting the request report handler failed:\n" + ex);
                Console.WriteLine();
                Console.ForegroundColor = c;
            }
        }

        private static void AcceptClient(TcpListener listener) {
            while (_started)
                try {
                    HandleStream(listener.AcceptTcpClient());
                }
                catch {
                    //Do not log errors. We do not care for disconnects.
                }
        }

        private static void HandleStream(TcpClient client) {
            ThreadPool.QueueUserWorkItem(async (c) => {
                try {
                    var stream = (c as TcpClient).GetStream();
                    var sr = new StreamReader(stream);
                    var sw = new StreamWriter(stream);

                    while (_started) {
                        string line = sr.ReadLine();
                        if (line.Trim().ToLowerInvariant() == "requestreport") {
                            sw.Write("requestreport\r\n");
                            sw.Flush();
                            await SystemInformationReporter.Report();
                        }
                    }
                }
                catch {
                    //Do not log errors. We do not care for disconnects. Report errors are logged in SystemInformationReporter.Report().
                }
            }, client);
        }

        public static void Stop() {
            _started = false;
            if (_listener != null) {
                try { _listener.Stop(); } catch { };
                _listener = null;
            }
            if (_ipv6Listener != null) {
                try { _ipv6Listener.Stop(); } catch { };
                _ipv6Listener = null;
            }
        }

    }
}