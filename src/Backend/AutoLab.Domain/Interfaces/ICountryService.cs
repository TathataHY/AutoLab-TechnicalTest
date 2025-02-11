using AutoLab.Domain.Entities;

namespace AutoLab.Domain.Interfaces
{
    public interface ICountryService
    {
        Task<IEnumerable<Country>> GetCountriesAsync();
    }
}