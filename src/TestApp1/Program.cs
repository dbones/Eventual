using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestApp1
{
    using System.IO;
    using Consumers;
    using Events;
    using Eventual;
    using Eventual.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.AddJsonFile(Path.Combine("config", "stagesettings.json"), true);
                })
                .UseEventual<RabbitMqHostSetup>((setup) =>
                {
                    setup.FromConfiguration("RabbitMq");

                    //setup.BusConfiguration.ServiceName = "TestApp1";
                    setup.BusConfiguration.ConnectionString = "amqp://172.22.101.111/%2f";
                    setup.ConfigureSubscription<BookOrderedConsumer, BookOrdered>();

                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


            host.Build().Run();
        }

            
    }
}
