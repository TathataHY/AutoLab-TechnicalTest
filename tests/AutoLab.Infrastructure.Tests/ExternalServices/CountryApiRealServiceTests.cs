using AutoLab.Infrastructure.ExternalServices;
using AutoLab.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace AutoLab.Infrastructure.Tests.ExternalServices
{
    public class CountryApiRealServiceTests
    {
        private readonly CountryApiService _service;
        private readonly ITestOutputHelper _output;
        private const string API_TOKEN = "TlRzdE8yUzFCVDk4TlNCTGxrc3RvZnpZcmV0UEQxUFN5aTAybzk4MQ=="; // Reemplazar con tu token real
        private const string API_URL = "https://api.countrystatecity.in";

        public CountryApiRealServiceTests(ITestOutputHelper output)
        {
            _output = output;
            var options = new Mock<IOptions<CountryApiOptions>>();
            options.Setup(x => x.Value).Returns(new CountryApiOptions 
            { 
                Token = API_TOKEN,
                BaseUrl = API_URL
            });

            var httpClient = new HttpClient();
            _service = new CountryApiService(httpClient, options.Object);
        }

        [Fact]
        public async Task GetCountriesAsync_RealService_AnalyzeResponse()
        {
            // Act
            var countries = await _service.GetCountriesAsync();

            // Assert & Log
            Assert.NotNull(countries);
            Assert.NotEmpty(countries);

            // Imprimir los primeros 5 países para análisis
            var countriesList = countries.Take(5).ToList();
            _output.WriteLine("Primeros 5 países recibidos:");
            foreach (var country in countriesList)
            {
                _output.WriteLine($"- {country}");
            }
        }

        [Fact]
        public async Task GetCountriesAsync_RealService_AnalyzeFullResponse()
        {
            // Arrange
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-CSCAPI-KEY", API_TOKEN);

            // Act
            var response = await httpClient.GetAsync($"{API_URL}/v1/countries");
            var jsonString = await response.Content.ReadAsStringAsync();

            // Deserializar para ver la estructura completa
            var rawResponse = JsonSerializer.Deserialize<JsonElement>(jsonString);

            // Assert & Log
            _output.WriteLine("Estructura completa de la respuesta:");
            _output.WriteLine(JsonSerializer.Serialize(rawResponse, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            }));
        }

        [Theory]
        [InlineData("Spain")]
        [InlineData("France")]
        [InlineData("Germany")]
        [InlineData("United States")]
        [InlineData("United Kingdom")]
        public async Task ValidateCountryAsync_RealService_TestSpecificCountries(string countryName)
        {
            // Act & Assert
            await _service.ValidateCountryAsync(countryName);
            _output.WriteLine($"País validado exitosamente: {countryName}");
        }

        [Fact]
        public async Task GetCountriesAsync_RealService_AnalyzeErrorResponses()
        {
            // Arrange
            var invalidOptions = new Mock<IOptions<CountryApiOptions>>();
            invalidOptions.Setup(x => x.Value).Returns(new CountryApiOptions 
            { 
                Token = "invalid-token",
                BaseUrl = API_URL
            });

            var httpClient = new HttpClient();
            var invalidService = new CountryApiService(httpClient, invalidOptions.Object);

            // Act & Assert
            try
            {
                await invalidService.GetCountriesAsync();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error esperado: {ex.Message}");
                _output.WriteLine($"Tipo de excepción: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    _output.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }
    }
} 