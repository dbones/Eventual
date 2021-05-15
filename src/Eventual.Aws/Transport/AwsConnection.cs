namespace Eventual.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.SimpleNotificationService;
    using Amazon.SimpleNotificationService.Model;
    using Amazon.SQS;
    using Amazon.SQS.Model;
    using Middleware;

    public class AwsConnection : IDisposable, IConnection
    {
        private readonly WorkerPool _workerPool;
        readonly AmazonSQSClient _sqsClient;
        readonly AmazonSimpleNotificationServiceClient _snsClient;

        private readonly Dictionary<string, string> _snsTopics = new Dictionary<string, string>();
        private readonly ReaderWriterLockSlim _topicLock = new ReaderWriterLockSlim();
        

        public AwsConnection(
            WorkerPool workerPool)
        {
            _workerPool = workerPool;
            var sqsConfig = new AmazonSQSConfig();

            sqsConfig.ServiceURL = "http://sqs.us-west-2.amazonaws.com";

            _sqsClient = new AmazonSQSClient(sqsConfig);
            _snsClient = new AmazonSimpleNotificationServiceClient();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public MessagePublishContext<T> CreatePublishContext<T>(string topicName, Message<T> message)
        {
            var topicTask = EnsureTopic(topicName);
            topicTask.Wait();
            var url = topicTask.Result;

            var m = new AwsMessagePublishContext<T>();

            m.TopicArn = url;


            var ms = new PublishRequest()
            {
                
            };

            return null;
            //_snsClient.PublishAsync()
        }

        public async Task<IDisposable> RegisterConsumer<T>(string topicName, string queueName, Handle<T> handle)
        {
            var queue = await EnsureQueue(queueName, topicName);


            var subscription = new Subscription(_sqsClient, queue);

            async Task ReceivedEvent(Message message)
            {
                var context = new AwsMessageReceivedContext<T>()
                {
                    Payload = message
                };

                await handle(context);
            }

            subscription.Handler = ReceivedEvent;


            //todo: fix this.
            return Task.FromResult(subscription);
        }


        private async Task<string> EnsureTopic(string topicName)
        {
            try
            {
                _topicLock.EnterReadLock();
                if (_snsTopics.TryGetValue(topicName, out var url))
                {
                    return url;
                }
            }
            finally
            {
                _topicLock.ExitReadLock();
            }

            try
            {
                _topicLock.EnterWriteLock();
                var snsCreateResponse = await _snsClient.CreateTopicAsync(topicName);
                var arn = snsCreateResponse.TopicArn;
                _snsTopics.Add(topicName, arn);
                return arn;
            }
            finally
            {
                _topicLock.ExitWriteLock();
            }
        }

        private async Task<string> EnsureQueue(string queueName, string topicName)
        {
            var arn = await EnsureTopic(topicName);

            var cqr = new CreateQueueRequest()
            {
                QueueName = queueName
            };
            var queueResponse = await _sqsClient.CreateQueueAsync(cqr);

            //_snsClient.ListSubscriptionsAsync(new SubscribeRequest())

            var subResponse = await _snsClient.SubscribeQueueAsync(arn, _sqsClient, queueResponse.QueueUrl);
            return queueResponse.QueueUrl;
        }


    }



    public class Subscription : IDisposable
    {
        private readonly AmazonSQSClient _client;
        private readonly string _queueUrl;
        private volatile bool _isDisposing = false;

        public Subscription(AmazonSQSClient client, string queueUrl)
        {
            _client = client;
            _queueUrl = queueUrl;
        }

        public Func<Message, Task> Handler { get; set; }

        private async void BackgroundPolling(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !_isDisposing)
            {
                var request = new ReceiveMessageRequest
                {
                    AttributeNames = { "All" },
                    MaxNumberOfMessages = 5,
                    MessageAttributeNames = { "All" },
                    QueueUrl = _queueUrl,
                    WaitTimeSeconds = 20
                };

                var responseTask = await _client.ReceiveMessageAsync(request, token);

                Task.WaitAll(
                    responseTask
                        .Messages
                        .Select(message => Handler(message))
                        .ToArray());
            }
        }

        public void Dispose()
        {
            _isDisposing = true;
        }
    }

}
