namespace Eventual.Configuration
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;

    public class BusHostedService : IHostedService
    {
        private readonly IInitBus _initBus;

        public BusHostedService(IInitBus initBus)
        {
            _initBus = initBus;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _initBus.Start();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}