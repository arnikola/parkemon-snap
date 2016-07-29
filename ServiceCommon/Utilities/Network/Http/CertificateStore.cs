namespace ServiceCommon.Utilities.Network.Http
{
    using System;
    using System.Diagnostics;

    using ServiceCommon.Utilities.Extensions;

    public static class HttpCertificateConfiguration
    {
        public static void SetSslCertificate(string thumbprint, string address)
        {
            new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = "netsh",
                Arguments = $@"http add sslcert ipport={address} certhash={thumbprint} appid={{{Guid.NewGuid()}}}" }.Execute();
        }

        public static bool IsSslCertificateSet(string thumbprint, string address)
        {
            var result = new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = "netsh",
                Arguments = $"http show sslcert ipport={address}" }.Execute(throwOnError: false);
            return result.ExitCode == 0 && result.Output.ToLowerInvariant().Contains(thumbprint.ToLowerInvariant());
        }

        public static void RemoveSslCertificate(string address, bool throwOnError = false)
        {
            new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"http delete sslcert ipport={address}" }.Execute(throwOnError);
        }
    }
}