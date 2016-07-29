namespace Console
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Autofac;
    using Autofac.Features.ResolveAnything;

    using Newtonsoft.Json;

    using Orleans;

    using ServiceCommon;
    using ServiceCommon.Actors;
    using ServiceCommon.Config;
    using ServiceCommon.Models;
    using ServiceCommon.Utilities;
    using ServiceCommon.Utilities.Config;
    using ServiceCommon.Utilities.Extensions;
    using ServiceCommon.Utilities.Observers;

    internal class Program
    {
        private ObserverHelper observerHelper;
        //private string locationId;
        //private IDisposable restaurantStream;
        //private IDisposable offerStream;
        //private string restaurantId;

        public Program()
        {
            //this.restaurantId = "mcdo";
            //this.locationId = "parkville";
            //this.restaurantStream = null;
            //this.offerStream = null;
        }

        private static void Main(string[] args)
        {
            try
            {
                var app = new Program();
                app.Run(args).Wait();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToDetailedString());
            }
        }

        public Task Run(string[] args)
        {

            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "places":
                        //await new GooglePlaces().RunPlaces(args.Skip(1).ToArray());
                        break;
                }

            }


            var container = GetServiceContainer();
            // Get instances config.
            var actorClientConfig = container.Resolve<OrleansConfiguration>().GetConfiguration();
            Trace.TraceInformation("StorageConfig: " + JsonConvert.SerializeObject(container.Resolve<StorageConfig>()));

            // Start the actor client.
            GrainClient.Initialize(actorClientConfig);

            this.observerHelper = new ObserverHelper(GrainClient.GrainFactory);

            string command;
            Console.WriteLine("Usage: ");
            Console.WriteLine("\tloc <location>     -   Set location");
            Console.WriteLine("\tres <restaurant>   -   Set restaurant");
            Console.WriteLine("\tg                  -   Get offer");
            Console.WriteLine("\tc                  -   Claim offer");
            Console.WriteLine("\td                  -   Reject offer");
            Console.WriteLine("\tr                  -   Redeem offer");
            Console.WriteLine("\ts                  -   Seen offer");
            this.SubscribeToOfferStream();
            while (!string.IsNullOrWhiteSpace(command = ReadLine()))
            {
                var vals = command.Split(' ');
                var cmd = vals.FirstOrDefault();
                var arg = vals.Skip(1).FirstOrDefault();

                // Get deals.
                if (cmd == "loc")
                {
                    this.SubscribeToOfferStream();
                }

                
                if (cmd == "c")
                {
                    //var restaurantGrain = GrainClient.GrainFactory.GetGrain<IRestaurantGrain>(this.offer.Restaurant.Id);
                    //this.coupon = await restaurantGrain.ClaimOffer(this.offer.Offer.Id, Guid.NewGuid().ToString("N"));
                    //Console.WriteLine($"Claimed offer: {this.coupon}");
                }
            }
            return Task.FromResult(1);
        }


        private void SubscribeToOfferStream()
        {
            //this.offerStream?.Dispose();
            //this.offerStream = this.observerHelper.Get<RestaurantOffer, ILocationGrain>(this.plsBeInMelbourne).Subscribe(_ =>
            //{
            //    this.offer = _;
            //    Console.WriteLine($"[{locId}] Offer:\n{_.ToJsonString(true)}");
            //});
        }
        
        private static string ReadLine()
        {
            Console.Write("> ");
            return Console.ReadLine();
        }

        private static ILifetimeScope GetServiceContainer()
        {
            var builder = new ContainerBuilder();
            var environment = GetCurrentEnvironment();
            builder.RegisterInstance(environment);
            builder.RegisterConfigurations();
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            // Load all modules & build the container.
            builder.RegisterModule<CommonServices>();
            var container = ServiceLocator.Container = builder.Build();
            Trace.TraceInformation("Environment: " + environment.Name);
            return container;
        }

        public static HostingEnvironment GetCurrentEnvironment()
        {
            var appConfigValue = ConfigurationManager.AppSettings.Get("Environment");
            return new HostingEnvironment(appConfigValue);
        }
    }
}