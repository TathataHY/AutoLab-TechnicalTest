using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoLab.API;
using AutoLab.Application.DTOs;
using AutoLab.Domain.Interfaces;
using AutoLab.Infrastructure.ExternalServices;
using AutoLab.Infrastructure.Options;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace AutoLab.IntegrationTests
{
    public class CountryApiIntegrationTests : IntegrationTestBase, IDisposable
    {
        private readonly HttpClient _client;
        private readonly WireMockServer _mockServer;
        private const string TEST_TOKEN = "test-token";

        public CountryApiIntegrationTests(WebApplicationFactory<Program> factory) 
            : base(factory)
        {
            _mockServer = WireMockServer.Start();
            Console.WriteLine($"Mock Server URL: {_mockServer.Urls[0]}"); // Para debug
            
            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptors = services.Where(d => 
                        d.ServiceType == typeof(IConfigureOptions<CountryApiOptions>) ||
                        d.ServiceType == typeof(ICountryService)).ToList();
                    
                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }

                    services.Configure<CountryApiOptions>(options =>
                    {
                        options.BaseUrl = _mockServer.Urls[0];
                        options.Token = TEST_TOKEN;
                    });

                    services.AddHttpClient<ICountryService, CountryApiService>()
                        .ConfigureHttpClient(client => 
                        {
                            client.Timeout = TimeSpan.FromSeconds(2);
                        });
                });
            }).CreateClient();
        }

        [Fact]
        public async Task CountryApi_ValidCountry_ReturnsSuccess()
        {
            // Arrange
            await ResetDatabase();
            
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v1/countries")
                    .WithHeader("X-CSCAPI-KEY", TEST_TOKEN)
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("[{\"name\":\"Spain\",\"iso2\":\"ES\"}]"));

            var createDto = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/vehicles", createDto);
            
            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CountryApi_InvalidCountry_ReturnsBadRequest()
        {
            // Arrange
            await ResetDatabase();
            
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v1/countries")
                    .WithHeader("X-CSCAPI-KEY", TEST_TOKEN)
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("[]")); // Lista vacía = país no encontrado

            var createDto = new CreateVehicleDto
            {
                Country = "PaísInexistente",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/vehicles", createDto);
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CountryApi_ServiceUnavailable_HandlesError()
        {
            // Arrange
            await ResetDatabase();
            
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v1/countries")
                    .WithHeader("X-CSCAPI-KEY", TEST_TOKEN)
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(503)
                    .WithBody("{\"error\": \"Service Unavailable\"}"));

            var createDto = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/vehicles", createDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CountryApi_SlowResponse_HandlesTimeout()
        {
            // Arrange
            await ResetDatabase();
            
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v1/countries")
                    .WithHeader("X-CSCAPI-KEY", TEST_TOKEN)
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithDelay(TimeSpan.FromSeconds(5))
                    .WithBody("[{\"name\":\"Spain\",\"iso2\":\"ES\"}]"));

            var createDto = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/vehicles", createDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        public new void Dispose()
        {
            _mockServer?.Dispose();
            base.Dispose();
        }
    }
} 