[![Release](https://github.com/rickpowell1311/onwrd/actions/workflows/release.yml/badge.svg)](https://github.com/rickpowell1311/onwrd/actions/workflows/release.yml)

# Onwrd

Adopt the outboxing pattern with Entity Framework Core. 

# Why Onwrd?

Reliably ensuring that onward processing happens after a business transaction has completed is not always that easy. 

Take an example of needing to send an email after a purchase has been made. Typically in this scenario, the details of the purchase will likely be stored in a database, and then an email sent afterwards. In a modern distributed architecture, this email is most likely to be sent as a result of a message being sent on an event bus, but what happens if the this *onward* service (the event bus) is not available? The purchase may have already been stored and the originating purchase request needs to finish processing in a timely manner... This is the kind of problem that the [outboxing pattern](https://microservices.io/patterns/data/transactional-outbox.html) helps solve.

Many .NET developers adopt [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) as their chosen ORM. As a result, implementing the outboxing pattern with EF core is a common use-case. Onwrd makes this implementation trivial.

# Quick start

Configure the DbContext to use onwrd by using the following overloaded method:

```
services.AddDbContext<MyContext>(
    (serviceProvider, builder) =>
    {
        // Your context configuration here...
        builder.UseSqlServer($"Server=.;Database=mydatabase;Trusted_Connection=True;");
    },
    onwrdConfig => 
    {
        // Outboxing configuration here
        onwrdConfig.UseOnwardProcessor<MyOnwardProcessor>();
    });
```

# Example usage

Define an event that an entity within the context can raise:

```
public class PurchaseMade
{
    public decimal Cost { get; set; }
}
```

... then raise the event from your domain, using the `EventRaiser` base class:
``` 
public class Purchase : EventRaiser
{
    public decimal Cost { get; private set; }

    public Purchase(decimal cost)
    {
        Cost = cost;

        RaiseEvent(new ItemPurchased { Cost = cost })
    }
}
```

... and allow the OnwardProcessor to forward the event in whatever way is required
```
public class BusOnwardProcessor : IOnwardProcessor
{
    private readonly IBus _bus;

    public BusOnwardProcessor(IBus bus)
    {
        _bus = bus;
    }    

    public async Task Process<T>(T @event, EventMetadata eventMetadata)
    {
        await _bus.SendAsync(@event);
    }
}
```

When you call `SaveChangesAsync()` after an entity has raised an event, the IOnwardProcessor will be invoked.
```
public class MakePurchase
{
    private readonly MyContext _context;

    public MakePurchase(MyContext context)
    {
        _context = context;
    }

    public async Task Execute()
    {
        context.Purchases.Add(new Purchase(30m));
        
        await context.SaveChangesAsync();
    }
}
```

# Supported DB Providers

Currently Onwrd supports the following DB providers:
- In-Memory 
- SQL Server
- PostgreSQL

Support is currently limited by the migration mechanism for maintaining the Onwrd schema. Other DB Providers may work out of the box, however they are not officially tested or supported at the moment.

Further DB Providers will be supported in future.

# Comparisons with other frameworks

## [CAP](https://github.com/dotnetcore/CAP)

CAP is a great framework which implements the outboxing pattern, and natively integrates with many other message buses. In this way, it's much more wide-reaching and complete than Onwrd. Having said this, there's a one major problem that CAP is yet to solve:
- Using transactions across multiple network trips locks database tables for larger periods of time. To implement outboxing, CAP first needs to start a transaction (one network trip), then commit some changes via the EF context (another network trip), and then commit the transaction (another network trip). This isn't very scalable when working with busy databases that cannot afford table locking for long periods

## [MassTransit](https://masstransit-project.com/)

MassTransit is yet another great library for managing onward processing of events on to an message bus. It is feature-rich, meaning it is powerful but also comes with a steep learning curve. MassTransit does not implement a persistent outbox yet, so Onwrd can be seen as a complimentary framework if there was a desire to use both Onwrd and MassTransit together.

## [NServiceBus](https://docs.particular.net/nservicebus/)

NServiceBus is one of the most well established frameworks for managing messaging between distributed systems. Like MassTransit, it's very feature-rich and comes with a steep learning curve. NServiceBus does implement a transactional outbox, however it suffers from the same issue of multiple network trips per transaction as the CAP framework does. It also has another major downside - it's not free to use.

# Commitments

Onwrd is built and maintained under an MIT license and always will be.


# Short term roadmap

- Extended documentation
- Add examples
- Support for integrating with logging providers (prioritising Serilog)
- Support for batched event onward processing
- Support for generic `IOnwardProcessor<T>` for filtered processing of event types
