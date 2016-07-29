namespace ServiceCommon.Utilities.Network
{
    using System;
    using System.Linq;

    using NetFwTypeLib;

    /// <summary>
    /// Utilities for managing firewall rules.
    /// </summary>
    public static class Firewall
    {
        /// <summary>
        /// The protocols.
        /// </summary>
        public enum Protocol
        {
            /// <summary>
            /// The TCP protocol.
            /// </summary>
            Tcp,

            /// <summary>
            /// The UDP protocol
            /// </summary>
            Udp
        }

        /// <summary>
        /// Returns true if the specified port is authorized or false otherwise.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="protocol">The protocol.</param>
        /// <returns>True if the specified port is authorized or false otherwise.</returns>
        public static bool IsOpened(int port, Protocol protocol)
        {
            var manager = GetManager();
            return
                manager.LocalPolicy.CurrentProfile.GloballyOpenPorts.Cast<INetFwOpenPort>()
                    .Any(_ => _.Port == port && _.Protocol == GetProtocol(protocol));
        }

        /// <summary>
        /// Allows a port through the firewall.
        /// </summary>
        /// <param name="name">The rule name.</param>
        /// <param name="port">The port.</param>
        /// <param name="protocol">The protocol.</param>
        public static void OpenPort(string name, int port, Protocol protocol)
        {
            if (IsOpened(port, protocol))
            {
                return;
            }

            var manager = GetManager();
            var rule = (INetFwOpenPort)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwOpenPort"));
            rule.Name = name;
            rule.Port = port;
            rule.Protocol = GetProtocol(protocol);
            rule.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL;
            rule.Enabled = true;
            manager.LocalPolicy.CurrentProfile.GloballyOpenPorts.Add(rule);
        }

        /// <summary>
        /// Disallows traffic to a specified port and protocol.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="protocol">The protocol.</param>
        public static void ClosePort(int port, Protocol protocol)
        {
            var manager = GetManager();
            manager.LocalPolicy.CurrentProfile.GloballyOpenPorts.Remove(port, GetProtocol(protocol));
        }

        private static NET_FW_IP_PROTOCOL_ GetProtocol(Protocol protocol)
        {
            return protocol == Protocol.Tcp
                       ? NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP
                       : NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP;
        }

        private static INetFwMgr GetManager()
        {
            return (INetFwMgr)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr"));
        }
    }
}