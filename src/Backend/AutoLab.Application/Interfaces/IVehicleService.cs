using AutoLab.Application.DTOs;

namespace AutoLab.Application.Interfaces
{
    public interface IVehicleService
    {
        Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto);
        Task<VehicleDto> GetByIdAsync(int id);
        Task<(IEnumerable<VehicleDto> Items, int Total)> GetAllAsync(
            int page, 
            int pageSize,
            string country = null,
            string brand = null,
            string model = null,
            int? year = null,
            string licensePlate = null,
            string vinCode = null);
        Task UpdateAsync(int id, CreateVehicleDto dto);
        Task DeleteAsync(int id);
    }
} 