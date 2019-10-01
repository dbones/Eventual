namespace Eventual.Infrastructure.NamingStrategies
{
    using System;
    using Configuration;

    public class RabbitMqNamingStrategy : INamingStrategy
    {
        private readonly BusConfiguration _configuration;

        public RabbitMqNamingStrategy(BusConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetQueueName(Type messageType, string serviceName)
        {
            return $"{_configuration.ServiceName}.{messageType.FullName}";
        }

        public string GetTopicName(Type messageType, string serviceName)
        {
            return messageType.FullName;
        }
    }
}