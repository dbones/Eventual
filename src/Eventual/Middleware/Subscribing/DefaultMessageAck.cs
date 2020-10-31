namespace Eventual.Middleware.Subscribing
{
    using System;
    using System.Threading.Tasks;
    using Fox.Middleware;

    public class DefaultMessageAck<T> : IConsumeAction<T>
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
                context.NotAcknowledge();
                throw;
            }
        }
    }
}