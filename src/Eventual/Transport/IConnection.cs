namespace Eventual.Transport
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Middleware;

    /// <summary>
    /// the connection logic to a broker/service-bus
    ///
    /// it should deal with how queues are setup
    ///
    /// and it needs to handle network connections (including re-connection)
    /// 
    /// each transport will provide their own connection
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// creates the publish context for publishing the message to a bus
        /// </summary>
        /// <typeparam name="T">the message type</typeparam>
        /// <param name="topicName">the topic name</param>
        /// <param name="message">the full message with headers</param>
        /// <returns>context which can be passed into the publish pipeline</returns>
        MessagePublishContext<T> CreatePublishContext<T>(string topicName, Message<T> message);
        
        /// <summary>
        /// subscribes a consumer with the bus
        /// </summary>
        /// <typeparam name="T">the message</typeparam>
        /// <param name="topicName">the topic</param>
        /// <param name="queueName">the queue which the consumer will process from</param>
        /// <param name="handle">the pipeline which will be called</param>
        /// <returns>a disposable to allow the subscription to be cleared up</returns>
        Task<IDisposable> RegisterConsumer<T>(string topicName, string queueName, Handle<T> handle);
    }
}