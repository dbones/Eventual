namespace Eventual.Configuration
{
    public class RabbitMqHostSetup : HostSetup
    {
        public RabbitMqHostSetup() : base()
        {
            BusConfiguration = new RabbitMqBusConfiguration();
            Factory = new RabbitMqHostFactory();
        }

        public RabbitMqBusConfiguration BusConfiguration { get; set; }
        public RabbitMqHostFactory Factory { get; set; }

        protected override BusConfiguration GetConfiguration()
        {
            return BusConfiguration;
        }

        protected override Factory GetFactory()
        {
            return Factory;
        }
    }
}