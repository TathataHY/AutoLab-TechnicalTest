using AutoLab.Domain.Entities;

namespace AutoLab.Domain.Interfaces
{
    public interface IVehicleRepository
    {
        Task<Vehicle> GetByIdAsync(int id);
        Task<(IEnumerable<Vehicle> Items, int Total)> GetAllAsync(
            int page,
            int pageSize,
            string? country = null,
            string? brand = null,
            string? model = null,
            int? year = null,
            string? licensePlate = null,
            string? vinCode = null,
            string? orderBy = null,
            int? yearFrom = null,
            int? yearTo = null,
            string[]? licensePlates = null);
        Task<Vehicle> AddAsync(Vehicle vehicle);
        Task UpdateAsync(Vehicle vehicle);
        Task DeleteAsync(int id);
        Task<Vehicle?> GetByLicensePlateAsync(string licensePlate);
    }
} 