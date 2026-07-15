using CargoTrackingSystem.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CargoTrackingSystem.Application.Features.Customers.Commands.DeleteCustomer;

public class DeleteCustomerHandler : IRequestHandler<DeleteCustomerCommand, DeleteCustomerResponse>
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache;

    public DeleteCustomerHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<DeleteCustomerResponse> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Set<Customer>().FindAsync(new object[] { request.Id }, cancellationToken);

        if (customer == null)
        {
            throw new KeyNotFoundException("Customer not found!");
        }

        _context.Set<Customer>().Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);

        // Sends a cancellation signal to invalidate the token if the data changed
        if (_cache.TryGetValue("Customers_Cache_Token", out CancellationTokenSource customerCts))
        {
            customerCts.Cancel();
        }

        return new DeleteCustomerResponse($"{customer.FullName} named customer deleted successfully.");
    }
}
