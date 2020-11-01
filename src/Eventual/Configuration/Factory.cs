namespace Eventual.Configuration
{
    using System;
    using System.Linq;
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
                Setup setup,
                Func<IServiceProvider, Setup> loadConfigurationIntoSetup,
                Func<IServiceProvider, Task> startFunc)
        {

            services.AddSingleton<IPublisher, DefaultPublisher>();
            services.AddSingleton<ISubscriber, DefaultSubscriber>();
            services.AddSingleton<IBus, DefaultBus>();
            services.AddSingleton<ISerializer, DefaultSerializer>();

            services.AddSingleton<IDispatcher, DefaultDispatcher>();
            services.AddSingleton(svc => svc.GetService<Setup>().Transport.GetConfiguration());
            services.AddSingleton<IInitBus>(svc => new DefaultInitBus(svc, startFunc));
            services.AddHostedService<BusHostedService>();

            services.AddSingleton(loadConfigurationIntoSetup);

            //setup all the consumers
            //confirm if there are any consumers which are already registered with the container.
            var containerRegisteredConsumers = services
                .Where(x => x.ServiceType.IsGenericType)
                .Where(x => x.ServiceType.GetGenericTypeDefinition().IsAssignableFrom(typeof(IConsumer<>)))
                .ToList();

            //register manually setup consumers
            foreach (var consumer in setup.Consumers)
            {
                var consumerInterfaceType = typeof(IConsumer<>).MakeGenericType(consumer.MessageType);
                services.AddScoped(consumerInterfaceType, consumer.ConsumerType);
            }

            //ensure ioc registered consumers are known about in Eventual, as it will need to setup the subscriptions
            foreach (var registeredController in containerRegisteredConsumers)
            {
                setup.Subscribe(registeredController.ImplementationType);
            }


            //middleware
            //publishing
            services.AddSingleton(typeof(MessagePublishContextMiddleware<>));
            services.AddSingleton(svc => svc.GetService<Setup>().PublishContextActions);
            services.AddTransient(typeof(InvokePublish<>));

            var pa = setup.PublishContextActions;
            pa.InvokePublisherAction = pa.InvokePublisherAction ?? typeof(InvokePublish<>);

            //subscriptions
            services.AddSingleton(typeof(ReceivedMessageMiddleware<>));
            services.AddSingleton(svc => svc.GetService<Setup>().ReceivedContextActions);
            services.AddTransient(typeof(InvokeConsumer<>));
            services.AddTransient(typeof(LogReceivedMessage<>));
            services.AddTransient(typeof(DefaultMessageAck<>));

            var ra = setup.ReceivedContextActions;
            ra.DeadLetterAction = ra.DeadLetterAction ?? typeof(DefaultMessageAck<>);
            ra.LoggingAction = ra.LoggingAction ?? typeof(LogReceivedMessage<>);
            ra.InvokeConsumerAction = ra.InvokeConsumerAction ?? typeof(InvokeConsumer<>);

        }
    }

}