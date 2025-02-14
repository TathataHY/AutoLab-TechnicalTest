using AutoLab.Domain.Entities;
using AutoLab.Domain.Exceptions;
using AutoLab.Infrastructure.Persistence;
using AutoLab.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AutoLab.Infrastructure.Tests.Repositories
{
    public class VehicleRepositoryTests
    {
        private readonly AutoLabDbContext _context;
        private readonly VehicleRepository _repository;

        public VehicleRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AutoLabDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AutoLabDbContext(options);
            _repository = new VehicleRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingVehicle_ReturnsVehicle()
        {
            // Arrange
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(vehicle.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(vehicle.Country, result.Country);
            Assert.Equal(vehicle.Brand, result.Brand);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingVehicle_ThrowsDomainException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _repository.GetByIdAsync(999));
        }

        [Fact]
        public async Task GetAllAsync_WithFilters_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("Francia", "Renault", "Clio", 2021, "XYZ456FR", "98765432109876543"),
                new Vehicle("España", "Seat", "Leon", 2019, "DEF789ES", "45678901234567890")
            };

            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(
                page: 1,
                pageSize: 10,
                country: "España"
            );

            // Assert
            Assert.Equal(2, total);
            Assert.Equal(2, results.Count());
            Assert.All(results, v => Assert.Equal("España", v.Country));
        }

        [Fact]
        public async Task AddAsync_ValidVehicle_SavesToDatabase()
        {
            // Arrange
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");

            // Act
            var result = await _repository.AddAsync(vehicle);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(0, result.Id);
            var savedVehicle = await _context.Vehicles.FindAsync(result.Id);
            Assert.NotNull(savedVehicle);
        }

        [Fact]
        public async Task DeleteAsync_ExistingVehicle_RemovesFromDatabase()
        {
            // Arrange
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(vehicle.Id);

            // Assert
            var deletedVehicle = await _context.Vehicles.FindAsync(vehicle.Id);
            Assert.Null(deletedVehicle);
        }

        [Fact]
        public async Task AddAsync_DuplicateVinCode_ThrowsDomainException()
        {
            // Arrange
            var existingVehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            await _context.Vehicles.AddAsync(existingVehicle);
            await _context.SaveChangesAsync();

            var duplicateVehicle = new Vehicle("Francia", "Renault", "Clio", 2021, "XYZ456FR", "12345678901234567");

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _repository.AddAsync(duplicateVehicle));
        }

        [Fact]
        public async Task UpdateAsync_DuplicateVinCode_ThrowsDomainException()
        {
            // Arrange
            var vehicle1 = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            var vehicle2 = new Vehicle("Francia", "Renault", "Clio", 2021, "XYZ456FR", "98765432109876543");
            await _context.Vehicles.AddRangeAsync(vehicle1, vehicle2);
            await _context.SaveChangesAsync();

            // Crear un nuevo vehículo para la actualización con el VIN del primer vehículo
            var updateVehicle = new Vehicle("Italia", "Fiat", "500", 2022, "DEF789IT", "12345678901234567");
            updateVehicle.Id = vehicle2.Id;  // Asignar el ID del segundo vehículo

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _repository.UpdateAsync(updateVehicle));
        }

        [Fact]
        public async Task GetAllAsync_WithPagination_ReturnsCorrectItems()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "11111111111111111"),
                new Vehicle("Francia", "Renault", "Clio", 2021, "DEF456FR", "22222222222222222"),
                new Vehicle("Italia", "Fiat", "500", 2022, "GHI789IT", "33333333333333333")
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(page: 2, pageSize: 1);

            // Assert
            Assert.Single(results);
            Assert.Equal(3, total);
            Assert.Equal(vehicles[1].VinCode, results.First().VinCode);
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleFilters_ReturnsCorrectResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123", "11111111111111111"),
                new Vehicle("España", "Toyota", "Camry", 2020, "DEF456", "22222222222222222"),
                new Vehicle("Francia", "Toyota", "Corolla", 2020, "GHI789", "33333333333333333")
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(
                page: 1,
                pageSize: 10,
                country: "España",
                brand: "Toyota",
                model: "Corolla"
            );

            // Assert
            Assert.Single(results);
            Assert.Equal(1, total);
            var vehicle = results.First();
            Assert.Equal("España", vehicle.Country);
            Assert.Equal("Toyota", vehicle.Brand);
            Assert.Equal("Corolla", vehicle.Model);
        }

        [Fact]
        public async Task UpdateAsync_ConcurrentModification_ThrowsDomainException()
        {
            // Arrange
            var originalVehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            await _context.Vehicles.AddAsync(originalVehicle);
            await _context.SaveChangesAsync();

            // Crear dos instancias nuevas para la actualización
            var updateVehicle1 = new Vehicle("Francia", "Renault", "Clio", 2021, "XYZ456FR", "98765432109876543");
            var updateVehicle2 = new Vehicle("Italia", "Fiat", "500", 2022, "DEF789IT", "45678901234567890");
            updateVehicle1.Id = originalVehicle.Id;
            updateVehicle2.Id = originalVehicle.Id;

            var context2 = new AutoLabDbContext(
                new DbContextOptionsBuilder<AutoLabDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options);
            
            var repository2 = new VehicleRepository(context2);
            
            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => 
                Task.WhenAll(
                    _repository.UpdateAsync(updateVehicle1),
                    repository2.UpdateAsync(updateVehicle2)
                )
            );
        }

        [Fact]
        public async Task GetAllAsync_EmptyPage_ReturnsEmptyList()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "11111111111111111"),
                new Vehicle("Francia", "Renault", "Clio", 2021, "DEF456FR", "22222222222222222")
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(page: 3, pageSize: 1);

            // Assert
            Assert.Empty(results);
            Assert.Equal(2, total);
        }

        [Fact]
        public async Task GetAllAsync_WithPartialLicensePlate_ReturnsMatchingResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "11111111111111111"),
                new Vehicle("Francia", "Renault", "Clio", 2021, "ABC456FR", "22222222222222222"),
                new Vehicle("Italia", "Fiat", "500", 2022, "XYZ789IT", "33333333333333333")
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(
                page: 1,
                pageSize: 10,
                licensePlate: "ABC"
            );

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Equal(2, total);
            Assert.All(results, v => Assert.Contains("ABC", v.LicensePlate));
        }

        [Fact]
        public async Task GetAllAsync_OrdersResultsById()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("Italia", "Fiat", "500", 2022, "XYZ789IT", "33333333333333333"),
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "11111111111111111"),
                new Vehicle("Francia", "Renault", "Clio", 2021, "DEF456FR", "22222222222222222")
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, _) = await _repository.GetAllAsync(page: 1, pageSize: 10);

            // Assert
            var orderedIds = results.Select(v => v.Id).ToList();
            Assert.Equal(orderedIds, orderedIds.OrderBy(id => id).ToList());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetAllAsync_WithInvalidPageSize_ThrowsDomainException(int pageSize)
        {
            // Arrange
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => 
                _repository.GetAllAsync(page: 1, pageSize: pageSize));
        }

        [Fact]
        public async Task GetAllAsync_WithPartialVinCode_ReturnsMatchingResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "11111AAAAA1111111"),
                new Vehicle("Francia", "Renault", "Clio", 2021, "XYZ456FR", "22222AAAAA2222222"),
                new Vehicle("Italia", "Fiat", "500", 2022, "DEF789IT", "33333BBBBB3333333")
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(
                page: 1,
                pageSize: 10,
                vinCode: "AAAAA"
            );

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Equal(2, total);
            Assert.All(results, v => Assert.Contains("AAAAA", v.VinCode));
        }

        [Fact]
        public async Task DeleteAsync_ConcurrentDeletion_ThrowsDomainException()
        {
            // Arrange
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();

            var context2 = new AutoLabDbContext(
                new DbContextOptionsBuilder<AutoLabDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options);
            var repository2 = new VehicleRepository(context2);

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => 
                Task.WhenAll(
                    _repository.DeleteAsync(vehicle.Id),
                    repository2.DeleteAsync(vehicle.Id)
                )
            );
        }

        [Fact]
        public async Task UpdateAsync_DuplicateLicensePlate_ThrowsDomainException()
        {
            // Arrange
            var vehicle1 = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "11111111111111111");
            var vehicle2 = new Vehicle("Francia", "Renault", "Clio", 2021, "XYZ456FR", "22222222222222222");
            await _context.Vehicles.AddRangeAsync(vehicle1, vehicle2);
            await _context.SaveChangesAsync();

            var updateVehicle = new Vehicle("Italia", "Fiat", "500", 2022, "ABC123DE", "33333333333333333");
            updateVehicle.Id = vehicle2.Id;

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _repository.UpdateAsync(updateVehicle));
        }

        [Fact]
        public async Task GetAllAsync_WithBrandCaseSensitive_ReturnsCorrectResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "11111111111111111"),
                new Vehicle("España", "TOYOTA", "Camry", 2021, "DEF456ES", "22222222222222222"),
                new Vehicle("España", "toyota", "Yaris", 2022, "GHI789ES", "33333333333333333")
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(
                page: 1,
                pageSize: 10,
                brand: "Toyota"
            );

            // Assert
            Assert.Single(results);
            Assert.Equal(1, total);
            Assert.Equal("Toyota", results.First().Brand);
        }

        [Fact]
        public async Task GetAllAsync_WithYearFilter_ReturnsCorrectResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "11111111111111111"),
                new Vehicle("Francia", "Renault", "Clio", 2020, "DEF456FR", "22222222222222222"),
                new Vehicle("Italia", "Fiat", "500", 2021, "GHI789IT", "33333333333333333")
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(
                page: 1,
                pageSize: 10,
                year: 2020
            );

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Equal(2, total);
            Assert.All(results, v => Assert.Equal(2020, v.Year));
        }

        [Fact]
        public async Task GetByLicensePlateAsync_WithExactMatch_ReturnsCorrectVehicle()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "11111111111111111"),
                new Vehicle("Francia", "Renault", "Clio", 2021, "ABC456FR", "22222222222222222")
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByLicensePlateAsync("ABC123DE");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ABC123DE", result.LicensePlate);
            Assert.Equal("España", result.Country);
        }

        [Fact]
        public async Task GetByLicensePlateAsync_NonExistingPlate_ReturnsNull()
        {
            // Arrange
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "11111111111111111");
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByLicensePlateAsync("XYZ999XX");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_WithSameLicensePlate_DoesNotThrowException()
        {
            // Arrange
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "11111111111111111");
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();

            var updateVehicle = new Vehicle("España", "Toyota", "Corolla", 2021, "ABC123DE", "11111111111111111");
            updateVehicle.Id = vehicle.Id;

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _repository.UpdateAsync(updateVehicle));
            Assert.Null(exception);
        }

        [Fact]
        public async Task GetAllAsync_WithSpecialCharactersInFilters_HandlesCorrectly()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123", "11111111111111111"),  // Matrícula ajustada
                new Vehicle("España", "Toyota", "Corolla", 2021, "XYZ456", "22222222222222222")   // Matrícula ajustada
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(
                page: 1,
                pageSize: 10,
                licensePlate: "123"
            );

            // Assert
            Assert.Single(results);
            Assert.Equal(1, total);
            Assert.Contains("123", results.First().LicensePlate);
        }

        [Fact]
        public async Task GetAllAsync_WithMaxPageSize_ReturnsCorrectResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>();
            for (int i = 0; i < 105; i++)
            {
                vehicles.Add(new Vehicle(
                    "España", 
                    "Toyota", 
                    $"Modelo{i}", 
                    2020, 
                    $"ABC{i:000}DE", 
                    $"{i:00000000000000000}")
                );
            }
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(page: 1, pageSize: 100);

            // Assert
            Assert.Equal(100, results.Count());
            Assert.Equal(105, total);
        }

        [Fact]
        public async Task GetAllAsync_WithLastPage_ReturnsRemainingResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>();
            for (int i = 0; i < 25; i++)
            {
                vehicles.Add(new Vehicle(
                    "España", 
                    "Toyota", 
                    $"Modelo{i}", 
                    2020, 
                    $"ABC{i:000}DE", 
                    $"{i:00000000000000000}")
                );
            }
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(page: 3, pageSize: 10);

            // Assert
            Assert.Equal(5, results.Count());
            Assert.Equal(25, total);
        }

        [Fact]
        public async Task AddAsync_WithMaxLengthValues_SavesCorrectly()
        {
            // Arrange
            var vehicle = new Vehicle(
                "España".PadRight(50, 'x'),
                "Toyota".PadRight(50, 'x'),
                "Corolla".PadRight(50, 'x'),
                2020,
                "ABC123",  // Matrícula ajustada a 6 caracteres
                "12345678901234567"
            );

            // Act
            var result = await _repository.AddAsync(vehicle);

            // Assert
            Assert.NotNull(result);
            var savedVehicle = await _context.Vehicles.FindAsync(result.Id);
            Assert.NotNull(savedVehicle);
            Assert.Equal(vehicle.Country, savedVehicle.Country);
            Assert.Equal(vehicle.Brand, savedVehicle.Brand);
            Assert.Equal(vehicle.Model, savedVehicle.Model);
            Assert.Equal(vehicle.LicensePlate, savedVehicle.LicensePlate);
        }

        [Fact]
        public async Task GetAllAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var vehicles = new List<Vehicle>();
            for (int i = 1; i <= 15; i++)
            {
                vehicles.Add(new Vehicle(
                    "España", 
                    "Toyota", 
                    $"Modelo{i}", 
                    2020, 
                    $"ABC{i:000}", 
                    $"{i:00000000000000000}")
                );
            }
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(page: 2, pageSize: 5);

            // Assert
            Assert.Equal(5, results.Count());
            Assert.Equal(15, total);
            Assert.Equal("Modelo6", results.First().Model);
            Assert.Equal("Modelo10", results.Last().Model);
        }

        [Fact]
        public async Task UpdateAsync_ConcurrentUpdate_ThrowsDomainException()
        {
            // Arrange
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123", "11111111111111111");
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();

            var context2 = new AutoLabDbContext(
                new DbContextOptionsBuilder<AutoLabDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options);
            var repository2 = new VehicleRepository(context2);

            var updateVehicle1 = new Vehicle("Francia", "Renault", "Clio", 2021, "DEF456", "22222222222222222");
            var updateVehicle2 = new Vehicle("Italia", "Fiat", "500", 2022, "GHI789", "33333333333333333");
            updateVehicle1.Id = vehicle.Id;
            updateVehicle2.Id = vehicle.Id;

            // Act & Assert
            await _repository.UpdateAsync(updateVehicle1);
            await Assert.ThrowsAsync<DomainException>(() => repository2.UpdateAsync(updateVehicle2));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GetAllAsync_WithInvalidFilters_IgnoresInvalidFilters(string invalidFilter)
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123", "11111111111111111"),
                new Vehicle("Francia", "Renault", "Clio", 2021, "DEF456", "22222222222222222")
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(
                page: 1,
                pageSize: 10,
                country: invalidFilter,
                brand: invalidFilter,
                model: invalidFilter
            );

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Equal(2, total);
        }

        [Fact]
        public async Task GetAllAsync_OrderByYearDescending_ReturnsOrderedResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123", "11111111111111111"),
                new Vehicle("España", "Toyota", "Camry", 2022, "DEF456", "22222222222222222"),
                new Vehicle("España", "Toyota", "Yaris", 2021, "GHI789", "33333333333333333")
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(
                page: 1,
                pageSize: 10,
                orderBy: "year_desc"
            );

            // Assert
            Assert.Equal(3, results.Count());
            Assert.Equal(2022, results.First().Year);
            Assert.Equal(2020, results.Last().Year);
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleLicensePlates_ReturnsMatchingResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123", "11111111111111111"),
                new Vehicle("Francia", "Renault", "Clio", 2021, "XYZ456", "22222222222222222"),
                new Vehicle("Italia", "Fiat", "500", 2022, "DEF789", "33333333333333333")
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            var licensePlates = new[] { "ABC123", "XYZ456" };

            // Act
            var (results, total) = await _repository.GetAllAsync(
                page: 1,
                pageSize: 10,
                licensePlates: licensePlates
            );

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Equal(2, total);
            Assert.All(results, v => Assert.Contains(v.LicensePlate, licensePlates));
        }

        [Fact]
        public async Task GetAllAsync_WithYearRange_ReturnsMatchingResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123", "11111111111111111"),
                new Vehicle("Francia", "Renault", "Clio", 2021, "XYZ456", "22222222222222222"),
                new Vehicle("Italia", "Fiat", "500", 2022, "DEF789", "33333333333333333")
            };
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var (results, total) = await _repository.GetAllAsync(
                page: 1,
                pageSize: 10,
                yearFrom: 2020,
                yearTo: 2021
            );

            // Assert
            Assert.Equal(2, results.Count());
            Assert.All(results, v => Assert.InRange(v.Year, 2020, 2021));
        }
    }
}