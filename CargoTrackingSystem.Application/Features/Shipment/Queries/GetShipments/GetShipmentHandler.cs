using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

//Defining aliases to prevent naming collisions between modules
using ShipmentEntity = CargoTrackingSystem.Domain.Entities.Shipment;

namespace CargoTrackingSystem.Application.Features.Shipment.Queries.GetShipments;

public class GetShipmentHandler : IRequestHandler<GetShipmentsQuery, PagedShipmentsResponse>
{

    private readonly DbContext _context;
    private readonly IMemoryCache _cache;

    // To clear cache at once created a static shipment cache token
    private const string ShipmentsCacheTokenKey = "Shipments_Cache_Token";

    public GetShipmentHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<PagedShipmentsResponse> Handle(GetShipmentsQuery request, CancellationToken cancellationToken)
    {
        // Address of the Specific Query Cache makes it Unique so user can Specify
        string cacheKey = $"shipments_p:{request.Page}_sz:{request.PageSize}_st:{request.Status ?? "all"}_c:{request.CourierId?.ToString() ?? "all"}_d:{request.CreatedAt?.ToString("yyyyMMdd") ?? "all"}_tn:{request.TrackingNumber ?? "all"}";

        if (_cache.TryGetValue(cacheKey, out PagedShipmentsResponse cachedResponse))
        {
            return cachedResponse;
        }

        var query = _context.Set<ShipmentEntity>()
            .Include(s => s.SenderAddress)
            .Include(s => s.ReceiverAddress)
            .Include(s => s.Courier)
            .Include(s => s.StatusHistories)

            .AsQueryable();

        //Filtering Rules:
        //Checking if Request filters are null or not.
        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(s => s.Status == request.Status);

        //If TrackingNumber Added to Query Parameters(URL) it will filter by.
        if (!string.IsNullOrEmpty(request.TrackingNumber))
            query = query.Where(s => s.TrackingNumber == request.TrackingNumber);

        //If CourierId Added to Query Parameters(URL) it will filter by.
        if (request.CourierId.HasValue)
            query = query.Where(s => s.CourierId == request.CourierId.Value);

        //If CreatedAt Added to Query Parameters(URL) it will sort by the same day Shipments.
        //Example Usage: 2026-07-14 unless it won't work.
        if (request.CreatedAt.HasValue)
        {
            var startDate = DateTime.SpecifyKind(request.CreatedAt.Value.Date, DateTimeKind.Utc);

            var endDate = startDate.AddDays(1);

            query = query.Where(s => s.CreatedAt >= startDate && s.CreatedAt < endDate);
        }

        var totalCargo = await query.CountAsync(cancellationToken);

        //Database Skips Cargo's by page number.
        //For Example If page number is 3, it will skip first 20 cargo which shows between 21-30 cargo.
        var skipCount = (request.Page - 1) * request.PageSize;

        var shipments = await query
            .Skip(skipCount)
            .Take(request.PageSize)

            .Select(s => new
            {
                Id = s.Id,
                TrackingNumber = s.TrackingNumber,
                Status = s.Status,
                Weight = s.Weight,
                CreatedAt = s.CreatedAt,
                SenderAddress = s.SenderAddress,
                ReceiverAddress = s.ReceiverAddress,

                //Checks if Courier New
                Courier = s.Courier != null ? new
                {
                    Id = s.Courier.Id,
                    FullName = $"{s.Courier.FirstName} {s.Courier.LastName}"
                } : null,

                StatusHistories = s.StatusHistories.Select(h => new
                {
                    Id = h.Id,
                    Status = h.Status,
                    ChangedAt = h.ChangedAt
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        //Rounds the page Number.
        //For example if 25(cargo)/10(page size) = 2.5(Totalpage), Totalpage will round to 3.  
        var totalPages = (int)Math.Ceiling((double)totalCargo / request.PageSize);

        var response = new PagedShipmentsResponse(
            Items: shipments,
            CurrentPage: request.Page,
            TotalPages: totalPages,
            TotalCargo: totalCargo
        );

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)) 
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));   

        var cancellationTokenSource = _cache.GetOrCreate(ShipmentsCacheTokenKey, _ => new CancellationTokenSource());
        cacheOptions.AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token));

        _cache.Set(cacheKey, response, cacheOptions);

        return response;
    }
}