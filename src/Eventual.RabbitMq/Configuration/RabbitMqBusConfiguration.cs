namespace Eventual.Configuration
{
    using System;

    public class RabbitMqBusConfiguration : BusConfiguration
    {
        public string RoutingExchangeName { get; set; } = "routing.exchange";
        public string DeadLetterExchangeName { get; set; } = "routing.deadletter";
        public string DeadLetterQueueName { get; set; } = "deadletter.queue";

        /// <summary>
        /// "amqp://user:pass@hostName:port/vhost"
        /// </summary>
        public string ConnectionString { get; set; } = "amqp://localhost";
    }
}