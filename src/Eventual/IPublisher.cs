namespace Eventual
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPublisher
    {
        Task Publish<T>(T body, CancellationToken cancellationToken);
        Task Publish<T>(Message<T> message, CancellationToken cancellationToken);
    }
}