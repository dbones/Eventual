namespace Eventual.Middleware.Subscribing
{
    using System;
    using System.Threading.Tasks;
    using Fox.Middleware;
    using Microsoft.Extensions.Logging;

    public class LogReceivedMessage<T> : IConsumeAction<T>
    {
        private readonly ILogger<T> _logger;

        public LogReceivedMessage(ILogger<T> logger)
        {
            _logger = logger;
        }

        public async Task Execute(MessageReceivedContext<T> context, Next<MessageReceivedContext<T>> next)
        {
            var messageType = typeof(T).FullName;
            using (var scope = _logger.BeginScope(context.Message.Id))
            {
                _logger.LogInformation($"Receiving message {messageType}");
                try
                {
                    await next(context);
                    _logger.LogInformation($"Received message {messageType}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to process {messageType}");
                    throw;
                }
            }
        }
    }
}