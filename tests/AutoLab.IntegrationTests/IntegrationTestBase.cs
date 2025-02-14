using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net.Http.Json;
using Xunit;
using AutoLab.Infrastructure.Persistence;
using AutoLab.API;
using AutoLab.Application.Interfaces;
using AutoLab.Application.Services;
using AutoLab.Domain.Interfaces;
using AutoLab.Infrastructure.Repositories;
using AutoLab.Infrastructure.Options;
using AutoLab.Infrastructure.ExternalServices;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Microsoft.AspNetCore.Hosting;
using WireMock.Settings;
using System.Net;
using System.Net.Sockets;

namespace AutoLab.IntegrationTests
{
    public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        protected readonly WebApplicationFactory<Program> _factory;
        protected readonly HttpClient _client;
        private readonly string _databaseName;
        private readonly WireMockServer _mockServer;

        public IntegrationTestBase(WebApplicationFactory<Program> factory)
        {
            _databaseName = Guid.NewGuid().ToString();
            
            var currentDirectory = Directory.GetCurrentDirectory();
            var solutionDirectory = FindSolutionRoot(currentDirectory);
            var contentRoot = Path.Combine(solutionDirectory, "src", "Backend", "AutoLab.API");

            Console.WriteLine($"Directorio actual: {currentDirectory}");
            Console.WriteLine($"Directorio de la solución: {solutionDirectory}");
            Console.WriteLine($"Directorio del contenido: {contentRoot}");

            if (!Directory.Exists(contentRoot))
            {
                throw new DirectoryNotFoundException($"El directorio raíz del contenido no existe: {contentRoot}");
            }

            _mockServer = WireMockServer.Start(new WireMockServerSettings
            {
                UseSSL = false,
                StartAdminInterface = true,
                Port = GetAvailablePort()
            });

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseContentRoot(contentRoot);
                builder.ConfigureServices(services =>
                {
                    // Remover servicios existentes
                    var descriptors = services.Where(d => 
                        d.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true ||
                        d.ServiceType == typeof(DbContextOptions<AutoLabDbContext>) ||
                        d.ServiceType == typeof(DbContextOptions) ||
                        d.ServiceType == typeof(AutoLabDbContext) ||
                        d.ServiceType == typeof(IVehicleRepository) ||
                        d.ServiceType == typeof(IVehicleService) ||
                        d.ServiceType == typeof(ICountryService)).ToList();

                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }

                    // Agregar DbContext en memoria
                    services.AddDbContext<AutoLabDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(_databaseName);
                    });

                    // Registrar servicios necesarios
                    services.AddScoped<IVehicleRepository, VehicleRepository>();
                    services.AddScoped<IVehicleService, VehicleService>();
                    services.AddScoped<ICountryService, CountryApiService>();

                    // Configurar el servicio de países
                    services.AddHttpClient();
                    services.Configure<CountryApiOptions>(options =>
                    {
                        options.Token = "test-token";
                        options.BaseUrl = _mockServer.Urls[0];
                    });

                    // Configurar JSON igual que en Program.cs
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

                    services.AddMvc()
                        .AddApplicationPart(typeof(Program).Assembly)
                        .AddControllersAsServices();
                });
            });

            ConfigureCountryApiMock();
            
            _client = _factory.CreateClient();
        }

        private int GetAvailablePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        protected virtual void ConfigureCountryApiMock()
        {
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v1/countries")
                    .WithHeader("X-CSCAPI-KEY", "*")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("[{\"name\":\"Spain\",\"iso2\":\"ES\"},{\"name\":\"France\",\"iso2\":\"FR\"}]"));
        }

        protected async Task ResetDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AutoLabDbContext>();
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }

        protected async Task<AutoLabDbContext> GetDbContext()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AutoLabDbContext>();
            await context.Database.EnsureCreatedAsync();
            return context;
        }

        protected async Task ClearDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AutoLabDbContext>();
            context.Vehicles.RemoveRange(context.Vehicles);
            await context.SaveChangesAsync();
        }

        public virtual void Dispose()
        {
            _mockServer?.Dispose();
            _factory?.Dispose();
            _client?.Dispose();
        }

        private string FindSolutionRoot(string currentPath)
        {
            while (currentPath != null)
            {
                if (File.Exists(Path.Combine(currentPath, "AutoLab-TechnicalTest.sln")))
                    return currentPath;
                    
                currentPath = Directory.GetParent(currentPath)?.FullName;
            }
            
            throw new DirectoryNotFoundException("No se pudo encontrar el archivo de solución (.sln)");
        }
    }
}