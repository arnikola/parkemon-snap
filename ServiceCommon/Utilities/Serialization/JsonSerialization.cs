using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans.CodeGeneration;
using Orleans.Serialization;

namespace ServiceCommon.Utilities.Serialization
{
    /// <summary>
    /// Provides support for serializing JSON values.
    /// </summary>
    [RegisterSerializer]
    public class JsonSerialization
    {
        /// <summary>
        /// Initializes static members of the <see cref="JsonSerialization"/> class.
        /// </summary>
        static JsonSerialization()
        {
            Register();
        }

        /// <summary>
        /// The deep copier.
        /// </summary>
        /// <param name="original">
        /// The original.
        /// </param>
        /// <returns>
        /// The copy.
        /// </returns>
        public static object DeepCopier(object original)
        {
            return original;
        }

        /// <summary>
        /// Serializes an object to a stream.
        /// </summary>
        /// <param name="obj">
        /// The object being serialized.
        /// </param>
        /// <param name="stream">
        /// The stream to serialize to.
        /// </param>
        /// <param name="expected">
        /// The expected type.
        /// </param>
        public static void Serialize(object obj, BinaryTokenStreamWriter stream, Type expected)
        {
            var str = JsonConvert.SerializeObject(obj, expected, SerializationSettings.JsonConfig);
            SerializationManager.SerializeInner(str, stream, typeof(string));
        }

        /// <summary>
        /// Deserializes a JSON object.
        /// </summary>
        /// <param name="expected">
        /// The expected type.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The deserialized object.
        /// </returns>
        public static object Deserialize(Type expected, BinaryTokenStreamReader stream)
        {
            var str = (string)SerializationManager.DeserializeInner(typeof(string), stream);
            return JsonConvert.DeserializeObject(str, expected, SerializationSettings.JsonConfig);
        }

        /// <summary>
        /// Registers this class with the <see cref="SerializationManager"/>.
        /// </summary>
        public static void Register()
        {
            var jsonTypes = new[]
            { typeof(JObject), typeof(JArray), typeof(JToken), typeof(JValue), typeof(JProperty), typeof(JConstructor) };
            foreach (var type in jsonTypes)
            {
                SerializationManager.Register(type, DeepCopier, Serialize, Deserialize);
            }
        }
    }
}