using AutoLab.Domain.Entities;
using AutoLab.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace AutoLab.Infrastructure.ExternalServices
{
    public class CountryApiService : ICountryService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public CountryApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["CountryApi:ApiKey"];
            _httpClient.BaseAddress = new Uri("https://api.countrystatecity.in/v1/");
            _httpClient.DefaultRequestHeaders.Add("X-CSCAPI-KEY", _apiKey);
        }

        public async Task<IEnumerable<Country>> GetCountriesAsync()
        {
            var response = await _httpClient.GetAsync("countries");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var countries = JsonSerializer.Deserialize<List<ApiCountryResponse>>(content);
            
            return countries.Select(c => new Country(c.Id, c.Name, c.Iso2));
        }
    }

    public class ApiCountryResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Iso2 { get; set; }
    }
} 