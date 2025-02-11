using AutoLab.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AutoLabDbContext).Assembly);
        }
    }
} 