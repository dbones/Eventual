namespace TestApp1
{
    using System.IO;
    using Consumers;
    using Eventual;
    using Eventual.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.AddJsonFile(Path.Combine("config", "stagesettings.json"), true);
                })
                .ConfigureEventual(config =>
                {
                    config.UseTransport<RabbitMq>(setup =>
                    {
                        //setup.BusConfiguration.ServiceName = "TestApp1";
                        setup.BusConfiguration.ConnectionString = "amqp://admin:admin@localhost/%2f";
                        setup.FromConfiguration("RabbitMq");
                    });

                    config.Subscribe<BookOrderedConsumer>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


            host.Build().Run();
        }

            
    }
}
