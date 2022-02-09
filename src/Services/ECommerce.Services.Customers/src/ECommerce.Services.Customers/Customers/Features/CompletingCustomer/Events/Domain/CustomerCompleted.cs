using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.CQRS.Command;
using ECommerce.Services.Customers.Customers.Features.UpdatingCustomerReadsModel;
using ECommerce.Services.Customers.Customers.Models;

namespace ECommerce.Services.Customers.Customers.Features.CompletingCustomer.Events.Domain;

public record CustomerCompleted(Customer Customer) : DomainEvent;

internal class CustomerCompletedHandler : IDomainEventHandler<CustomerCompleted>
{
    private readonly IMapper _mapper;
    private readonly ICommandProcessor _commandProcessor;

    public CustomerCompletedHandler(IMapper mapper, ICommandProcessor commandProcessor)
    {
        _mapper = mapper;
        _commandProcessor = commandProcessor;
    }

    public Task Handle(CustomerCompleted notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        var command = _mapper.Map<UpdateCustomerReadsModel>(notification.Customer);

        return _commandProcessor.SendAsync(command, cancellationToken);
    }
}
