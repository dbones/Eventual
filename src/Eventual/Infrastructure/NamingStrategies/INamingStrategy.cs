namespace Eventual.Infrastructure.NamingStrategies
{
    using System;

    public interface INamingStrategy
    {
        string GetQueueName(Type messageType, string serviceName);
        string GetTopicName(Type messageType, string serviceName);
    }
}