namespace Eventual.Transport
{
    using System.Threading.Tasks;
    using Middleware;

    public delegate Task Handle<T>(MessageReceivedContext<T> messageReceivedContext);
}