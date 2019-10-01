namespace Eventual.Middleware
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fox.Middleware;
    using Microsoft.Extensions.DependencyInjection;
    using Publishing;
    using Subscribing;

    public interface IDispatcher
    {
        Task ProcessMessage<T>(MessageReceivedContext<T> receivedContext, CancellationToken cancellationToken);
        Task ProcessMessage<T>(MessagePublishContext<T> publishContext, CancellationToken cancellationToken);
    }

    public class DefaultDispatcher : IDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task ProcessMessage<T>(MessageReceivedContext<T> receivedContext, CancellationToken cancellationToken)
        {
            var middleware = _serviceProvider.GetService<ReceivedMessageMiddleware<T>>();
            return middleware.Execute(_serviceProvider, receivedContext);
        }

        public Task ProcessMessage<T>(MessagePublishContext<T> publishContext, CancellationToken cancellationToken)
        {
            var middleware = _serviceProvider.GetService<MessagePublishContextMiddleware<T>>();
            return middleware.Execute(_serviceProvider, publishContext);
        }
    }

    
}