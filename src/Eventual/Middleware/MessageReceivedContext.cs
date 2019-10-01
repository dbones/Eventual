namespace Eventual.Middleware
{
    using System;

    public abstract class MessageReceivedContext<T>
    {
        public Message<T> Message { get; set; }
        public Action Acknowledge { get; set; }
        public Action NotAcknowledge { get; set; }
        public Action Reject { get; set; }
    }
}