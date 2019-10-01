namespace Eventual.Configuration
{
    using System;

    public class ConsumerSetup
    {
        public string QueueName { get; set; } = "";
        public string Topic { get; set; } = "";
        public Type MessageType { get; set; }
        public Type ConsumerType { get; set; }
    }
}