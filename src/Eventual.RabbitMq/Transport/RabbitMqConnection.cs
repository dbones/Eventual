namespace Eventual.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.Extensions.Logging;
    using Middleware;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using RabbitMQ.Client.Framing;

    public class RabbitMqConnection : IConnection
    {
        private readonly ILogger<RabbitMqConnection> _logger;
        private readonly RabbitMqBusConfiguration _busConfiguration;
        private RabbitMQ.Client.IConnection _connection;
        private IModel _channel;
        readonly ConnectionFactory _factory;
        
        private readonly ReaderWriterLockSlim _exchangeLock = new ReaderWriterLockSlim();
        private readonly HashSet<string> _exchanges = new HashSet<string>();
        
        public RabbitMqConnection(RabbitMqBusConfiguration busConfiguration, ILogger<RabbitMqConnection> logger)
        {
            _logger = logger;
            _busConfiguration = busConfiguration;

            //setup connection and channels
            _factory = new ConnectionFactory
            {
                Uri = new Uri(busConfiguration.ConnectionString),
                AutomaticRecoveryEnabled = true //wanted this to be explicit.
            };

            SetupGlobalExchanges();
        }

        private void SetupConnection()
        {
            if (_connection != null && _connection.IsOpen) return;

            _connection?.Dispose();

            _connection = _factory.CreateConnection();
            _connection.ConnectionShutdown += _connection_ConnectionShutdown;
            _connection.RecoverySucceeded += _connection_RecoverySucceeded;
            _logger.LogInformation("Connection is ready");

        }

        private void _connection_RecoverySucceeded(object sender, EventArgs e)
        {
            _logger.LogInformation("Connection Recovered");
        }

        private void _connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogWarning($"Lost connection:{Environment.NewLine}{e}");
        }


        private void SetupChannel()
        {
            if (_channel != null && _channel.IsOpen) return;

            _channel?.Dispose();
            SetupConnection();

            _channel = _connection.CreateModel();
            _logger.LogInformation("Channel is ready");
        }


        private void SetupGlobalExchanges()
        {
            SetupChannel();

            //setup global exchange
            _channel.ExchangeDeclare(_busConfiguration.RoutingExchangeName, "topic", true, false);

            //setup dead-letter
            _channel.ExchangeDeclare(_busConfiguration.DeadLetterExchangeName, "direct");
            QueueDeclareOk queue = _channel.QueueDeclare(_busConfiguration.DeadLetterQueueName, true, false, false);
            _channel.QueueBind(queue.QueueName, _busConfiguration.DeadLetterExchangeName, queue.QueueName);
        }

        public MessagePublishContext<T> CreatePublishContext<T>(string topicName, Message<T> message)
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

        public Task<IDisposable> RegisterConsumer<TMessage>(string topicName, string queueName, Handle<TMessage> handle)
        {
            var queue = EnsureQueue(queueName, topicName);
            var consumer = new EventingBasicConsumer(_channel);

            void ReceivedEvent(object sender, BasicDeliverEventArgs args)
            {
                var context = new RabbitMqMessageReceivedContext<TMessage>
                {
                    Payload = args,
                    Acknowledge = () => _channel.BasicAck(args.DeliveryTag, true),
                    NotAcknowledge = () => _channel.BasicNack(args.DeliveryTag, true, true),
                    Reject = () => _channel.BasicReject(args.DeliveryTag, false)
                };

                var task = handle(context);
                task.Wait();
            }

            consumer.Received += ReceivedEvent;

            _channel.BasicConsume(
                queue: queue.QueueName,
                autoAck: false,
                consumer: consumer);

            Action remove = () => consumer.Received -= ReceivedEvent;
            return Task.FromResult((IDisposable)new RemoveReceivedEvent(remove, _logger));
        }

        private string EnsureExchange(string topicName)
        {
            try
            {
                _exchangeLock.EnterReadLock();
                if (_exchanges.Contains(topicName)) return topicName;
            }
            finally
            {
                _exchangeLock.ExitReadLock();
            }

            try
            {
                _exchangeLock.EnterWriteLock();
                _channel.ExchangeDeclare(topicName, "fanout", true, false);
                _channel.ExchangeBind(topicName, _busConfiguration.RoutingExchangeName, topicName);
                _exchanges.Add(topicName);
                return topicName;
            }
            finally
            {
                _exchangeLock.ExitWriteLock();
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
                exclusive: false,
                autoDelete: false,
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