namespace Eventual.Configuration
{
    using System.Collections.Generic;

    public class RabbitMqBusConfiguration : BusConfiguration
    {
        /// <summary>
        /// name of the main exchange where all topics are routed through
        /// </summary>
        public string RoutingExchangeName { get; set; } = "routing.exchange";

        /// <summary>
        /// name of the exchange which failed messages are routed through
        /// </summary>
        public string FailedExchangeName { get; set; } = "routing.failed";

        /// <summary>
        /// name of the exchange which retry messages are routed back to the processing queue 
        /// </summary>
        public string RetryExchangeName { get; set; } = "routing.retry";

        /// <summary>
        /// name of the routing exchange for dead-letter timeouts/backoff
        /// </summary>
        public string DeadLetterExchangeName { get; set; } = "routing.deadletter";


        /// <summary>
        /// name of the failed messages queue, where messages are placed ready for retry to dead-lettering
        /// </summary>
        public string FailedQueueName { get; set; } = "queue.failed";
        
        /// <summary>
        /// name of the prefix for all the message delay/timeout/back-off queues
        /// </summary>
        public string RetryQueuePrefixName { get; set; } = "queue.retry";

        /// <summary>
        /// name of the deadletter queue
        /// </summary>
        public string DeadLetterQueueName { get; set; } = "queue.deadletter";


        /// <summary>
        /// how long a queue should stay before it is removed for inactivity
        /// </summary>
        public long ExpireQueueAfter { get; set; } = 3600000; //1 hour


        /// <summary>
        /// ordered list of retry back-off
        /// </summary>
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
        public string ConnectionString { get; set; } = "amqp://localhost/%2f";
    }
}