namespace Eventual.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class BusBuilder
    {
        private Func<IServiceProvider, HostSetup> _setupConfiguration;
        private HostSetup _setup;
        private Factory _factory;
        private volatile bool _started = false;

        public void Configure<T>(Action<T> conf) where T : HostSetup, new()
        {
            _setup = new T();
            _factory = _setup.GetFactory();
            
            conf((T)_setup);

            _setupConfiguration = sp =>
            {
                if (string.IsNullOrWhiteSpace(_setup.ConfigurationEntryName)) return _setup;
                var config = sp.GetService<IConfiguration>();
                config.GetSection(_setup.ConfigurationEntryName).Bind(_setup.GetConfiguration());

                return _setup;
            }; 
        }


        public void SetupContainer(IServiceCollection services)
        {
            _factory.RegisterServices(
                services, 
                _setup,
                _setupConfiguration,
                Start);
        }

        public Task Start(IServiceProvider serviceProvider)
        {
            if (_started) return Task.CompletedTask;
            _started = true;

            _setupConfiguration(serviceProvider);
            var tasks = new List<Task>();
            var subscriber = serviceProvider.GetService<ISubscriber>();
            foreach (var consumer in _setup.Consumers)
            {
                var task = subscriber.Subscribe(consumer);
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            return Task.CompletedTask;
        }
    }
}