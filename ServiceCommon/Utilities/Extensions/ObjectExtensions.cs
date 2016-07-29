using Newtonsoft.Json;
using ServiceCommon.Utilities.Serialization;

namespace ServiceCommon.Utilities.Extensions
{
    /// <summary>
    /// The object extensions.
    /// </summary>
    public static class ObjectExtensions
    {
        public static string ToJsonString(this object value, bool indent = false)
        {
            return JsonConvert.SerializeObject(value, indent ? Formatting.Indented : Formatting.None,
                SerializationSettings.JsonConfig);
        }
    }
}
