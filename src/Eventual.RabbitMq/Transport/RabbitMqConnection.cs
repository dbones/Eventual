namespace Eventual.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Middleware;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using RabbitMQ.Client.Framing;

    public class RabbitMqConnection : IDisposable, IConnection
    {
        private readonly RabbitMqBusConfiguration _busConfiguration;
        private readonly RabbitMQ.Client.IConnection _connection;
        private readonly IModel _channel;

        private ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();
        private HashSet<string> _exchanges = new HashSet<string>();

        public RabbitMqConnection(RabbitMqBusConfiguration busConfiguration)
        {
            _busConfiguration = busConfiguration;


            //setup connection and channels
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_busConfiguration.ConnectionString),
                AutomaticRecoveryEnabled = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            //setup global exchange
            _channel.ExchangeDeclare(_busConfiguration.RoutingExchangeName, "topic", true);

            //setup dead-letter
            _channel.ExchangeDeclare(_busConfiguration.DeadLetterExchangeName, "direct");
            QueueDeclareOk queue = _channel.QueueDeclare(_busConfiguration.DeadLetterQueueName, true);
            _channel.QueueBind(queue.QueueName, _busConfiguration.DeadLetterExchangeName, queue.QueueName);
        }

        public MessagePublishContext<T> CreatePublishContext<T>(string topicName, Message<T> message, CancellationToken cancellationToken)
        {
            var topic = EnsureExchange(topicName);

            var properties = new BasicProperties
            {
                DeliveryMode = 2, //topic
                AppId = _busConfiguration.ServiceName
            };

            var context = new RabbitMqMessagePublishContext<T>()
            {
                Message = message,
                Properties = properties,
            };

            context.PublishAction = () =>
            {
                _channel.BasicPublish(_busConfiguration.RoutingExchangeName, topic, true, properties, context.Body);
                return Task.CompletedTask;
            };

            return context;
        }


        public Task<IDisposable> RegisterConsumer<TMessage>(string topicName, string queueName, Handle<TMessage> handle, CancellationToken cancellationToken) 
        {
            var queue = EnsureQueue(queueName, topicName);
            var consumer = new EventingBasicConsumer(_channel);

            void ReceivedEvent(object sender, BasicDeliverEventArgs args)
            {
                var context = new RabbitMqMessageReceivedContext<TMessage>
                {
                    Payload = args,
                    Acknowledge = () => _channel.BasicAck(args.DeliveryTag, false),
                    NotAcknowledge = () => _channel.BasicNack(args.DeliveryTag, false, true),
                    Reject = () => _channel.BasicReject(args.DeliveryTag, false)
                };

                var task = handle(context);
                task.Wait(cancellationToken);
            }

            consumer.Received += ReceivedEvent;

            _channel.BasicConsume(
                queue: queue.QueueName,
                autoAck: true,
                consumer: consumer);

            Action remove = () => consumer.Received -= ReceivedEvent;
            return Task.FromResult((IDisposable)new RemoveReceivedEvent(remove));
        }


        private string EnsureExchange(string topicName)
        {
            try
            {
                _lockSlim.EnterReadLock();
                if (_exchanges.Contains(topicName)) return topicName;
            }
            finally
            {
                _lockSlim.ExitReadLock();                
            }

            try
            {
                _lockSlim.EnterWriteLock();
                _channel.ExchangeDeclare(topicName, "fanout", true, false);
                _channel.ExchangeBind(topicName, _busConfiguration.RoutingExchangeName, topicName);
                _exchanges.Add(topicName);
                return topicName;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
            
        }

        private QueueDeclareOk EnsureQueue(string queueName, string topicName)
        {
            EnsureExchange(topicName);

            var properties = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", _busConfiguration.DeadLetterExchangeName }
            };

            var queue = _channel.QueueDeclare(
                queue: queueName, 
                durable: true, 
                arguments: properties);

            _channel.QueueBind(queue.QueueName, topicName, topicName);

            return queue;
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _channel?.Dispose();
        }
    }
}