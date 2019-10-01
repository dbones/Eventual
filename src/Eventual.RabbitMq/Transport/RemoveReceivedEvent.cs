namespace Eventual.Transport
{
    using System;

    internal class RemoveReceivedEvent : IDisposable
    {
        private readonly Action _removeHandler;

        public RemoveReceivedEvent(Action removeHandler)
        {
            _removeHandler = removeHandler;
        }

        public void Dispose()
        {
            _removeHandler();
        }
    }
}