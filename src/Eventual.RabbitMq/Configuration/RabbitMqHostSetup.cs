namespace Eventual.Configuration
{
    public class RabbitMq : TransportSetup
    {
        public RabbitMq()
        {
            BusConfiguration = new RabbitMqBusConfiguration();
            Factory = new RabbitMqTransportFactory();
        }

        public RabbitMqBusConfiguration BusConfiguration { get; set; }
        public RabbitMqTransportFactory Factory { get; set; }

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