using LogRetriever;

using Microsoft.Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace LogRetriever
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;

    using Autofac;
    using Autofac.Features.ResolveAnything;
    using Autofac.Integration.WebApi;

    using global::LogRetriever.Config;

    using Microsoft.Owin;
    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.StaticFiles;

    using Newtonsoft.Json;

    using Owin;

    using PowerArgs;

    using ServiceCommon;
    using ServiceCommon.Config;
    using ServiceCommon.Utilities;
    using ServiceCommon.Utilities.Config;
    using ServiceCommon.Utilities.Extensions;
    using ServiceCommon.Utilities.Network.Http;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var args = Args.GetAmbientArgs<CommandLineArguments>();
            var environment = new HostingEnvironment(args.Environment);
            Trace.TraceInformation("Environment: " + environment.Name);
            var container = GetServiceContainer(environment);
            var httpConfig = GetHttpConfiguration(container);
            
            // Start the Web server.
            Trace.TraceInformation("Starting web application.");

            // Redirect all plaintext clients to HTTPS/WSS if TLS certificate has been set.
            var config = container.Resolve<LogRetrieverConfig>();
            if (!string.IsNullOrWhiteSpace(config.CertificateThumbprint))
            {
                app.RedirectToHttps();
            }

            app.UseBasicAuth(config.UserName, config.Password);

            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(httpConfig);
            app.UseWebApi(httpConfig);
            httpConfig.EnsureInitialized();
            
            // Configure static content.
            var wwwRoot = Path.Combine(AssemblyUtil.GetDirectory(typeof(Startup).Assembly), "wwwroot");
            Trace.TraceInformation("wwwroot: " + wwwRoot);
            app.UseFileServer(
                new FileServerOptions
                {
                    RequestPath = new PathString(""),
                    FileSystem = new PhysicalFileSystem(wwwRoot)
                });
            
            Trace.TraceInformation("Running");
        }

        private static HttpConfiguration GetHttpConfiguration(ILifetimeScope container)
        {
            var httpConfig = new HttpConfiguration();

            var errorDetailPolicy = container.Resolve<SiteConfig>().AlwaysIncludeErrorDetail
                                        ? IncludeErrorDetailPolicy.Always
                                        : IncludeErrorDetailPolicy.LocalOnly;
            httpConfig.IncludeErrorDetailPolicy = errorDetailPolicy;

            // Configure routing
            httpConfig.MapHttpAttributeRoutes();
            httpConfig.Formatters.JsonFormatter.SerializerSettings = container.Resolve<JsonSerializerSettings>();

            // Disable the XML formatter.
            var xmlFormat =
                httpConfig.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            if (xmlFormat != null)
            {
                httpConfig.Formatters.XmlFormatter.SupportedMediaTypes.Remove(xmlFormat);
            }

            httpConfig.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            return httpConfig;
        }

        private static ILifetimeScope GetServiceContainer(HostingEnvironment environment)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(environment);
            builder.RegisterConfigurations();
            builder.RegisterType<LogRetriever>().SingleInstance().AsImplementedInterfaces();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            // Load all modules & build the container.
            builder.RegisterModule<CommonServices>();
            return ServiceLocator.Container = builder.Build();
        }

        /// <summary>
        /// Returns the current assembly path.
        /// </summary>
        /// <returns>
        /// The current assembly path.
        /// </returns>
        private static string GetAssemblyPath()
        {
            return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        }
    }
}