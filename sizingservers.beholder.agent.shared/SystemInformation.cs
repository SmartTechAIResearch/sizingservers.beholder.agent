/*
 * 2017 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

namespace sizingservers.beholder.agent.shared {
    public struct SystemInformation {
        /// <summary>
        /// Computer name + domain. Example: DELL.WORKGROUP
        /// </summary>
        public string hostname { get; set; }
        /// <summary>
        /// Tab seperated, for dbs that cannot handle arrays
        /// </summary>
        public string ips { get; set; }
        /// <summary>
        /// Windows, Ubuntu, Debian, ... + version + build no. Example: Microsoft Windows 10 Pro 10.0.14393 Build 14393
        /// </summary>
        public string os { get; set; }
        /// <summary>
        /// Dell, Supermicro, HP, ... + model. Example: Dell Inc. - Latitude E5570
        /// </summary>
        public string system { get; set; }
        /// <summary>
        /// Dell, AsRock, ... + model + procuct + part no. Example: Dell Inc. - product: 06YF8N
        /// </summary>
        public string baseboard { get; set; }
        /// <summary>
        /// Name. Example: 1.11.4
        /// </summary>
        public string bios { get; set; }
        /// <summary>
        ///Tab seperated, for dbs that cannot handle arrays; Intel, AMD, ... + model + clock. Example: Intel(R) Core(TM) i7-6820HQ CPU @ 2.70GHz 
        /// </summary>
        public string processors { get; set; }
        /// <summary>
        ///Tab seperated, for dbs that cannot handle arrays; Size in GB + manufacturer + model + part number + clock. Example: 8 GB - manufacturer: SK Hynix - part number: HMA41GS6AFR8N-TF (2133 Mhz)
        /// </summary>
        public string memoryModules { get; set; }
        /// <summary>
        ///Tab seperated, for dbs that cannot handle arrays; Size in GB + model. Example: 476 GB - SK hynix SC308 SATA 512GB
        /// </summary>
        public string disks { get; set; }
        /// <summary>
        ///Tab seperated, for dbs that cannot handle arrays; Name + description + status. Example: Wi-Fi - Intel(R) Dual Band Wireless-AC 8260 (connected)
        /// </summary>
        public string nics { get; set; }
        /// <summary>
        /// </summary>
        public string bmcIp { get; set; }
        /// <summary>
        /// Gets or sets the request report TCP port.
        /// </summary>
        public int requestReportTcpPort { get; set; }
    }
}
