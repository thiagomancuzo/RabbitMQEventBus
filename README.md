# RabbitMQEventBus

A library that implements the concept of EventBus using the RabbitMQ message broker.

## Known types

### EventHandler

A EventHandler is the artfact responsible for handle the events emited by any publisher. It is a contract (aka Interface) that have one method called Handle.
Every EventHandler handles a generic type, that is the Event itself. This type is also received in the Handle method.

```csharp
internal class StringHandler : IEventHandler<SomeEventClass>
{
    public Task Handle(SomeEventClass @event)
    {
       // do something
        return Task.CompletedTask;
    }
}
```

## Usage

Creating a new EventBus instance:

```csharp
  var configuration = new RabbitMQEventBusConfiguration(
          connectionUri: new Uri("amqp://guest:guest@localhost:8081"),
          exchangeName: "PurgeTest", 
          queueName: "PurgeTestQueue",
          prefetchCount: 10
          );

  // there are some optional configurations below:
  
  configuration.UseToLog((s) => Console.WriteLine(s)); // due the handler is an async process, the errors thrown by any network issue or handler exception, this delegate will be called, so you will be able to write your own logger. Also, you may consider to use the ILogger contract and put it on a overload of this method.
  
  configuration.UseToResolveTypes((t) => Activator.CreateInstance(t)); // here you may use your own DI container
  
  configuration.CreateDeadLetterExchangeConfiguration(TimeSpan.FromMinutes(1), "retry", true); // with this, you are going to configure a retry policy with RabbitMQ's Dead Letter Exchange, that will retry in a minute.
  
  configuration.CreateDefaultTls12SecurityConfiguration("mydomain.com"); // for security, it supports TLS 1.2 to communicate with RabbitMQ.

  var provider = new RabbitMQEventBusProvider(configuration);
  
  var eventBus = (RabbitMQEventBus)provider.Create();
```

Publishing something

```csharp
  var someEventInstance = new SomeEventClass();
  eventBus.Publish<SomeEventClass>(someEventInstance);
```

Subscribing handlers

```csharp
  eventBus.Subscribe<SomeEventClass, SomeEventClassHandler>();
```

Unsubscribing handlers
```csharp
  eventBus.Unsubscribe<SomeEventClass, SomeEventClassHandler>();
```
