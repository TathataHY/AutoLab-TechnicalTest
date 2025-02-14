using AutoLab.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AutoLab.Infrastructure.Persistence
{
    public class AutoLabDbContext : DbContext
    {
        public AutoLabDbContext(DbContextOptions<AutoLabDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Esto aplicará automáticamente todas las configuraciones en el assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
} 