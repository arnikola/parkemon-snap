using Newtonsoft.Json;

namespace ServiceCommon.Utilities.Serialization
{
    /// <summary>
    ///     Global constants.
    /// </summary>
    public static class SerializationSettings
    {
        /// <summary>
        ///     Initializes static members of the <see cref="SerializationSettings"/> class.
        /// </summary>
        static SerializationSettings()
        {
            JsonConfig = new JsonSerializerSettings
            {
                ContractResolver = JsonContractResolver.Instance,
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Populate,
                CheckAdditionalContent = false,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                DateFormatString = "dd MMM yyyy HH:mm:ss zzz"
            };

            JsonSerializer = JsonSerializer.Create(JsonConfig);

            // Set the default JSON serializer.
            JsonConvert.DefaultSettings = () => JsonConfig;
        }

        /// <summary>
        ///     Gets the JSON serializer settings.
        /// </summary>
        public static JsonSerializerSettings JsonConfig { get; }

        /// <summary>
        ///     Gets the JSON serializer.
        /// </summary>
        public static JsonSerializer JsonSerializer { get; private set; }
    }
}