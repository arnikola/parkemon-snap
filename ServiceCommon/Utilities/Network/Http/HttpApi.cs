// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBePrivate.Local
namespace ServiceCommon.Utilities.Network.Http
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    using ServiceCommon.Utilities.Extensions;

    /// <summary>
    /// Utility for managing HttpApi.
    /// </summary>
    public static class HttpApi
    {
        /// <summary>
        /// Modifies the specified HTTP prefix to allow the specified account to listen on it.
        /// </summary>
        /// <param name="urlPrefix">The URL prefix.</param>
        /// <param name="sid">The account SID.</param>
        /// <param name="removeReservation">Whether or not to remove the reservation.</param>
        /// <param name="throwOnError">
        /// Whether or not to throw an error if an exception occurs.
        /// </param>
        /// <exception cref="Exception">An error occurred.</exception>
        public static void ModifyReservation(
            string urlPrefix,
            string sid,
            bool removeReservation = false,
            bool throwOnError = true)
        {
            var config = new NativeMethods.HttpServiceConfigUrlAclSet
            {
                Key = { UrlPrefix = urlPrefix },
                Param = { Sddl = CreateSddl(sid) }
            };
            var httpApiVersion = new NativeMethods.HttpapiVersion(1, 0);
            var result = NativeMethods.HttpInitialize(httpApiVersion, NativeMethods.HttpInitializeConfig, IntPtr.Zero);
            if (result != 0)
            {
                var exception = GetException("HttpInitialize", result);
                Trace.TraceError($"Exception during HttpInitialize: {exception.ToDetailedString()}.");
                if (throwOnError)
                {
                    throw exception;
                }
            }

            try
            {
                // do our best to delete any existing ACL
                result = NativeMethods.HttpDeleteServiceConfigurationAcl(
                    IntPtr.Zero,
                    NativeMethods.HttpServiceConfigUrlAclInfo,
                    ref config,
                    Marshal.SizeOf(typeof(NativeMethods.HttpServiceConfigUrlAclSet)),
                    IntPtr.Zero);
                if (removeReservation)
                {
                    if (result != 0)
                    {
                        var exception = GetException("HttpDeleteServiceConfigurationAcl", result);
                        Trace.TraceWarning(
                            $"Exception during HttpDeleteServiceConfigurationAcl: {exception.ToDetailedString()}.");
                        if (throwOnError)
                        {
                            throw exception;
                        }
                    }

                    return;
                }

                result = NativeMethods.HttpSetServiceConfigurationAcl(
                    IntPtr.Zero,
                    NativeMethods.HttpServiceConfigUrlAclInfo,
                    ref config,
                    Marshal.SizeOf(typeof(NativeMethods.HttpServiceConfigUrlAclSet)),
                    IntPtr.Zero);
                if (result != 0)
                {
                    var exception = GetException("HttpSetServiceConfigurationAcl", result);
                    Trace.TraceWarning(
                        $"Exception during HttpSetServiceConfigurationAcl: {exception.ToDetailedString()}.");
                    if (throwOnError)
                    {
                        throw exception;
                    }
                }
            }
            finally
            {
                result = NativeMethods.HttpTerminate(NativeMethods.HttpInitializeConfig, IntPtr.Zero);
                if (result != 0)
                {
                    var exception = GetException("HttpTerminate", result);
                    Trace.TraceWarning(
                        $"Exception during HttpSetServiceConfigurationAcl: {exception.ToDetailedString()}.");
                    if (throwOnError)
                    {
                        throw exception;
                    }
                }
            }
        }

        private static Exception GetException(string fcn, int errorCode)
        {
            var inner = GetWin32Exception(errorCode);
            return new Exception($"{fcn} failed: {inner.ToDetailedString()}", inner);
        }

        private static string CreateSddl(string sid)
        {
            return $"D:(A;;GX;;;{sid})";
        }

        private static Exception GetWin32Exception(int errorCode)
        {
            return Marshal.GetExceptionForHR(HResultFromWin32(errorCode));
        }

        private static int HResultFromWin32(int errorCode)
        {
            if (errorCode <= 0) return errorCode;
            return (int)((0x0000FFFFU & ((uint)errorCode)) | (7U << 16) | 0x80000000U);
        }

        private static class NativeMethods
        {
            public const int HttpServiceConfigUrlAclInfo = 2;

            public const int HttpInitializeConfig = 2;

            [StructLayout(LayoutKind.Sequential)]
            public struct HttpapiVersion
            {
                public HttpapiVersion(ushort major, ushort minor)
                {
                    this.Major = major;
                    this.Minor = minor;
                }

                public ushort Major;

                public ushort Minor;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct HttpServiceConfigUrlAclKey
            {
                [MarshalAs(UnmanagedType.LPWStr)]
                public string UrlPrefix;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct HttpServiceConfigUrlAclParam
            {
                [MarshalAs(UnmanagedType.LPWStr)]
                public string Sddl;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct HttpServiceConfigUrlAclSet
            {
                public HttpServiceConfigUrlAclKey Key;

                public HttpServiceConfigUrlAclParam Param;
            }

            [DllImport("httpapi.dll", ExactSpelling = true, EntryPoint = "HttpSetServiceConfiguration")]
            public static extern int HttpSetServiceConfigurationAcl(
                IntPtr mustBeZero,
                int configId,
                [In] ref HttpServiceConfigUrlAclSet configInfo,
                int configInfoLength,
                IntPtr mustBeZero2);

            [DllImport("httpapi.dll", ExactSpelling = true, EntryPoint = "HttpDeleteServiceConfiguration")]
            public static extern int HttpDeleteServiceConfigurationAcl(
                IntPtr mustBeZero,
                int configId,
                [In] ref HttpServiceConfigUrlAclSet configInfo,
                int configInfoLength,
                IntPtr mustBeZero2);

            [DllImport("httpapi.dll")]
            public static extern int HttpInitialize(HttpapiVersion version, int flags, IntPtr mustBeZero);

            [DllImport("httpapi.dll")]
            public static extern int HttpTerminate(int flags, IntPtr mustBeZero);
        }
    }
}