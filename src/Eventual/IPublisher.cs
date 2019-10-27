namespace Eventual
{
    using System.Threading.Tasks;

    public interface IPublisher
    {
        Task Publish<T>(T body);
        Task Publish<T>(Message<T> message);
    }
}