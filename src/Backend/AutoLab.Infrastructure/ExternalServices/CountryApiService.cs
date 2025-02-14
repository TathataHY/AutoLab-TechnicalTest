using AutoLab.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Options;
using AutoLab.Domain.Exceptions;
using AutoLab.Infrastructure.Options;
using System.Net.Http.Json;
using AutoLab.Domain.DTOs;

namespace AutoLab.Infrastructure.ExternalServices
{
    public class CountryApiService : ICountryService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<CountryApiOptions> _options;

        public CountryApiService(HttpClient httpClient, IOptions<CountryApiOptions> options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<IEnumerable<string>> GetCountriesAsync()
        {
            try
            {
                if (_httpClient.DefaultRequestHeaders.Contains("X-CSCAPI-KEY"))
                {
                    _httpClient.DefaultRequestHeaders.Remove("X-CSCAPI-KEY");
                }
                _httpClient.DefaultRequestHeaders.Add("X-CSCAPI-KEY", _options.Value.Token);

                using var response = await _httpClient.GetAsync($"{_options.Value.BaseUrl}/v1/countries");

                if (!response.IsSuccessStatusCode)
                {
                    throw new DomainException("El servicio de validación de países no está disponible");
                }

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content) || content == "[]")
                {
                    throw new DomainException("No se encontraron países disponibles");
                }

                var countries = await response.Content.ReadFromJsonAsync<List<CountryResponse>>();
                if (countries == null || !countries.Any())
                {
                    throw new DomainException("No se encontraron países disponibles");
                }
                return countries.Select(c => c.Name).ToList();
            }
            catch (TaskCanceledException)
            {
                throw new DomainException("Tiempo de espera agotado en el servicio de validación");
            }
            catch (HttpRequestException)
            {
                throw new DomainException("Error de conexión con el servicio de validación");
            }
            catch (JsonException)
            {
                throw new DomainException("Error al procesar la respuesta del servicio");
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                throw new DomainException("Error inesperado en el servicio de validación");
            }
        }

        public async Task ValidateCountryAsync(string countryName)
        {
            if (string.IsNullOrWhiteSpace(countryName))
            {
                throw new DomainException("El país no puede estar vacío");
            }

            try
            {
                var countries = await GetCountriesAsync();

                // Normalizar el nombre del país para la comparación
                var normalizedInput = countryName.Trim().ToLowerInvariant()
                    .Replace("united states", "united states of america")
                    .Replace("uk", "united kingdom");

                var normalizedCountries = countries.Select(c => c.Trim().ToLowerInvariant());

                if (!normalizedCountries.Any(c => c.Contains(normalizedInput) || normalizedInput.Contains(c)))
                {
                    throw new DomainException($"El país '{countryName}' no está en la lista de países válidos");
                }
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DomainException($"Error al validar el país: {ex.Message}");
            }
        }

        public async Task<IEnumerable<CountrySelectDto>> GetCountriesForSelectAsync()
        {
            try
            {
                var countries = await GetCountriesAsync();
                return countries.Select(c => new CountrySelectDto
                {
                    Value = c,
                    Label = c,
                    SearchText = c.ToLowerInvariant()
                });
            }
            catch (Exception ex)
            {
                throw new DomainException($"Error al obtener países para el selector: {ex.Message}");
            }
        }

        private class CountryResponse
        {
            public string Name { get; set; } = string.Empty;
            public string Iso2 { get; set; } = string.Empty;
            public string Iso3 { get; set; } = string.Empty;
            public string? Native { get; set; }
            public string? Currency { get; set; }
            public string? Currency_Name { get; set; }
            public string? Currency_Symbol { get; set; }
            public string? Region { get; set; }
            public string? Subregion { get; set; }
            public string? Emoji { get; set; }
            public string? EmojiU { get; set; }
        }
    }
}