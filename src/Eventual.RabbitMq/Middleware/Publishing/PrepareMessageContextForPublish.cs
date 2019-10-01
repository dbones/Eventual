namespace Eventual.Middleware.Publishing
{
    using System.Text;
    using System.Threading.Tasks;
    using Fox.Middleware;
    using Infrastructure.Serialization;

    public class PrepareMessageContextForPublish<T> : IPublishAction<T>
    {
        private readonly ISerializer _serializer;

        public PrepareMessageContextForPublish(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public Task Execute(MessagePublishContext<T> context, Next<MessagePublishContext<T>> next)
        {
            var rbc = (RabbitMqMessagePublishContext<T>) context;
            var payload = _serializer.Serialize(context.Message.Body);
            var encoded = Encoding.UTF8.GetBytes(payload);

            rbc.Body = encoded;
            rbc.Properties.CorrelationId = context.Message.CorrelationId;
            rbc.Properties.MessageId = context.Message.Id;
            //rbc.Properties.ContentEncoding = ""
            return next(context);
        }
    }
}