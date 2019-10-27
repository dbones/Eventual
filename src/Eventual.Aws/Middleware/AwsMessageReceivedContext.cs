namespace Eventual.Middleware
{
    using Amazon.SQS.Model;

    public class AwsMessageReceivedContext<T> : MessageReceivedContext<T>
    {
        public Message Payload { get; set; }
    }
}