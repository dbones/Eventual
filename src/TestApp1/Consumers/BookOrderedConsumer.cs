namespace TestApp1.Consumers
{
    using System;
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
            //throw new Exception();
            _logger.LogInformation($"wa hey someone ordered : {message.Body.Name}");
            //throw new Exception("meh");
            return Task.CompletedTask;
        }
    }
}
