# Eventual => dotnet pub-sub messaging

this is a wip project to see how we can support the publish / subscribe pattern with multiple brokers

The idea in a true SOA world all the services will communicate with messages, making the platform to become **Eventual**ly consistent.


**features**

- multi-broker support WIP
- full pub / sub implementation
- deadletter queue setup
- direct dotnet core dependency injection support
- opinionated broker setup
- middleware support
- async pattern WIP

## Example RabbitMq Setup

```
public static void Main(string[] args)
{
    var host = 
      Host
        .CreateDefaultBuilder(args)
        .UseEventual<RabbitMqHostSetup>((setup) =>
        {
            //read from the IConfiguration
            setup.FromConfiguration("RabbitMq");

            //override any settings
            setup.BusConfiguration.ConnectionString = "amqp://172.22.101.111/%2f";

            //setup any subscriptions (there are a few overloads)
            setup.ConfigureSubscription<BookOrderedConsumer, BookOrdered>();
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });

    host.Build().Run();
}
```

## Publish

```
private readonly IBus _bus;

public EpicController(IBus bus)
{
    _bus = bus;
}

[HttpPost]
public async Task Get()
{
    // do awesome things

    //publish events
    await _bus.Publish(new BookOrdered 
    {
        Author = "dave", 
        Name = "events with eventual"
    }, CancellationToken.None);

```

## Subscribe and consume

```
public class BookOrderedConsumer : IConsumer<BookOrdered>
{
    private readonly ILogger<BookOrderedConsumer> _logger;

    public BookOrderedConsumer(ILogger<BookOrderedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Handle(Message<BookOrdered> message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"wa hey someone ordered : {message.Body.Name}");

        return Task.CompletedTask;
    }
}
```

**really awesome notes:**

- https://derickbailey.com/2015/09/02/rabbitmq-best-practices-for-designing-exchanges-queues-and-bindings/
- https://jack-vanlightly.com/blog/2017/12/5/rabbitmq-vs-kafka-part-2-rabbitmq-messaging-patterns-and-topologies
- http://dbones.github.io/2020/01/retries-and-deadletters/
