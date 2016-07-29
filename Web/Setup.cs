using System;

namespace Web
{
    using System.Diagnostics;
    using System.Security.Principal;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using ServiceCommon.Config;
    using ServiceCommon.Utilities.Config;
    using ServiceCommon.Utilities.Extensions;
    using ServiceCommon.Utilities.Network;
    using ServiceCommon.Utilities.Network.Http;

    public class Setup
    {
        /// <summary>
        /// Runs the setup procedure.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the work performed.</returns>
        public static async Task Run(HostingEnvironment environment)
        {
            var siteConfig = new SiteConfig(environment);

            // Kill skype on dev boxes, because it squats on port 443.
            if (string.Equals(environment.Name, Environments.Dev))
            {
                foreach (var skype in Process.GetProcessesByName("Skype"))
                {
                    try
                    {
                        Trace.TraceInformation("Killing Skype, the port-443-hogging scourge of the earth.");
                        skype.Kill();
                    }
                    catch (Exception exception)
                    {
                        Trace.TraceWarning($"Exception killing Skype, the port-443-hogging scourge of the earth: {exception}");
                    }
                }
            }

            // Configure the certificates
            await ConfigureCertificates(siteConfig.CertificateThumbprint);

            // Configure HTTP/HTTPS reservations with http.sys.
            ConfigureHttpReservations(siteConfig, WindowsIdentity.GetCurrent());

            // Open firewall ports.
            ConfigureFirewall(siteConfig);

            Trace.TraceInformation("Setup complete.");
        }

        private static async Task ConfigureCertificates(string thumbprint)
        {
            if (string.IsNullOrWhiteSpace(thumbprint))
            {
                return;
            }

            Trace.TraceInformation("Configuring certificates.");

            var localAddress = $"{(await NetworkUtil.GetNodeAddress())}:443";
            
            try
            {
                if (!HttpCertificateConfiguration.IsSslCertificateSet(thumbprint, localAddress))
                {
                    HttpCertificateConfiguration.RemoveSslCertificate(localAddress);
                }
            }
            catch (Exception exception)
            {
                // Warn and ignore.
                Trace.TraceWarning(
                    "Failed to remove certificate from address {0}: {1}",
                    localAddress,
                    exception.ToDetailedString());
            }

            const string AllAddresses = "0.0.0.0:443";
            if (!HttpCertificateConfiguration.IsSslCertificateSet(thumbprint, AllAddresses))
            {
                try
                {
                    HttpCertificateConfiguration.RemoveSslCertificate(AllAddresses);
                }
                catch (Exception exception)
                {
                    // Warn and ignore.
                    Trace.TraceWarning(
                        $"Failed to remove certificate from address {localAddress}: {exception.ToDetailedString()}");
                }
                
                HttpCertificateConfiguration.SetSslCertificate(thumbprint, AllAddresses);
            }

            Trace.TraceInformation("Configured certificates.");
        }

        private static void ConfigureFirewall(SiteConfig siteConfig)
        {
            Trace.TraceInformation("Configuring firewall.");
            var listenAddresses = siteConfig.ListenAddresses;
            foreach (var prefix in listenAddresses)
            {
                ushort port;
                var matches = Regex.Match(prefix, @"^.*\:(\d+).*");
                if (matches.Groups.Count > 1)
                {
                    port = ushort.Parse(matches.Groups[1].Value);
                }
                else if (prefix.StartsWith("https"))
                {
                    port = 443;
                }
                else
                {
                    port = 80;
                }

                FirewallOpenPort(siteConfig.SiteName, port);
            }

            if (siteConfig.FirewallAllowPorts != null)
            {
                foreach (var portString in siteConfig.FirewallAllowPorts)
                {
                    FirewallOpenPort(siteConfig.SiteName, ushort.Parse(portString));
                }
            }

            Trace.TraceInformation("Configured firewall.");
        }

        private static void ConfigureHttpReservations(SiteConfig siteConfig, WindowsIdentity identity)
        {
            Trace.TraceInformation("Configuring HTTP reservations for user {0}.", identity.Name);
            var listenAddresses = siteConfig.ListenAddresses;
            foreach (var prefix in listenAddresses)
            {
                Trace.TraceInformation("Setting HTTP ACL for prefix {0} to allow user {1}.", prefix, identity.Name);
                var sid = identity.User?.ToString();
                HttpApi.ModifyReservation(prefix, sid);

                // Remove strong-wildcard reservations which will prevent this reservation from matching.
                var hostMatch = Regex.Match(prefix, @".*//([^\*^\+]*):.*");
                if (hostMatch.Success)
                {
                    var host = hostMatch.Groups[1].Value;
                    var strongWildcard = prefix.Replace(host, "+");
                    HttpApi.ModifyReservation(
                        strongWildcard,
                        sid,
                        removeReservation: true,
                        throwOnError: false);
                }
            }

            Trace.TraceInformation("Configured HTTP reservations for user {0}.", identity.Name);
        }

        private static void FirewallOpenPort(string applicationName, ushort port)
        {
            var name =
                $"{applicationName}_{port}_{Guid.NewGuid().ToString("N").Substring(0, 4)}";
            Firewall.OpenPort(name, port, Firewall.Protocol.Tcp);
        }
    }
}