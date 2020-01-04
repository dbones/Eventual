namespace Eventual.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Middleware.Publishing;
    using Middleware.Subscribing;

    public abstract class HostSetup
    {
        protected HostSetup()
        {
            Consumers = new List<ConsumerSetup>();
            PublishContextActions = new PublishContextActions();
            ReceivedContextActions = new ReceivedContextActions();
        }

        /// <summary>
        /// message type / consumer type
        /// </summary>
        internal List<ConsumerSetup> Consumers { get; set; }

        public PublishContextActions PublishContextActions { get; set; }
        public ReceivedContextActions ReceivedContextActions { get; set; }

        internal string ConfigurationEntryName { get; set; }

        public void ConfigureSubscription(Type consumer, Action<ConsumerSetup> conf = null)
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
            ConfigureSubscription(consumerSetup);
        }

        public void ConfigureSubscription(ConsumerSetup setup)
        {
            Consumers.Add(setup);
        }

        public void ConfigureSubscription<T>(Action<ConsumerSetup> conf = null) where T: class
        {
            ConfigureSubscription(typeof(T), conf);
        }

        public void ConfigureSubscription<TConsumer, TMessage>(Action<ConsumerSetup> conf = null)
            where TConsumer : IConsumer<TMessage>
            where TMessage : class
        {
            ConfigureSubscription(typeof(TConsumer), conf);

            //var consumerSetup = new ConsumerSetup()
            //{
            //    ConsumerType = typeof(TConsumer),
            //    MessageType = typeof(TMessage)
            //};

            //conf?.Invoke(consumerSetup);
            //Consumers.Add(consumerSetup);
        }


        public void FromConfiguration(string configurationEntry)
        {
            ConfigurationEntryName = configurationEntry;
        }


        public void AddConsumeAction<T>() where T: IConsumeAction<T>
        {
            ReceivedContextActions.CustomActions.Add(typeof(T));
        }

        public void AddPublishAction<T>() where T : IPublishAction<T>
        {
            PublishContextActions.CustomActions.Add(typeof(T));
        }

        protected internal abstract BusConfiguration GetConfiguration();
        protected internal abstract Factory GetFactory();
    }
}