using AutoLab.Application.DTOs;
using AutoLab.Application.Interfaces;
using AutoLab.Domain.Entities;
using AutoLab.Domain.Interfaces;

namespace AutoLab.Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;

        public VehicleService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<VehicleDto> GetByIdAsync(int id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            return new VehicleDto(vehicle);
        }

        public async Task<(IEnumerable<VehicleDto> Items, int Total)> GetAllAsync(
            int page, 
            int pageSize,
            string country = null,
            string brand = null,
            string model = null,
            int? year = null,
            string licensePlate = null,
            string vinCode = null)
        {
            var (vehicles, total) = await _vehicleRepository.GetAllAsync(
                page, pageSize, country, brand, model, year, licensePlate, vinCode);
            
            return (vehicles.Select(v => new VehicleDto(v)), total);
        }

        public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto)
        {
            var vehicle = new Vehicle(
                dto.Country,
                dto.Brand,
                dto.Model,
                dto.Year,
                dto.LicensePlate,
                dto.VinCode
            );

            await _vehicleRepository.AddAsync(vehicle);
            return new VehicleDto(vehicle);
        }

        public async Task UpdateAsync(int id, CreateVehicleDto dto)
        {
            var existingVehicle = await _vehicleRepository.GetByIdAsync(id);
            
            var updatedVehicle = new Vehicle(
                dto.Country,
                dto.Brand,
                dto.Model,
                dto.Year,
                dto.LicensePlate,
                dto.VinCode
            );

            await _vehicleRepository.UpdateAsync(updatedVehicle);
        }

        public async Task DeleteAsync(int id)
        {
            await _vehicleRepository.DeleteAsync(id);
        }

        // Más métodos del servicio
    }
} 