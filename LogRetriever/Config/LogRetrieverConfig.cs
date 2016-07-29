// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="Dapr Labs">
//   Copyright © Dapr Labs. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace LogRetriever.Config
{
    using System.Collections.Generic;

    using ServiceCommon.Utilities.Config;

    /// <summary>
    /// Web API config.
    /// </summary>
    public class LogRetrieverConfig : IConfiguration
    {
        public LogRetrieverConfig(HostingEnvironment environment)
        {
            DefaultConfiguration.SetEnvironmentValues(this, environment);
        }

        /// <summary>
        /// Gets or sets the listen addresses.
        /// </summary>
        [Default("http://+:8080/logs")]
        public List<string> ListenAddresses { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        [Default("admin")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Default("nother")]
        public string Password { get; set; }

        [Default(null)]
        public string CertificateThumbprint { get; set; }
    }
}
