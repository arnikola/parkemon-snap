// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CryptographicallySecureRandom.cs" company="Dapr Labs">
//   Copyright © Dapr Labs. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ServiceCommon.Utilities.Random
{
    using System.Security.Cryptography;

    /// <summary>
    /// The cryptographically secure random number generator.
    /// </summary>
    public class CryptographicallySecureRandom : IRandom
    {
        /// <summary>
        /// Returns a byte array filled with random data.
        /// </summary>
        /// <param name="length">
        /// The length of the byte array to return.
        /// </param>
        public byte[] GetBytes(int length)
        {
            var bytes = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }

            return bytes;
        }
    }
}
