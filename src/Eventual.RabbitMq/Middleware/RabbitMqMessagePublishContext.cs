namespace Eventual.Middleware
{
    using RabbitMQ.Client.Framing;

    public class RabbitMqMessagePublishContext<T> : MessagePublishContext<T>
    {
        public byte[] Body { get; set; }
        public BasicProperties Properties { get; set; }
    }
}