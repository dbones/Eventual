namespace Eventual.Configuration
{
    using System;
    using System.Threading.Tasks;

    public class DefaultInitBus : IInitBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Func<IServiceProvider, Task> _startFunc;

        public DefaultInitBus(IServiceProvider serviceProvider, Func<IServiceProvider, Task> startFunc)
        {
            _serviceProvider = serviceProvider;
            _startFunc = startFunc;
        }

        public Task Start()
        {
            return _startFunc(_serviceProvider);
        }
    }
}