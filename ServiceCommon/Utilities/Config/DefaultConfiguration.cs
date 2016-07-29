using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ServiceCommon.Utilities.Config
{
    public static class DefaultConfiguration
    {
        public static void SetEnvironmentValues<T>(T instance, HostingEnvironment environment) where T : IConfiguration
        {
            var type = typeof(T);

            // Get all properties.
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (properties.Length == 0)
            {
                return;
            }

            foreach (var property in properties)
            {
                // Skip properties which have no set method.
                var setter = property.GetSetMethod();
                if (setter == null)
                {
                    continue;
                }

                // Get the default value attributes.
                var defaults = property.GetCustomAttributes<DefaultAttribute>();
                if (defaults == null)
                {
                    continue;
                }

                // Find the most suitable default value for this property.
                object value = null;
                foreach (var attr in defaults)
                {
                    if (string.IsNullOrWhiteSpace(attr.Environment))
                    {
                        value = attr.Value;
                    }

                    if (EnvironmentMatches(environment.Name, attr))
                    {
                        value = attr.Value;
                        break;
                    }
                }

                // Set the default value.
                setter.Invoke(instance, new[] { ConvertValue(value, property) });
            }
        }

        private static bool EnvironmentMatches(string environment, DefaultAttribute defaultAttribute)
        {
            return string.Equals(defaultAttribute.Environment, environment, StringComparison.OrdinalIgnoreCase);
        }

        private static object ConvertValue(object value, PropertyInfo target)
        {
            if (value == null)
            {
                return null;
            }

            var targetType = target.PropertyType;
            var originalType = value.GetType();
            if (originalType == targetType)
            {
                return value;
            }

            if (originalType == typeof(string))
            {
                var valueString = (string)value;
                if (targetType == typeof(string[]))
                {
                    return
                        valueString.Split(',').Select(_ => _.Trim()).Where(_ => !string.IsNullOrWhiteSpace(_)).ToArray();
                }

                if (targetType == typeof(List<string>))
                {
                    return
                        valueString.Split(',').Select(_ => _.Trim()).Where(_ => !string.IsNullOrWhiteSpace(_)).ToList();
                }
                
                if (targetType == typeof(DateTime))
                {
                    return DateTime.Parse(valueString);
                }
                
                if (targetType == typeof(DateTimeOffset))
                {
                    return DateTimeOffset.Parse(valueString);
                }

                if (targetType == typeof(TimeSpan))
                {
                    return DateTime.Parse(valueString);
                }

                if (targetType == typeof(Guid))
                {
                    return Guid.Parse(valueString);
                }

                if (typeof(Enum).IsAssignableFrom(targetType))
                {
                    return Enum.Parse(targetType, valueString, true);
                }
            }

            throw new InvalidCastException(
                string.Format(
                    "Error converting value \"{0}\" of type {1} to type {2} for property {3}",
                    value,
                    originalType,
                    targetType,
                    target.Name));
        }
    }
}