namespace Eventual
{
    using System;
    using Configuration;
    using Microsoft.Extensions.Hosting;

    public static class HostExtensions
    {
        /// <summary>
        /// setup eventual
        /// </summary>
        public static IHostBuilder UseEventual<T>(this IHostBuilder builder, Action<T> conf) where T : HostSetup, new()
        {
            var busBuilder = new BusBuilder();
            busBuilder.Configure(conf);

            builder.ConfigureServices((ctx, services) =>
            {
                busBuilder.SetupContainer(services);
            });

            return builder;
        }
    }

}