namespace Eventual.Middleware.Subscribing
{
    using Fox.Middleware;

    public interface IConsumeAction<T> : IAction<MessageReceivedContext<T>> {}
}