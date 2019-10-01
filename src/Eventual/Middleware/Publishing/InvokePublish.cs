namespace Eventual.Middleware.Publishing
{
    using System.Threading.Tasks;
    using Fox.Middleware;

    public class InvokePublish<T> : IPublishAction<T>
    {
        public async Task Execute(MessagePublishContext<T> context, Next<MessagePublishContext<T>> next)
        {
            await context.PublishAction();
            await next(context);
        }
    }
}