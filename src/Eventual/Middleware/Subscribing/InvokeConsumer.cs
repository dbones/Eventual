namespace Eventual.Middleware.Subscribing
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fox.Middleware;
    using Microsoft.Extensions.DependencyInjection;

    public class InvokeConsumer<T> : IConsumeAction<T>
    {
        private readonly IServiceProvider _scope;

        public InvokeConsumer(IServiceProvider scope)
        {
            _scope = scope;
        }

        public async Task Execute(MessageReceivedContext<T> context, Next<MessageReceivedContext<T>> next)
        {
            var consumer = _scope.GetService<IConsumer<T>>();
            await consumer.Handle(context.Message);
            await next(context);
        }
    }
}