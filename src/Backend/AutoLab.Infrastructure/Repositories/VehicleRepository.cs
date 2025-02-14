using AutoLab.Domain.Entities;
using AutoLab.Domain.Exceptions;
using AutoLab.Domain.Interfaces;
using AutoLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoLab.Infrastructure.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly AutoLabDbContext _context;

        public VehicleRepository(AutoLabDbContext context)
        {
            _context = context;
        }

        public async Task<Vehicle> GetByIdAsync(int id)
        {
            return await _context.Vehicles.FindAsync(id)
                ?? throw new DomainException($"Vehículo con ID {id} no encontrado");
        }

        public async Task<(IEnumerable<Vehicle> Items, int Total)> GetAllAsync(
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
            string[]? licensePlates = null)
        {
            if (page <= 0)
                throw new DomainException("El número de página debe ser mayor que 0");
            
            if (pageSize <= 0)
                throw new DomainException("El tamaño de página debe ser mayor que 0");

            var query = _context.Vehicles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(country))
            {
                query = query.Where(v => v.Country == country);
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(v => v.Brand == brand);
            }

            if (!string.IsNullOrWhiteSpace(model))
            {
                query = query.Where(v => v.Model == model);
            }

            if (year.HasValue)
            {
                query = query.Where(v => v.Year == year.Value);
            }

            if (!string.IsNullOrWhiteSpace(licensePlate))
            {
                query = query.Where(v => v.LicensePlate.Contains(licensePlate));
            }

            if (!string.IsNullOrWhiteSpace(vinCode))
            {
                query = query.Where(v => v.VinCode.Contains(vinCode));
            }

            if (yearFrom.HasValue && yearTo.HasValue)
            {
                query = query.Where(v => v.Year >= yearFrom.Value && v.Year <= yearTo.Value);
            }

            if (licensePlates != null && licensePlates.Any())
            {
                query = query.Where(v => licensePlates.Contains(v.LicensePlate));
            }

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                query = orderBy.ToLower() switch
                {
                    "year_desc" => query.OrderByDescending(v => v.Year),
                    "year_asc" => query.OrderBy(v => v.Year),
                    "brand_desc" => query.OrderByDescending(v => v.Brand),
                    "brand_asc" => query.OrderBy(v => v.Brand),
                    _ => query
                };
            }

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Vehicle> AddAsync(Vehicle vehicle)
        {
            // Verificar si ya existe un vehículo con el mismo VIN
            var existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VinCode == vehicle.VinCode);
            
            if (existingVehicle != null)
            {
                throw new DomainException($"Ya existe un vehículo con el código VIN: {vehicle.VinCode}");
            }

            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task UpdateAsync(Vehicle vehicle)
        {
            try 
            {
                var existingVehicle = await GetByIdAsync(vehicle.Id);
                
                // Verificar VIN duplicado
                var duplicateVin = await _context.Vehicles
                    .Where(v => v.Id != vehicle.Id && v.VinCode == vehicle.VinCode)
                    .FirstOrDefaultAsync();
                    
                if (duplicateVin != null)
                {
                    throw new DomainException($"Ya existe otro vehículo con el código VIN: {vehicle.VinCode}");
                }

                // Verificar matrícula duplicada
                var duplicateLicense = await _context.Vehicles
                    .Where(v => v.Id != vehicle.Id && v.LicensePlate == vehicle.LicensePlate)
                    .FirstOrDefaultAsync();

                if (duplicateLicense != null)
                {
                    throw new DomainException($"Ya existe otro vehículo con la matrícula: {vehicle.LicensePlate}");
                }

                _context.Entry(existingVehicle).CurrentValues.SetValues(vehicle);
                await _context.SaveChangesAsync();
                
                // Verificar que se actualizó correctamente
                var updatedVehicle = await GetByIdAsync(vehicle.Id);
                if (!AreVehiclesEqual(updatedVehicle, vehicle))
                {
                    throw new DomainException("Error al verificar la actualización del vehículo");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DomainException("El vehículo ha sido modificado por otro usuario");
            }
            catch (DbUpdateException ex)
            {
                throw new DomainException($"Error al actualizar el vehículo en la base de datos: {ex.Message}");
            }
        }

        private bool AreVehiclesEqual(Vehicle v1, Vehicle v2)
        {
            return v1.Country == v2.Country &&
                   v1.Brand == v2.Brand &&
                   v1.Model == v2.Model &&
                   v1.Year == v2.Year &&
                   v1.LicensePlate == v2.LicensePlate &&
                   v1.VinCode == v2.VinCode;
        }

        public async Task DeleteAsync(int id)
        {
            var vehicle = await GetByIdAsync(id);
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task<Vehicle> GetByLicensePlateAsync(string licensePlate)
        {
            return await _context.Vehicles
                .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
        }
    }
}