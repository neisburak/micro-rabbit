using System.Text;
using System.Text.Json;
using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroRabbit.Infra.Bus;

public sealed class RabbitMQBus : IEventBus
{
    private readonly IMediator _mediator;
    private readonly Dictionary<string, List<Type>> _handlers;
    private readonly List<Type> _eventTypes;

    public RabbitMQBus(IMediator mediator)
    {
        _mediator = mediator;
        _handlers = new Dictionary<string, List<Type>>();
        _eventTypes = new List<Type>();
    }

    public Task SendCommand<TCommand>(TCommand command) where TCommand : Command
    {
        return _mediator.Send(command);
    }

    public void Publish<TEvent>(TEvent @event) where TEvent : Event
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var eventName = @event.GetType().Name;

        channel.QueueDeclare(eventName, false, false, false);

        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(string.Empty, eventName, null, body);
    }

    public void Subscribe<TEvent, THandler>() where TEvent : Event where THandler : IEventHandler<TEvent>
    {
        var eventName = typeof(TEvent).Name;
        var handlerType = typeof(THandler);

        if (!_eventTypes.Contains(typeof(TEvent))) _eventTypes.Add(typeof(TEvent));

        if (!_handlers.ContainsKey(eventName)) _handlers.Add(eventName, new List<Type>());

        if (_handlers[eventName].Any(a => a.GetType() == handlerType))
        {
            throw new ArgumentException($"Handler type {handlerType.Name} already is registered for {eventName}", nameof(handlerType));
        }

        _handlers[eventName].Add(handlerType);

        StartBasicConsume<TEvent>();
    }

    private void StartBasicConsume<TEvent>() where TEvent : Event
    {
        var factory = new ConnectionFactory() { HostName = "localhost", DispatchConsumersAsync = true };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var eventName = typeof(TEvent).Name;

        channel.QueueDeclare(eventName, false, false, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += Consumer_Received;

        channel.BasicConsume(eventName, true, consumer);
    }

    private async Task Consumer_Received(object sender, BasicDeliverEventArgs args)
    {
        var eventName = args.RoutingKey;
        var message = Encoding.UTF8.GetString(args.Body.ToArray());

        try
        {
            await ProcessEvent(eventName, message).ConfigureAwait(false);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (_handlers.ContainsKey(eventName))
        {
            var subscriptions = _handlers[eventName];
            foreach (var subscription in subscriptions)
            {
                var handler = Activator.CreateInstance(subscription);
                if (handler is null) continue;
                var eventType = _eventTypes.SingleOrDefault(s => s.Name == eventName);
                var @event = JsonSerializer.Deserialize(message, eventType!);
                var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType!);
                await (Task)concreteType.GetMethod("Handle")?.Invoke(handler, new object[] { @event! })!;
            }
        }
    }
}