/*
 * 2017 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace sizingservers.beholder.agent.shared {
    public class Config {
        private static Config _instance = new Config();

        public string endpoint { get; internal set; }
        public string apiKey { get; internal set; }
        public int reportEveryXMinutes { get; internal set; }
        public string reportEvery { get; internal set; }
        public int pingReplierTcpPort { get; internal set; }

        /// <summary>
        /// Sets default properties, calls Load().
        /// </summary>
        private Config() {
            reportEveryXMinutes = 60 * 24; //Every day.
            reportEvery = "day";
            Load();
        }

        /// <summary>
        /// Return the loaded config (sizingservers.beholder.conf). Throws exception on load failure.
        /// </summary>
        /// <returns></returns>
        public static Config GetInstance() { return _instance; }

        /// <summary>
        /// Loads sizingservers.beholder.agent.conf. Throws exception on failure.
        /// </summary>
        private void Load() {
            Dictionary<string, string> kvps = GetConfKvps();
            foreach (var kvp in kvps)
                switch (kvp.Key) {
                    case "endpoint":
                        endpoint = kvp.Value;
                        break;
                    case "apikey":
                        apiKey = kvp.Value;
                        break;
                    case "reportevery":
                        reportEvery = kvp.Value;
                        reportEveryXMinutes = ParseReportEvery(reportEvery);
                        break;
                    case "pingrepliertcpport":
                        pingReplierTcpPort = int.Parse(kvp.Value);
                        break;
                }
        }

        private int ParseReportEvery(string value) {
            switch (value) {
                case "minute": return 1;
                case "hour": return 60;
                case "day": return 60 * 24;
                case string m when m.EndsWith(" minutes"):
                    return int.Parse(m.Substring(0, " minutes".Length));
                case string h when h.EndsWith(" hours"):
                    return int.Parse(h.Substring(0, " hours".Length)) * 60;
            }

            return 60 * 24;
        }

        private Dictionary<string, string> GetConfKvps() {
            var kvps = new Dictionary<string, string>();
            using (var sr = new StreamReader(
                new FileStream(
                    Path.Combine(AppContext.BaseDirectory, "sizingservers.beholder.agent.conf"), FileMode.Open)))
                while (sr.Peek() != -1) {
                    string line = sr.ReadLine().Trim();
                    if (line.Length != 0 && !line.StartsWith("#")) {
                        line = line.Replace('\t', ' ').ToLowerInvariant();
                        var s = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        kvps.Add(s[0], s[1]);
                    }
                }

            return kvps;
        }
    }
}
