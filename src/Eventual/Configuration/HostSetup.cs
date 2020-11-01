namespace Eventual.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Middleware.Publishing;
    using Middleware.Subscribing;

    public abstract class TransportSetup
    {
        protected internal abstract BusConfiguration GetConfiguration();
        protected internal abstract Factory GetFactory();
        internal string ConfigurationEntryName { get; set; }

        public void FromConfiguration(string configurationEntry)
        {
            ConfigurationEntryName = configurationEntry;
        }
    }


    public static class Internals
    {
        public static Factory GetFactory(Setup setup)
        {
            return setup.Transport.GetFactory();
        }

        public static BusConfiguration GetConfiguration(Setup setup)
        {
            return setup.Transport.GetConfiguration();
        }
    }


    public class Setup
    {
        public Setup()
        {
            Consumers = new List<ConsumerSetup>();
            PublishContextActions = new PublishContextActions();
            ReceivedContextActions = new ReceivedContextActions();
        }

        internal TransportSetup Transport { get; set; }

        /// <summary>
        /// message type / consumer type
        /// </summary>
        internal List<ConsumerSetup> Consumers { get; set; }

        public PublishContextActions PublishContextActions { get; set; }
        public ReceivedContextActions ReceivedContextActions { get; set; }

        public void Subscribe(Type consumer, Action<ConsumerSetup> conf = null)
        {
            var consumerType = consumer.GetInterfaces()
                .Where(x => x.IsGenericType)
                .Select(x =>
                    new
                    {
                        Definition = x.GetGenericTypeDefinition(),
                        GenericArgs = x.GetGenericArguments(),
                        Type = x
                    })
                .Where(x => x.GenericArgs.Length == 1)
                .FirstOrDefault(x => x.Definition == typeof(IConsumer<>));

            if (consumerType == null) throw new Exception($"is this a IConsumer<>, {consumer}");

            var consumerSetup = new ConsumerSetup()
            {
                ConsumerType = consumer,
                MessageType = consumerType.GenericArgs[0]
            };

            conf?.Invoke(consumerSetup);
            Subscribe(consumerSetup);
        }

        public void Subscribe(ConsumerSetup setup)
        {
            Consumers.Add(setup);
        }

        public void Subscribe<T>(Action<ConsumerSetup> conf = null) where T: class
        {
            Subscribe(typeof(T), conf);
        }

        public void Subscribe<TConsumer, TMessage>(Action<ConsumerSetup> conf = null)
            where TConsumer : IConsumer<TMessage>
            where TMessage : class
        {
            Subscribe(typeof(TConsumer), conf);

            //var consumerSetup = new ConsumerSetup()
            //{
            //    ConsumerType = typeof(TConsumer),
            //    MessageType = typeof(TMessage)
            //};

            //conf?.Invoke(consumerSetup);
            //Consumers.Add(consumerSetup);
        }

        public void UseTransport<T>(Action<T> configure) where T : TransportSetup, new()
        {
            var transportSetup = new T();
            Transport = transportSetup;
            configure(transportSetup);
        }

        public void AddConsumeAction<T>() where T: IConsumeAction<T>
        {
            ReceivedContextActions.CustomActions.Add(typeof(T));
        }

        public void AddPublishAction<T>() where T : IPublishAction<T>
        {
            PublishContextActions.CustomActions.Add(typeof(T));
        }

    }

}