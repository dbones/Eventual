namespace Eventual.Middleware.Subscribing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fox.Middleware;
    using Infrastructure.Serialization;

    public class ReadMessageFromQueueIntoContext<T> : IConsumeAction<T>
    {
        private readonly ISerializer _serializer;

        public ReadMessageFromQueueIntoContext(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public async Task Execute(MessageReceivedContext<T> context, Next<MessageReceivedContext<T>> next)
        {
            var rbc = (RabbitMqMessageReceivedContext<T>)context;
            var properties = rbc.Payload.BasicProperties;
            var headers = properties?.Headers?.ToDictionary(key => key.Key, pair => pair.Value.ToString()) 
                          ?? new Dictionary<string, string>();

            var content = Encoding.UTF8.GetString(rbc.Payload.Body);
            var deserialized = _serializer.Deserialize<T>(content);

            var msg = new Message<T>()
            {
                Metadata = headers,
                Body = deserialized,
                CorrelationId = properties.CorrelationId,
                Id = properties.MessageId,
                DateTime = ConvertFromUnixTimestamp(properties.Timestamp.UnixTime)
            };

            context.Message = msg;
            await next(context);
        }

        static DateTime ConvertFromUnixTimestamp(long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }
    }
}