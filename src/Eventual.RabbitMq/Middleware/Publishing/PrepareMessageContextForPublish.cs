namespace Eventual.Middleware.Publishing
{
    using System.Text;
    using System.Threading.Tasks;
    using Configuration;
    using Fox.Middleware;
    using Infrastructure.Serialization;

    public class PrepareMessageContextForPublish<T> : IPublishAction<T>
    {
        private readonly ISerializer _serializer;
        private readonly RabbitMqBusConfiguration _busConfiguration;

        public PrepareMessageContextForPublish(
            ISerializer serializer, 
            RabbitMqBusConfiguration busConfiguration)
        {
            _serializer = serializer;
            _busConfiguration = busConfiguration;
        }

        public Task Execute(MessagePublishContext<T> context, Next<MessagePublishContext<T>> next)
        {
            var rbc = (RabbitMqMessagePublishContext<T>) context;
            
            var payload = _serializer.Serialize(context.Message.Body);
            var encoded = Encoding.UTF8.GetBytes(payload);
            rbc.Body = encoded;

            var properties = rbc.Properties;
            properties.AppId = _busConfiguration.ServiceName;
            properties.DeliveryMode = 2; //topic
            properties.CorrelationId = context.Message.CorrelationId;
            properties.MessageId = context.Message.Id;
            //properties.ContentEncoding = ""
            properties.Headers.Add("count", 0);
            properties.Headers.Add("retry.in", 0);

            for (var i = 0; i < _busConfiguration.RetryBackOff.Count; i++)
            {
                properties.Headers.Add($"retry.{i + 1}.after", _busConfiguration.RetryBackOff[i]);
            }

            return next(context);
        }
    }
}