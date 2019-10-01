namespace Eventual.Middleware.Subscribing
{
    using System;
    using System.Threading.Tasks;
    using Fox.Middleware;
    using Microsoft.Extensions.DependencyInjection;

    public class ReceivedMessageMiddleware<T> : IMiddleware<MessageReceivedContext<T>>
    {

        private readonly Middleware<MessageReceivedContext<T>> _internalMiddleware;

        public ReceivedMessageMiddleware(ReceivedContextActions actions)
        {
            _internalMiddleware = new Middleware<MessageReceivedContext<T>>();

            _internalMiddleware.Add(MakeGeneric<T>(actions.ReadMessageFromQueueIntoContextAction));
            if (actions.LoggingAction != null) _internalMiddleware.Add(MakeGeneric<T>(actions.LoggingAction));
            if (actions.ApmAction != null) _internalMiddleware.Add(MakeGeneric<T>(actions.ApmAction));
            if (actions.DeadLetterAction != null) _internalMiddleware.Add(MakeGeneric<T>(actions.DeadLetterAction));

            foreach (var customAction in actions.CustomActions)
            {
                _internalMiddleware.Add(MakeGeneric<T>(customAction));
            }

            _internalMiddleware.Add(MakeGeneric<T>(actions.InvokeConsumerAction));

        }

        private static Type MakeGeneric<T>(Type type)
        {
            return type.MakeGenericType(typeof(T));
        }

        public Task Execute(IServiceProvider scope, MessageReceivedContext<T> context)
        {
            using (var requestScope = scope.CreateScope())
            {
                return _internalMiddleware.Execute(requestScope.ServiceProvider, context);
            }
        }
    }
}