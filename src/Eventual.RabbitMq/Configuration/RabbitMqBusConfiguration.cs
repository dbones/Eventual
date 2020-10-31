namespace Eventual.Configuration
{
    using System;
    using System.Collections.Generic;

    public class RabbitMqBusConfiguration : BusConfiguration
    {
        public string RoutingExchangeName { get; set; } = "routing.exchange";
        public string FailedExchangeName { get; set; } = "routing.failed";
        public string RetryExchangeName { get; set; } = "routing.retry";
        public string DeadLetterExchangeName { get; set; } = "routing.deadletter";

        public string FailedQueueName { get; set; } = "queue.failed";
        public string RetryQueuePrefixName { get; set; } = "queue.retry";
        public string DeadLetterQueueName { get; set; } = "queue.deadletter";

        public long ExpireQueueAfter { get; set; } = 3600000; //1 hour

        public List<long> RetryBackOff = new List<long>
        {
            500,
            1000,
            1000,
            2000
        };

        /// <summary>
        /// "amqp://user:pass@hostName:port/vhost"
        /// </summary>
        public string ConnectionString { get; set; } = "amqp://localhost";
    }
}