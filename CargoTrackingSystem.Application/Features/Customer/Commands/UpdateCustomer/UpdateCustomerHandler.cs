using CargoTrackingSystem.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CargoTrackingSystem.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, UpdateCustomerResponse>
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache;

    public UpdateCustomerHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<UpdateCustomerResponse> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Set<Customer>().FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            throw new KeyNotFoundException("Customer to be updated could not found.");
        }

        //Not Specified Values will be null by default and will skipped and remain unchanged
        if (!string.IsNullOrEmpty(request.FullName))
            customer.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.Email))
            customer.Email = request.Email;

        if (!string.IsNullOrEmpty(request.PhoneNumber))
            customer.PhoneNumber = request.PhoneNumber;

        await _context.SaveChangesAsync(cancellationToken);

        // Sends a cancellation signal to invalidate the token because the data changed
        if (_cache.TryGetValue("Customers_Cache_Token", out CancellationTokenSource customerCts))
        {
            customerCts.Cancel();
        }

        return new UpdateCustomerResponse("Customer information updated successfully!");
    }
}