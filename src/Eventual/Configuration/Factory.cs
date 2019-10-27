namespace Eventual.Configuration
{
    using System;
    using System.Threading.Tasks;
    using Infrastructure;
    using Infrastructure.Hosting;
    using Infrastructure.Serialization;
    using Microsoft.Extensions.DependencyInjection;
    using Middleware;
    using Middleware.Publishing;
    using Middleware.Subscribing;

    public abstract class Factory
    {
        public virtual void RegisterServices(
                IServiceCollection services,
                HostSetup setup,
                Func<IServiceProvider, HostSetup> loadConfigurationIntoSetup,
                Func<IServiceProvider, Task> startFunc)
        {

            services.AddSingleton<IPublisher, DefaultPublisher>();
            services.AddSingleton<ISubscriber, DefaultSubscriber>();
            services.AddSingleton<IBus, DefaultBus>();
            services.AddSingleton<ISerializer, DefaultSerializer>();

            services.AddSingleton<IDispatcher, DefaultDispatcher>();
            services.AddSingleton(svc => svc.GetService<HostSetup>().GetConfiguration());
            services.AddSingleton<IInitBus>(svc => new DefaultInitBus(svc, startFunc));
            services.AddHostedService<BusHostedService>();

            services.AddSingleton(loadConfigurationIntoSetup);


            foreach (var consumer in setup.Consumers)
            {
                var consumerInterfaceType = typeof(IConsumer<>).MakeGenericType(consumer.MessageType);
                services.AddTransient(consumerInterfaceType, consumer.ConsumerType);
            }


            //middleware
            //publishing
            services.AddSingleton(typeof(MessagePublishContextMiddleware<>));
            services.AddSingleton(svc => svc.GetService<HostSetup>().PublishContextActions);
            services.AddTransient(typeof(InvokePublish<>));

            var pa = setup.PublishContextActions;
            pa.InvokePublisherAction = pa.InvokePublisherAction ?? typeof(InvokePublish<>);

            //subscriptions
            services.AddSingleton(typeof(ReceivedMessageMiddleware<>));
            services.AddSingleton(svc => svc.GetService<HostSetup>().ReceivedContextActions);
            services.AddTransient(typeof(InvokeConsumer<>));
            services.AddTransient(typeof(LogReceivedMessage<>));
            services.AddTransient(typeof(MessageAck<>));

            var ra = setup.ReceivedContextActions;
            ra.DeadLetterAction = ra.DeadLetterAction ?? typeof(MessageAck<>);
            ra.LoggingAction = ra.LoggingAction ?? typeof(LogReceivedMessage<>);
            ra.InvokeConsumerAction = ra.InvokeConsumerAction ?? typeof(InvokeConsumer<>);

        }
    }

}