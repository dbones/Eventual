namespace Eventual.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.Extensions.Hosting;

    //public class BusHostedService : IHostedService
    //{
    //    private readonly IEnumerable<ConsumerSetup> _candidateConsumers;
    //    private readonly ISubscriber _subscriber;


    //    public BusHostedService(
    //        IEnumerable<ConsumerSetup> candidateConsumers,
    //        ISubscriber subscriber
    //    )
    //    {
    //        _candidateConsumers = candidateConsumers;
    //        _subscriber = subscriber;
    //    }


    //    public Task StartAsync(CancellationToken cancellationToken)
    //    {
    //        Task.WaitAll(
    //            _candidateConsumers
    //                .Select(candidateConsumer => _subscriber.Subscribe(candidateConsumer))
    //                .Cast<Task>()
    //                .ToArray());
    //        return Task.CompletedTask;  
    //    }

    //    public Task StopAsync(CancellationToken cancellationToken)
    //    {
    //        return Task.CompletedTask;
    //    }
    //}
}