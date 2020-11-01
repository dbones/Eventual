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
        public static IHostBuilder ConfigureEventual(this IHostBuilder builder, Action<Setup> setup) 
        {
            var busBuilder = new BusBuilder();
            busBuilder.Configure(setup);

            builder.ConfigureServices((ctx, services) =>
            {
                busBuilder.SetupContainer(services);
            });

            return builder;
        }
    }

}