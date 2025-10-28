using api.Repositories;
using api.Services;
using Microsoft.EntityFrameworkCore;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                // Suppress automatic 400 response to allow custom error handling
                options.SuppressModelStateInvalidFilter = true;
            });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(); // Add Swashbuckle Swagger generation
        builder.Services.AddOpenApi(); // Add Microsoft's native OpenAPI generation


        // Add database context
        if (builder.Environment.EnvironmentName != "Testing")
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Database=registration_db;Username=user;Password=postgres";
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        // Add repositories
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // Add services
        builder.Services.AddScoped<IRegisterService, RegisterService>();

        // Add logging
        builder.Services.AddLogging();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "My API V1"); // Adjust path as needed
                });
        }

        app.UseHttpsRedirection();

        // Map controllers
        app.MapControllers();

        var summaries = new[]
        {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

        app.MapGet("/weatherforecast", () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast");

        app.Run();
    }
}

public partial class Program { }

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
