using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

//Defining aliases to prevent naming collisions between modules
using CourierEntity = CargoTrackingSystem.Domain.Entities.Courier;

namespace CargoTrackingSystem.Application.Features.Couriers.Queries.GetCouriers;

public class GetCouriersHandler : IRequestHandler<GetCouriersQuery, List<CourierEntity>>
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache; 

    // Unique cache key for store courier list
    private const string CouriersCacheKey = "all_couriers";

    //Cache token used to invalidate the cached courier list when modification occur
    private const string CouriersCacheTokenKey = "Couriers_Cache_Token";

    public GetCouriersHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache; 
    }

    public async Task<List<CourierEntity>> Handle(GetCouriersQuery request, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CouriersCacheKey, out List<CourierEntity> cachedCouriers))
        {
            return cachedCouriers;
        }

        var couriers = await _context.Set<CourierEntity>().ToListAsync(cancellationToken);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(15)) 
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));   

       
        var cancellationTokenSource = _cache.GetOrCreate(CouriersCacheTokenKey, _ => new CancellationTokenSource());
        cacheOptions.AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token));

        _cache.Set(CouriersCacheKey, couriers, cacheOptions);

        return couriers;
    }
}