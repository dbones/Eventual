namespace Eventual.Configuration
{
    using System;
    using System.Threading.Tasks;
    using Infrastructure.NamingStrategies;
    using Microsoft.Extensions.DependencyInjection;
    using Middleware.Publishing;
    using Middleware.Subscribing;
    using Transport;

    public class RabbitMqTransportFactory : Factory
    {
        public override void RegisterServices(
            IServiceCollection services,
            HostSetup setup,
            Func<IServiceProvider, HostSetup> loadConfigurationIntoSetup,
            Func<IServiceProvider, Task> startFunc)
        {
            base.RegisterServices(services, setup, loadConfigurationIntoSetup, startFunc);
            services.AddSingleton<IConnection, RabbitMqConnection>();
            services.AddSingleton<INamingStrategy, RabbitMqNamingStrategy>();
            services.AddSingleton(svc => ((RabbitMqHostSetup)setup).BusConfiguration);

            //middleware
            services.AddTransient(typeof(ReadMessageFromQueueIntoContext<>));
            services.AddTransient(typeof(PrepareMessageContextForPublish<>));

            setup.PublishContextActions.PrepareMessageContextForPublish = typeof(PrepareMessageContextForPublish<>);
            setup.ReceivedContextActions.ReadMessageFromQueueIntoContextAction = typeof(ReadMessageFromQueueIntoContext<>);
        }
    }
}