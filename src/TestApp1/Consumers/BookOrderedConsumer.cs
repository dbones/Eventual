namespace TestApp1.Consumers
{
    using System.Threading.Tasks;
    using Events;
    using Eventual;
    using Microsoft.Extensions.Logging;

    public class BookOrderedConsumer : IConsumer<BookOrdered>
    {
        private readonly ILogger<BookOrderedConsumer> _logger;

        public BookOrderedConsumer(ILogger<BookOrderedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Handle(Message<BookOrdered> message)
        {
            _logger.LogInformation($"wa hey someone ordered : {message.Body.Name}");
            return Task.CompletedTask;
        }
    }
}
