// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Dapr Labs">
//   Copyright © Dapr Labs. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace LogRetriever
{
    using System;

    using global::LogRetriever.Config;

    using Microsoft.Owin.Hosting;

    using PowerArgs;

    using ServiceCommon.Config;
    using ServiceCommon.Utilities.Config;

    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class CommandLineArguments
    {
        [ArgExample(Environments.Dev, "Set the environment to dev."), ArgDefaultValue(Environments.Dev)]
        public string Environment { get; set; }
    }
    /// <summary>
    /// The program entry point.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The program entry point.
        /// </summary>
        /// <param name="args">
        /// The commandline arguments.
        /// </param>
        public static void Main(string[] args)
        {
            var parsedArgs = Args.Parse<CommandLineArguments>(args);
            var environment = new HostingEnvironment(parsedArgs.Environment);
            var siteConfig = new LogRetrieverConfig(environment);

            var options = new StartOptions();
            siteConfig.ListenAddresses.ForEach(options.Urls.Add);
            using (WebApp.Start<Startup>(options))
            {
                Console.ReadKey();
            }
        }
    }
}
