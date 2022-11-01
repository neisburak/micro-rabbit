using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;

namespace MicroRabbit.Domain.Core.Bus;

public interface IEventBus
{
    Task SendCommand<TCommand>(TCommand command) where TCommand : Command;
    void Publish<TEvent>(TEvent @event) where TEvent : Event;
    void Subscribe<TEvent, THandler>() where TEvent : Event where THandler : IEventHandler<TEvent>;
}