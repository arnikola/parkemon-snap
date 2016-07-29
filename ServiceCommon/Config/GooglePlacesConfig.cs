namespace ServiceCommon.Config
{
    using ServiceCommon.Utilities.Config;

    public class GooglePlacesConfig : IConfiguration
    {
        public GooglePlacesConfig(HostingEnvironment environment)
        {
            DefaultConfiguration.SetEnvironmentValues(this, environment);
        }

        [Default("AIzaSyCCzYjF0PVeNOqs1E7CQ7BmI886RTAD_Ho")]
        public string ApiKey { get; set; }

        [Default("https://maps.googleapis.com/maps/api/place")]
        public string BaseUrl { get; set; }
    }
}