using AutoLab.API.Middleware;
using AutoLab.Application.Interfaces;
using AutoLab.Application.Services;
using AutoLab.Domain.Interfaces;
using AutoLab.Infrastructure.ExternalServices;
using AutoLab.Infrastructure.Persistence;
using AutoLab.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using AutoLab.Infrastructure.Options;

namespace AutoLab.API;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.IncludeFields = true;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.AllowTrailingCommas = true;
                options.JsonSerializerOptions.DefaultIgnoreCondition = 
                    System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { 
                Title = "AutoLab API", 
                Version = "v1",
                Description = "API para gestión de vehículos de AutoLab"
            });
        });

        services.AddDbContext<AutoLabDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("AutoLab.Infrastructure")
            ));

        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<ICountryService, CountryApiService>();

        // Configuración de opciones
        services.Configure<CountryApiOptions>(
            configuration.GetSection("CountryApi"));

        // Configuración del cliente HTTP
        services.AddHttpClient<ICountryService, CountryApiService>()
            .ConfigureHttpClient((sp, client) => 
            {
                var options = sp.GetRequiredService<IOptions<CountryApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", options.Token);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoLab API V1");
            });
        }

        app.UseCors("AllowAll");
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.MapControllers();
    }
}
