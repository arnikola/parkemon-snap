// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DevCert.cs" company="Dapr Labs">
//   Copyright © Dapr Labs. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace ServiceCommon.Certificates
{
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Certificate management methods.
    /// </summary>
    public static class Certificates
    {
        /// <summary>
        /// Returns the embedded certificate with the provided filename and password.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private static X509Certificate2 GetEmbeddedCertificate(string fileName, string password)
        {
            var assembly = typeof(Certificates).Assembly;
            using (var stream = assembly.GetManifestResourceStream(typeof(Certificates).Namespace + "." + fileName))
            {
                return new X509Certificate2(
                    ToArray(stream),
                    password,
                    X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable
                    | X509KeyStorageFlags.MachineKeySet);
            }
        }

        /// <summary>
        /// Returns an array representation of the provided stream.
        /// </summary>
        /// <param name="input">
        /// The stream.
        /// </param>
        /// <returns>
        /// An array representation of the provided stream.
        /// </returns>
        private static byte[] ToArray(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}