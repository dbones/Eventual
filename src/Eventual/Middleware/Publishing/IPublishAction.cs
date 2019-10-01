namespace Eventual.Middleware.Publishing
{
    using Fox.Middleware;

    public interface IPublishAction<T> : IAction<MessagePublishContext<T>> { }
}