namespace Eventual
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPublisher
    {
        Task Publish<T>(T body, CancellationToken cancellationToken);
        Task Publish<T>(Message<T> message, CancellationToken cancellationToken);
    }

    public static class PublisherExtensions
    {
        public static Task Publish<T>(this IPublisher publisher, T body)
        {
            return publisher.Publish(body, CancellationToken.None);
        }

        public static Task Publish<T>(this IPublisher publisher, Message<T> message)
        {
            return publisher.Publish(message, CancellationToken.None);
        }
    }
}