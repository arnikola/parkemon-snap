// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRandom.cs" company="Dapr Labs">
//   Copyright © Dapr Labs. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ServiceCommon.Utilities.Random
{
    /// <summary>
    /// Wrapper for RNG generator.
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Returns a byte array filled with random data.
        /// </summary>
        /// <param name="length">
        /// The length of the byte array to return.
        /// </param>
        byte[] GetBytes(int length);
    }
}
