namespace ServiceCommon.Config
{
    using ServiceCommon.Utilities.Config;

    public class ZomatoConfig : IConfiguration
    {
        public ZomatoConfig(HostingEnvironment environment)
        {
            DefaultConfiguration.SetEnvironmentValues(this, environment);
        }

        [Default("b41964f91daba0bf3276f52c914463f8")]
        public string ApiKey { get; set; }

        [Default("b41964f91daba0bf3276f52c914463f8")]
        public string BaseUrl { get; set; }
    }
}