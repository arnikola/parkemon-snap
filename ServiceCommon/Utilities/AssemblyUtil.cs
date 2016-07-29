using System;
using System.IO;
using System.Reflection;

namespace ServiceCommon.Utilities
{
    /// <summary>
    /// Assembly utilities
    /// </summary>
    public static class AssemblyUtil
    {
        /// <summary>
        /// Returns the provided assembly's path.
        /// </summary>
        /// <param name="asm">
        /// The assembly.
        /// </param>
        /// <returns>
        /// The provided assembly's path.
        /// </returns>
        public static string GetDirectory(Assembly asm = null)
        {
            return Path.GetDirectoryName(GetFullPath(asm));
        }

        /// <summary>
        /// Returns the provided assembly's location.
        /// </summary>
        /// <param name="asm">
        /// The assembly.
        /// </param>
        /// <returns>
        /// The provided assembly's location.
        /// </returns>
        public static string GetFullPath(Assembly asm = null)
        {
            return new Uri((asm ?? Assembly.GetCallingAssembly()).CodeBase).LocalPath;
        }
    }
}
