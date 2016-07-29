using System;

namespace Web
{
    using Microsoft.Owin.Hosting;

    using PowerArgs;

    using ServiceCommon.Config;
    using ServiceCommon.Utilities.Config;
    using ServiceCommon.Utilities.Extensions;

    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class CommandLineArguments
    {
        [ArgExample(Environments.Dev, "Set the environment to dev."), ArgDefaultValue(Environments.Dev)]
        public string Environment { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var parsedArgs = Args.Parse<CommandLineArguments>(args);
            var environment = new HostingEnvironment(parsedArgs.Environment);
            var siteConfig = new SiteConfig(environment);
            
            while (true)
            {
                try
                {
                    var options = new StartOptions();
                    siteConfig.ListenAddresses.ForEach(options.Urls.Add);
                    using (WebApp.Start<Startup>(options))
                    {
                        Console.WriteLine("Press Esc to exit, or any other key to restart.");
                        while (Console.ReadKey().Key != ConsoleKey.Escape)
                        {
                            // Wait for another keystroke.
                        }

                        // Exit.
                        break;
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"FATAL Exception:\n{exception.ToDetailedString()}");
                }

                if (!Environment.UserInteractive)
                {
                    // If this is not running in an interactive terminal, exit.
                    break;
                }

                Console.WriteLine("Press Esc to exit, or any other key to restart.");
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    break;
                }
            }
        }
    }
}