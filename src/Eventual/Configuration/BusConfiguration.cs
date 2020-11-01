namespace Eventual.Configuration
{
    using System.Reflection;

    public abstract class BusConfiguration
    {
        public string ServiceName { get; set; } =
            Assembly.GetEntryAssembly().GetName().Name;
    }
}