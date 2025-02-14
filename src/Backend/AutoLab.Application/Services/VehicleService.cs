using AutoLab.Application.DTOs;
using AutoLab.Application.Interfaces;
using AutoLab.Domain.Entities;
using AutoLab.Domain.Exceptions;
using AutoLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoLab.Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ICountryService _countryService;

        public VehicleService(
            IVehicleRepository vehicleRepository,
            ICountryService countryService)
        {
            _vehicleRepository = vehicleRepository;
            _countryService = countryService;
        }

        public async Task<VehicleDto> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new DomainException("El ID debe ser mayor que 0");

            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            return new VehicleDto(vehicle);
        }

        public async Task<(IEnumerable<VehicleDto> Items, int Total)> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string country = null,
            string brand = null,
            string model = null,
            int? year = null,
            string licensePlate = null,
            string vinCode = null)
        {
            if (page <= 0)
                throw new DomainException("El número de página debe ser mayor que 0");

            if (pageSize <= 0 || pageSize > 100)
                throw new DomainException("El tamaño de página debe estar entre 1 y 100");

            var (vehicles, total) = await _vehicleRepository.GetAllAsync(
                page, 
                pageSize, 
                country?.Trim(), 
                brand?.Trim(), 
                model?.Trim(), 
                year, 
                licensePlate?.Trim(), 
                vinCode?.Trim());

            return (vehicles.Select(v => new VehicleDto(v)), total);
        }

        public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto)
        {
            // Validar si ya existe la placa
            var existingVehicle = await _vehicleRepository.GetByLicensePlateAsync(dto.LicensePlate);
            if (existingVehicle != null)
                throw new DomainException($"Ya existe un vehículo con la placa {dto.LicensePlate}");

            // Primero validamos el DTO
            dto.Validate();
            
            try 
            {
                // Luego validamos el país con el servicio externo
                await _countryService.ValidateCountryAsync(dto.Country);

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
            catch (DomainException ex)
            {
                throw new DomainException($"Error de validación: {ex.Message}");
            }
        }

        public async Task UpdateAsync(int id, CreateVehicleDto dto)
        {
            if (id <= 0)
                throw new DomainException("El ID debe ser mayor que 0");

            var existingVehicle = await _vehicleRepository.GetByIdAsync(id);
            
            dto.Validate();
            
            try 
            {
                var updatedVehicle = new Vehicle(
                    dto.Country,
                    dto.Brand,
                    dto.Model,
                    dto.Year,
                    dto.LicensePlate,
                    dto.VinCode
                )
                {
                    Id = id
                };

                await _vehicleRepository.UpdateAsync(updatedVehicle);
            }
            catch (Exception ex)
            {
                throw new DomainException($"Error al actualizar el vehículo: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id)
        {
            await _vehicleRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<VehicleDto>> SearchAsync(string term, string type)
        {
            var (vehicles, _) = await _vehicleRepository.GetAllAsync(
                page: 1,
                pageSize: 100,
                licensePlate: type == "licensePlate" ? term : null,
                vinCode: type == "vinCode" ? term : null
            );

            return vehicles.Select(v => new VehicleDto(v));
        }

        // Más métodos del servicio
    }
} 