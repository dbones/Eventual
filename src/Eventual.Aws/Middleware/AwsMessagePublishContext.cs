namespace Eventual.Middleware
{
    public class AwsMessagePublishContext<T> : MessagePublishContext<T>
    {
        public string Body { get; set; }
        public string TopicArn { get; set; }
    }
}