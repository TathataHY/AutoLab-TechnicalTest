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
            string country = null,
            string brand = null,
            string model = null,
            int? year = null,
            string licensePlate = null,
            string vinCode = null)
        {
            var query = _context.Vehicles.AsQueryable();

            if (!string.IsNullOrEmpty(country))
                query = query.Where(v => v.Country.Contains(country));
            
            if (!string.IsNullOrEmpty(brand))
                query = query.Where(v => v.Brand.Contains(brand));
            
            if (!string.IsNullOrEmpty(model))
                query = query.Where(v => v.Model.Contains(model));
            
            if (year.HasValue)
                query = query.Where(v => v.Year == year.Value);
            
            if (!string.IsNullOrEmpty(licensePlate))
                query = query.Where(v => v.LicensePlate.Contains(licensePlate));
            
            if (!string.IsNullOrEmpty(vinCode))
                query = query.Where(v => v.VinCode.Contains(vinCode));

            var total = await query.CountAsync();
            
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Vehicle> AddAsync(Vehicle vehicle)
        {
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task UpdateAsync(Vehicle vehicle)
        {
            _context.Entry(vehicle).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var vehicle = await GetByIdAsync(id);
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
        }
    }
} 