using AutoLab.Domain.DTOs;
using AutoLab.Domain.Entities;

namespace AutoLab.Domain.Interfaces
{
    public interface ICountryService
    {
        Task<IEnumerable<string>> GetCountriesAsync();
        Task ValidateCountryAsync(string countryName);
        Task<IEnumerable<CountrySelectDto>> GetCountriesForSelectAsync();
    }
}