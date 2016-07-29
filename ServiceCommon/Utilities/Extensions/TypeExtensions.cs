using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceCommon.Utilities.Extensions
{
    /// <summary>
    /// The type extensions.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns the non-generic type name without any special characters.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The non-generic type name without any special characters.
        /// </returns>
        public static string GetUnadornedTypeName(this Type type)
        {
            var index = type.Name.IndexOf('`');

            return index > 0 ? type.Name.Substring(0, index) : type.Name;
        }

        /// <summary>
        /// Returns a string representation of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="includeNamespace">
        /// A value indicating whether or not to include the namespace name.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> representing the name of <paramref name="type"/>.
        /// </returns>
        public static string GetParseableName(this Type type, bool includeNamespace = true)
        {
            var builder = new StringBuilder();
            GetParseableName(type, builder, new Queue<Type>(type.GenericTypeArguments), includeNamespace);
            return builder.ToString();
        }

        /// <summary>
        /// Returns a string representation of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="builder">
        /// The <see cref="StringBuilder"/> to append results to.
        /// </param>
        /// <param name="typeArguments">
        /// The type arguments of <paramref name="type"/>.
        /// </param>
        /// <param name="includeNamespace">
        /// A value indicating whether or not to include the namespace name.
        /// </param>
        private static void GetParseableName(
            Type type, 
            StringBuilder builder, 
            Queue<Type> typeArguments, 
            bool includeNamespace = true)
        {
            if (type.DeclaringType != null)
            {
                // This is not the root type.
                GetParseableName(type.DeclaringType, builder, typeArguments, includeNamespace);
                builder.Append('.');
            }
            else if (!string.IsNullOrWhiteSpace(type.Namespace) && includeNamespace)
            {
                // This is the root type.
                builder.Append(type.Namespace + '.');
            }

            if (type.IsGenericType)
            {
                // Get the unadorned name, the generic parameters, and add them together.
                var unadornedTypeName = type.GetUnadornedTypeName();

                var generics =
                    Enumerable.Range(0, Math.Min(type.GetGenericArguments().Count(), typeArguments.Count))
                        .Select(_ => typeArguments.Dequeue());
                var genericParameters = string.Join(
                    ",", 
                    generics.Select(generic => GetParseableName(generic, includeNamespace)));
                builder.AppendFormat("{0}<{1}>", unadornedTypeName, genericParameters);
            }
            else
            {
                builder.Append(type.Name);
            }
        }
    }
}
