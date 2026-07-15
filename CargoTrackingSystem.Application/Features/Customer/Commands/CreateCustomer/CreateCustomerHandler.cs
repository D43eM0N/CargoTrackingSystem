using CargoTrackingSystem.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CargoTrackingSystem.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, CreateCustomerResponse>
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache;

    public CreateCustomerHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CreateCustomerResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        _context.Set<Customer>().Add(customer);

        await _context.SaveChangesAsync(cancellationToken);

        // Sends a cancellation signal to invalidate the token if the data changed
        if (_cache.TryGetValue("Customers_Cache_Token", out CancellationTokenSource customerCts))
        {
            customerCts.Cancel();
        }

        return new CreateCustomerResponse("Customer Created Successfully!", customer.Id);
    }
}