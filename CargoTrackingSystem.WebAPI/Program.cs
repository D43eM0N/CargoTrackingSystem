using CargoTrackingSystem.Application.Features.Shipment.Commands.CreateShipment;
using CargoTrackingSystem.Application.Validator.Shipment;
using CargoTrackingSystem.Infrastructure;
using CargoTrackingSystem.WebAPI.Middlewares;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore; 

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<CargoTrackingSystem.Infrastructure.AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        //To prevent Infinity Loop between Shipment and ShipmentStatusHistory 
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

//Telling MediatR to how to find Command and Query Files.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateShipment).Assembly));

//If DbContext Wanted, Send instead AppDbContext(Needed for Application layer to access AppDbContext independently.)
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());

builder.Services.AddOpenApi(options =>
{
    //We register the Transformer here that adds example templates to the OpenAPI document.
    options.AddSchemaTransformer<CargoTrackingSystem.WebAPI.Infrastructure.OpenApi.ExampleSchemaTransformer>();
}   
);

// Implementing FluenValidation Rules
builder.Services.AddValidatorsFromAssemblyContaining<CreateShipmentValidator>();

//Implementing CargoStatusWorker
builder.Services.AddHostedService<CargoTrackingSystem.Infrastructure.CargoStatusWorker>();

builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.MapOpenApi();
    //Referencing Scalar to Use when Program StartUp.
    app.MapScalarApiReference();

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CargoTrackingSystem.Infrastructure.AppDbContext>();
 
        //Timeout Mechanism that waits for database to run
        int retryCount = 0;
        while (retryCount < 5)
        {
            try
            {
                if (context.Database.CanConnect()) 
                    break;
            }
            catch
            {
                retryCount++;
                Console.WriteLine($"Could not connect to the database. Retrying... ({retryCount}/5)");

                //Wait for 3 sec
                System.Threading.Thread.Sleep(3000);
            }
        }

        // Automatically applies migrations (creates the db and tables if they do not exist)
        context.Database.Migrate();
        Console.WriteLine("Database Updated Successfully! (Migrations Applied).");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while performing the database migration.");
    }
}

app.Run();