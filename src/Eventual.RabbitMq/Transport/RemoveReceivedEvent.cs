namespace Eventual.Transport
{
    using System;
    using Microsoft.Extensions.Logging;

    internal class RemoveReceivedEvent : IDisposable
    {
        private readonly Action _removeHandler;
        private readonly ILogger _logger;

        public RemoveReceivedEvent(Action removeHandler, ILogger logger)
        {
            _removeHandler = removeHandler;
            _logger = logger;
        }

        public void Dispose()
        {
            _logger.LogError("removing subscription");
            _removeHandler();
        }
    }
}