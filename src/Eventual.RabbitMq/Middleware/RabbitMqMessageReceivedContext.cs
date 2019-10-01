namespace Eventual.Middleware
{
    using RabbitMQ.Client.Events;

    public class RabbitMqMessageReceivedContext<T> : MessageReceivedContext<T>
    {
        public BasicDeliverEventArgs Payload { get; set; }
    }
}