namespace ServiceCommon.Utilities.Config
{
    /// <summary>
    /// Represents the current environment.
    /// </summary>
    public class HostingEnvironment
    {
        public HostingEnvironment(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the current environment.
        /// </summary>
        public string Name { get; private set; }
    }
}