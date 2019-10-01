namespace Eventual.Middleware.Publishing
{
    using System;
    using System.Threading.Tasks;
    using Fox.Middleware;

    public class MessagePublishContextMiddleware<T> : IMiddleware<MessagePublishContext<T>>
    {
        private readonly Middleware<MessagePublishContext<T>> _internalMiddleware;

        public MessagePublishContextMiddleware(PublishContextActions actions)
        {
            _internalMiddleware = new Middleware<MessagePublishContext<T>>();

            //if (actions.LoggingAction != null) _internalMiddleware.Add(actions.LoggingAction);
            if (actions.ApmAction != null) _internalMiddleware.Add(MakeGeneric<T>(actions.ApmAction));
            if (actions.PrepareMessageContextForPublish != null) _internalMiddleware.Add(MakeGeneric<T>(actions.PrepareMessageContextForPublish));

            foreach (var customAction in actions.CustomActions)
            {
                _internalMiddleware.Add(MakeGeneric<T>(customAction));
            }

            _internalMiddleware.Add(MakeGeneric<T>(actions.InvokePublisherAction));

        }

        private static Type MakeGeneric<T>(Type type)
        {
            return type.MakeGenericType(typeof(T));
        }

        public Task Execute(IServiceProvider scope, MessagePublishContext<T> context)
        {
            return _internalMiddleware.Execute(scope, context);
        }
    }
}