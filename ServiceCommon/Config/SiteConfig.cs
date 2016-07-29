using ServiceCommon.Utilities.Config;

namespace ServiceCommon.Config
{
    using System.Collections.Generic;

    /// <summary>
    /// Site config.
    /// </summary>
    public class SiteConfig : IConfiguration
    {
        public SiteConfig(HostingEnvironment environment)
        {
            DefaultConfiguration.SetEnvironmentValues(this, environment);
        }

        /// <summary>
        /// Gets or sets the error detail. If true, server exception details will display for all users 
        /// </summary>
        [Default(true)]
        public bool AlwaysIncludeErrorDetail { get; set; }

        [Default("http://localhost:80")]
        [Default("http://yamsmelb.cloudapp.net:80", Environment = Environments.Melbourne)]
        public List<string> ListenAddresses { get; set; }

        [Default("Otterpop")]
        public string SiteName { get; set; }

        [Default("80,443")]
        public List<string> FirewallAllowPorts { get; set; }

        [Default("")]
        public string CertificateThumbprint { get; set; }

        [Default(null)]
        [Default(@"C:\dev\Otterpop\Web\wwwroot", Environment = Environments.Dev)]
        public string OverrideWwwRoot { get; set; }
    }
}
