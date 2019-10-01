namespace Eventual
{
    using System;
    using System.Threading.Tasks;
    using Configuration;

    public interface ISubscriber
    {
        Task<IDisposable> Subscribe(ConsumerSetup setup);
        Task<IDisposable> Subscribe<T>();
    }
}