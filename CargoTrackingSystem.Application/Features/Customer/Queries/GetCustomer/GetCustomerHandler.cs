using MediatR;
using Microsoft.EntityFrameworkCore;
using CargoTrackingSystem.Domain.Entities;
using Microsoft.Extensions.Caching.Memory; 
using Microsoft.Extensions.Primitives; 

namespace CargoTrackingSystem.Application.Features.Customers.Queries.GetCustomers;

public class GetCustomersHandler : IRequestHandler<GetCustomersQuery, List<Customer>>
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache;

    //  Unique cache key for store customer list
    private const string CustomersCacheKey = "all_customers";

    //Cache token used to invalidate the cached customer list when modification occur
    private const string CustomersCacheTokenKey = "Customers_Cache_Token";

    public GetCustomersHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache; 
    }

    public async Task<List<Customer>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CustomersCacheKey, out List<Customer> cachedCustomers))
        {
            return cachedCustomers;
        }

        var customers = await _context.Set<Customer>().ToListAsync(cancellationToken);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20)) 
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));   

        var cancellationTokenSource = _cache.GetOrCreate(CustomersCacheTokenKey, _ => new CancellationTokenSource());
        cacheOptions.AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token));

        _cache.Set(CustomersCacheKey, customers, cacheOptions);

        return customers;
    }
}