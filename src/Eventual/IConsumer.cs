namespace Eventual
{
    using System.Threading.Tasks;

    public interface IConsumer { }

    public interface IConsumer<T> : IConsumer
    {
        Task Handle(Message<T> message);
    }
}