namespace Eventual.Middleware.Subscribing
{
    using System;
    using System.Threading.Tasks;
    using Fox.Middleware;

    public class MessageAck<T> : IConsumeAction<T>
    {
        public async Task Execute(MessageReceivedContext<T> context, Next<MessageReceivedContext<T>> next)
        {
            try
            {
                await next(context);
                context.Acknowledge();
            }
            catch (Exception)
            {
                context.Reject();
                throw;
            }
        }
    }
}