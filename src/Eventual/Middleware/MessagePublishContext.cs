namespace Eventual.Middleware
{
    using System;
    using System.Threading.Tasks;

    public abstract class MessagePublishContext<T>
    {
        public Message<T> Message { get; set; }
        public Func<Task> PublishAction { get; set; }
    }
}