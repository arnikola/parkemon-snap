using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.ServiceRuntime;

namespace Yams
{
    using System.Security.Permissions;

    using Etg.Yams;
    using Etg.Yams.Utils;

    public class WorkerRole : RoleEntryPoint
    {
        private YamsEntryPoint yamsEntryPoint;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public override void Run()
        {
            this.RunAsync().Wait();
            base.Run();
        }

        private async Task RunAsync()
        {
            var config = new WorkerRoleConfig();
            var yamsConfig = new YamsConfigBuilder(
                // mandatory configs
                config.StorageDataConnectionString,
                DeploymentIdUtils.CloudServiceDeploymentId,
                RoleEnvironment.CurrentRoleInstance.UpdateDomain.ToString(),
                RoleEnvironment.CurrentRoleInstance.Id,
                config.CurrentRoleInstanceLocalStoreDirectory)
                // optional configs
                .SetCheckForUpdatesPeriodInSeconds(config.UpdateFrequencyInSeconds)
                .SetApplicationRestartCount(config.ApplicationRestartCount)
                .Build();
            this.yamsEntryPoint = new YamsEntryPoint(yamsConfig);

            try
            {
                Trace.TraceInformation("Yams is starting");
                await this.yamsEntryPoint.Start();
                Trace.TraceInformation("Yams has started");
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        public override bool OnStart()
        {
            Trace.TraceInformation("Yams WorkerRole is starting");
            ServicePointManager.DefaultConnectionLimit = 1000;
            RoleEnvironment.Changing += this.RoleEnvironmentChanging;
            var result = base.OnStart();
            Trace.TraceInformation("Yams WorkerRole has started");
            return result;
        }

        public override void OnStop()
        {
            this.StopAsync().Wait();
        }

        private async Task StopAsync()
        {
            Trace.TraceInformation("Yams WorkerRole is stopping");
            RoleEnvironment.Changing -= this.RoleEnvironmentChanging;
            if (this.yamsEntryPoint != null)
            {
                await this.yamsEntryPoint.Stop();
            }

            base.OnStop();
            Trace.TraceInformation("Yams has stopped");
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // If a configuration setting is changing;
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                e.Cancel = true;
            }
        }
    }
    public class WorkerRoleConfig
    {
        public WorkerRoleConfig()
        {
            this.UpdateFrequencyInSeconds = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("UpdateFrequencyInSeconds"));
            this.ApplicationRestartCount = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("ApplicationRestartCount"));
            this.StorageDataConnectionString = RoleEnvironment.GetConfigurationSettingValue("StorageDataConnectionString");
            this.CurrentRoleInstanceLocalStoreDirectory = RoleEnvironment.GetLocalResource("LocalStoreDirectory").RootPath;
        }

        public string StorageDataConnectionString { get; }

        public string CurrentRoleInstanceLocalStoreDirectory { get; }

        public int UpdateFrequencyInSeconds { get; }

        public int ApplicationRestartCount { get; }
    }
}
