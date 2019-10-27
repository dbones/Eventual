namespace Eventual.Infrastructure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;

    public class DefaultBus : IBus
    {
        private readonly ISubscriber _subscriber;
        private readonly IPublisher _publisher;

        public DefaultBus(
            ISubscriber subscriber, 
            IPublisher publisher
        )
        {
            _subscriber = subscriber;
            _publisher = publisher;
        }

        public Task<IDisposable> Subscribe(ConsumerSetup setup)
        {
            return _subscriber.Subscribe(setup);
        }

        public Task<IDisposable> Subscribe<T>()
        {
            return _subscriber.Subscribe<T>();
        }

        public Task Publish<T>(T body)
        {
            return _publisher.Publish(body);
        }

        public Task Publish<T>(Message<T> message)
        {
            return _publisher.Publish(message);
        }
    }
}