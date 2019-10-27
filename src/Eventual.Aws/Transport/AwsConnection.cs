namespace Eventual.Transport
{
    using System;
    using System.Collections.Generic;
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
        private readonly PipelineBroker _pipelineBroker;
        readonly AmazonSQSClient _sqsClient;
        readonly AmazonSimpleNotificationServiceClient _snsClient;

        private readonly Dictionary<string, string> _snsTopics = new Dictionary<string, string>();
        private readonly ReaderWriterLockSlim _topicLock = new ReaderWriterLockSlim();
        

        public AwsConnection(
            WorkerPool workerPool,
            PipelineBroker pipelineBroker)
        {
            _workerPool = workerPool;
            _pipelineBroker = pipelineBroker;
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
            var topicTask = EnsureExchange(topicName);
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
            var queue = await this.EnsureQueue(queueName, topicName);

            _pipelineBroker.AddRoute(topicName, o =>
            {
                var received = new AwsMessageReceivedContext<T> {Payload = (Message) o};
                return handle(received);
            });

            void BackgroundPolling(CancellationToken token)
            {
                while (true)//(!cancellationToken.IsCancellationRequested)
                {
                    var request = new ReceiveMessageRequest
                    {
                        AttributeNames = {"All"},
                        MaxNumberOfMessages = 10,
                        MessageAttributeNames = {"All"},
                        QueueUrl = queue,
                        WaitTimeSeconds = 20
                    };

                    var responseTask = _sqsClient.ReceiveMessageAsync(request, token);
                    responseTask.Wait(token);

                    var tasks = new List<Task>();

                    foreach (var message in responseTask.Result.Messages)
                    {
                        var topic = message.Attributes["topic"];
                        var task = _pipelineBroker.Dispatch(topic, message);
                        tasks.Add(task);
                    }

                    Task.WaitAll(tasks.ToArray());
                    break;
                }
            }

            var work = new Work(BackgroundPolling);
            _workerPool.Schedule(work);

            //todo: fix this.
            return Task.FromResult(work);
        }


        private async Task<string> EnsureExchange(string topicName)
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
            var arn = await EnsureExchange(topicName);

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
}
