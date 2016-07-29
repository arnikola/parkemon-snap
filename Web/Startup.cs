using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Features.ResolveAnything;
using Autofac.Integration.WebApi;
using WebStreams.Server;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Newtonsoft.Json;
using Orleans;
using Owin;
using ServiceCommon;
using ServiceCommon.Config;
using ServiceCommon.Utilities;
using ServiceCommon.Utilities.Config;
using ServiceCommon.Utilities.Extensions;
using Web;

[assembly: OwinStartup(typeof(Startup))]

namespace Web
{
    using System;
    using System.Threading.Tasks;

    using PowerArgs;

    using ServiceCommon.Utilities.Network.Http;

    using Web.Filters;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var args = Args.GetAmbientArgs<CommandLineArguments>();
            var environment = new HostingEnvironment(args.Environment);
            Trace.TraceInformation("Environment: " + environment.Name);
            var container = GetServiceContainer(environment);
            var httpConfig = GetHttpConfiguration(container);

            // Get instances config.
            var actorClientConfig = container.Resolve<OrleansConfiguration>().GetConfiguration();
            
            // Start the actor client.
            RetryUtil.Retry(() => GrainClient.Initialize(actorClientConfig));
            
            // Start the Web server.
            Trace.TraceInformation("Starting main Web site.");

            // Redirect all plaintext clients to HTTPS/WSS if TLS certificate has been set.
            var siteConfig = container.Resolve<SiteConfig>();
            if (!string.IsNullOrWhiteSpace(siteConfig.CertificateThumbprint))
            {
                app.RedirectToHttps();
            }

            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(httpConfig);
            app.Use(PropagateUserAuth);
            app.UseWebApi(httpConfig);
            httpConfig.EnsureInitialized();

            app.UseWebStreams(
                new WebStreamsSettings
                {
                    GetControllerInstanceDelegate = type => container.Resolve(type),
                    JsonSerializerSettings = container.Resolve<JsonSerializerSettings>()
                });
            
            // Configure static content.
            var wwwRoot = siteConfig.OverrideWwwRoot ?? Path.Combine(AssemblyUtil.GetDirectory(typeof(Startup).Assembly), "wwwroot");
            Trace.TraceInformation("wwwroot: " + wwwRoot);
            app.UseFileServer(
                new FileServerOptions
                {
                    RequestPath = new PathString(""),
                    FileSystem = new PhysicalFileSystem(wwwRoot)
                });

            Trace.TraceInformation("Started Web application.");
            Trace.TraceInformation("Running");
        }

        /// <summary>
        /// Reads user authorization header and passes it along to grains.
        /// </summary>
        /// <param name="ctx">The OWIN context.</param>
        /// <param name="next">The next middleware.</param>
        /// <returns>A <see cref="Task"/> representing the work performed.</returns>
        private static Task PropagateUserAuth(IOwinContext ctx, Func<Task> next)
        {
            string[] authHeader;
            if (!ctx.Request.Headers.TryGetValue("Authorization", out authHeader) || authHeader.Length != 1
                || string.IsNullOrWhiteSpace(authHeader[0]))
            {
                return next();
            }

            // Strip off kind.
            if (authHeader[0].StartsWith("Basic "))
            {
                authHeader[0] = authHeader[0].Substring("Basic ".Length);

                var parts = authHeader[0].Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    ActorRequestContext.SetUserId(parts[0]);
                    ActorRequestContext.SetUserSecret(parts[1]);
                }
            }
            else if (authHeader[0].StartsWith("Bearer "))
            {
                authHeader[0] = authHeader[0].Substring("Bearer ".Length);

                //TODO: Implement JWTs
            }

            return next();
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

            // Allow exceptions to define their own error codes.
            httpConfig.Filters.Add(new ExceptionDetailsFilter());
            return httpConfig;
        }

        private static ILifetimeScope GetServiceContainer(HostingEnvironment environment)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(environment);
            builder.RegisterConfigurations();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            builder.Register(_ => GrainClient.GrainFactory).SingleInstance();

            // Load all modules & build the container.
            builder.RegisterModule<CommonServices>();
            return ServiceLocator.Container = builder.Build();
        }
    }
}