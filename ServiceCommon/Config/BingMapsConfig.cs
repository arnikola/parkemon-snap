namespace ServiceCommon.Config
{
    using ServiceCommon.Utilities.Config;

    public class BingMapsConfig : IConfiguration
    {
        public BingMapsConfig(HostingEnvironment environment)
        {
            DefaultConfiguration.SetEnvironmentValues(this, environment);
        }

        [Default("")]
        public string MapApiKey { get; set; }

        [Default("")]
        public string MapApiEndpoint { get; set; }
    }
}