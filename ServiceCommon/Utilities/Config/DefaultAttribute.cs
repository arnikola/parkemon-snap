using System;

namespace ServiceCommon.Utilities.Config
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DefaultAttribute : Attribute
    {
        public DefaultAttribute(object value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the name of the environment in which this value applies.
        /// </summary>
        public string Environment { get; set; }
    }
}
