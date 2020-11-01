namespace Eventual.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class BusBuilder
    {
        private Func<IServiceProvider, Setup> _setupConfiguration;
        private Setup _setup;
        private Factory _factory;
        private volatile bool _started = false;

        public void Configure(Action<Setup> conf)
        {
            _setup = new Setup();
            conf(_setup);
            _factory = _setup.Transport.GetFactory();
            
            

            _setupConfiguration = sp =>
            {
                if (string.IsNullOrWhiteSpace(_setup.Transport.ConfigurationEntryName)) return _setup;
                var config = sp.GetService<IConfiguration>();
                config.GetSection(_setup.Transport.ConfigurationEntryName).Bind(_setup.Transport.GetConfiguration());

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