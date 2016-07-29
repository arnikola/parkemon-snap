using System.Threading.Tasks;
using Orleans;

namespace ServiceCommon.Actors
{
    using System;
    using Orleans.Runtime;

    using ServiceCommon.Config;

    public interface ITestGrain : IGrainWithIntegerKey
    {
        Task InitializeTestra();
    }

    public class TestGrain : Grain, ITestGrain
    {
        public async Task InitializeTestra()
        {
            //this.timer = this.RegisterTimer(_ => this.Setup(), null, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1));
            //return Task.FromResult(0);
            this.logger.Info("Initializing test data.");
            var win = this.GrainFactory.GetGrain<IParkGrain>(new Guid());
            await win.WinHackathon();
            //var settings = await mcdo.GetSettings();
            //settings.Details.Address = "261-267 High St Kew VIC 3101 Australia";
            //settings.Details.Location = PlsBeInMelbourne;
            //settings.Details.Description = "I'm lovin' it!";
            //settings.Details.Name = "McDonald's";
            //settings.Email = ArtekNikololovsky;
            //settings.Details.PhoneNumber = "02 9875 7100";
            //settings.Details.LogoUrl =
            //    "https://lh5.googleusercontent.com/-avTHbIvvjKY/AAAAAAAAAAI/AAAAAAAAAoY/XbY3XEWyohs/s0-c-k-no-ns/photo.jpg";
            //settings.Details.ImageUrls = new List<string>
            //{
            //    "https://mcdonalds.com.au/sites/mcdonalds.com.au/files/mcd5128_food_mob.png"
            //};
            //settings.OfferSettings.Seats = 120;
            //settings.OfferSettings.MaxOfferPercentage = 35;

            //await mcdo.SetSettings(settings);
            //await mcdo.UpdateOffer(new Offer { Title = "McDoozy", Body = "McMazing taste, McMazing value", Active = true, Expires = DateTimeOffset.UtcNow.AddDays(200), DiscountPercent = 20, RemainingCoupons = 24 });
        }

        private readonly StorageConfig storageConfig;

        private Logger logger;

        private IDisposable timer;

        public TestGrain(StorageConfig storageConfig)
        {
            this.storageConfig = storageConfig;
        }

        public override Task OnActivateAsync()
        {
            this.logger = this.GetLogger("TestData");
            return base.OnActivateAsync();
        }

        private async Task Setup()
        {
            this.logger.Info("Initializing test data.");
            var win = this.GrainFactory.GetGrain<IParkGrain>(new Guid());
            await win.WinHackathon();
            //var settings = await mcdo.GetSettings();
            //settings.Details.Address = "261-267 High St Kew VIC 3101 Australia";
            //settings.Details.Location = PlsBeInMelbourne;
            //settings.Details.Description = "I'm lovin' it!";
            //settings.Details.Name = "McDonald's";
            //settings.Email = ArtekNikololovsky;
            //settings.Details.PhoneNumber = "02 9875 7100";
            //settings.Details.LogoUrl =
            //    "https://lh5.googleusercontent.com/-avTHbIvvjKY/AAAAAAAAAAI/AAAAAAAAAoY/XbY3XEWyohs/s0-c-k-no-ns/photo.jpg";
            //settings.Details.ImageUrls = new List<string>
            //{
            //    "https://mcdonalds.com.au/sites/mcdonalds.com.au/files/mcd5128_food_mob.png"
            //};
            //settings.OfferSettings.Seats = 120;
            //settings.OfferSettings.MaxOfferPercentage = 35;

            //await mcdo.SetSettings(settings);
            //await mcdo.UpdateOffer(new Offer { Title = "McDoozy", Body = "McMazing taste, McMazing value", Active = true, Expires = DateTimeOffset.UtcNow.AddDays(200), DiscountPercent = 20, RemainingCoupons = 24 });

            this.timer?.Dispose();
            this.timer = null;
            this.logger.Info("Completed initializing test data.");
        }
    }
}
