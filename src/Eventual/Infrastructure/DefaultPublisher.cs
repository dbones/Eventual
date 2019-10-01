namespace Eventual.Infrastructure
{
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Middleware;
    using NamingStrategies;
    using Transport;

    public class DefaultPublisher : IPublisher
    {
        private readonly IConnection _connection;
        private readonly INamingStrategy _namingStrategy;
        private readonly IDispatcher _dispatcher;
        private readonly BusConfiguration _configuration;

        public DefaultPublisher(
            IConnection connection,
            INamingStrategy namingStrategy, 
            IDispatcher dispatcher,
            BusConfiguration configuration)
        {
            _connection = connection;
            _namingStrategy = namingStrategy;
            _dispatcher = dispatcher;
            _configuration = configuration;
        }

        public Task Publish<T>(T body, CancellationToken cancellationToken)
        {
            var completeMessage = new Message<T>(body);
            return Publish(completeMessage, cancellationToken);
        }

        public Task Publish<T>(Message<T> message, CancellationToken cancellationToken)
        {
            var queueName = _namingStrategy.GetTopicName(typeof(T), _configuration.ServiceName);
            var context = _connection.CreatePublishContext(queueName, message, cancellationToken);
            
            return _dispatcher.ProcessMessage(context, cancellationToken);
        }
    }
}