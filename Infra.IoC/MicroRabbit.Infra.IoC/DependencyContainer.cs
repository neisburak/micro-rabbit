using MediatR;
using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Banking.Application.Services;
using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Banking.Data.Repository;
using MicroRabbit.Banking.Domain.CommandHandlers;
using MicroRabbit.Banking.Domain.Commands;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Infra.Bus;
using MicroRabbit.Transfer.Application.Interfaces;
using MicroRabbit.Transfer.Application.Services;
using MicroRabbit.Transfer.Data.Context;
using MicroRabbit.Transfer.Data.Repository;
using MicroRabbit.Transfer.Domain.EventHandlers;
using MicroRabbit.Transfer.Domain.Events;
using MicroRabbit.Transfer.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MicroRabbit.Infra.IoC;

public static class DependencyContainer
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Domain Bus
        services.AddSingleton<IEventBus, RabbitMQBus>(provider =>
        {
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            return new RabbitMQBus(provider.GetRequiredService<IMediator>(), scopeFactory);
        });

        // Subscription
        services.AddTransient<TransferEventHandler>();

        // Domain Events
        services.AddTransient<IEventHandler<TransferCreatedEvent>, TransferEventHandler>();

        // Domain Commands
        // services.AddTransient<IRequestHandler<CreateTransferCommand, bool>, TransferCommandHandler>();

        // Application Services
        // services.AddTransient<IAccountService, AccountService>();
        services.AddScoped<ITransferService, TransferService>();

        // Data
        // services.AddTransient<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        // services.AddTransient<BankingDbContext>();
        services.AddTransient<TransferDbContext>();

        return services;
    }
}