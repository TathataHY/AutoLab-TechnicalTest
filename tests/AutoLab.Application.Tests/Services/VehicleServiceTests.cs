using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoLab.Application.DTOs;
using AutoLab.Application.Services;
using AutoLab.Domain.Entities;
using AutoLab.Domain.Exceptions;
using AutoLab.Domain.Interfaces;
using Moq;
using Xunit;

namespace AutoLab.Application.Tests.Services
{
    public class VehicleServiceTests
    {
        private readonly Mock<IVehicleRepository> _mockRepository;
        private readonly VehicleService _service;

        public VehicleServiceTests()
        {
            _mockRepository = new Mock<IVehicleRepository>();
            var mockCountryService = new Mock<ICountryService>();
            _service = new VehicleService(_mockRepository.Object, mockCountryService.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingVehicle_ReturnsVehicleDto()
        {
            // Arrange
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vehicle);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(vehicle.Country, result.Country);
            Assert.Equal(vehicle.Brand, result.Brand);
            Assert.Equal(vehicle.Model, result.Model);
            Assert.Equal(vehicle.Year, result.Year);
            Assert.Equal(vehicle.LicensePlate, result.LicensePlate);
            Assert.Equal(vehicle.VinCode, result.VinCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetByIdAsync_InvalidId_ThrowsDomainException(int id)
        {
            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _service.GetByIdAsync(id));
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingVehicle_ThrowsDomainException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ThrowsAsync(new DomainException("Vehículo no encontrado"));

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _service.GetByIdAsync(1));
        }

        [Fact]
        public async Task CreateVehicleAsync_ValidData_ReturnsVehicleDto()
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Vehicle>()))
                .ReturnsAsync((Vehicle v) => v);

            // Act
            var result = await _service.CreateVehicleAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Country, result.Country);
            Assert.Equal(dto.Brand, result.Brand);
            Assert.Equal(dto.Model, result.Model);
            Assert.Equal(dto.Year, result.Year);
            Assert.Equal(dto.LicensePlate, result.LicensePlate);
            Assert.Equal(dto.VinCode, result.VinCode);
        }

        [Fact]
        public async Task CreateVehicleAsync_InvalidData_ThrowsDomainException()
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = "",  // País vacío
                Brand = "Toyota",
                Model = "Corolla",
                Year = 1800,  // Año inválido
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _service.CreateVehicleAsync(dto));
        }

        [Fact]
        public async Task GetAllAsync_WithFilters_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                "España",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles, 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, country: "España");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            Assert.All(items, item => Assert.Equal("España", item.Country));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(101)]
        public async Task GetAllAsync_InvalidPageSize_ThrowsDomainException(int pageSize)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.GetAllAsync(1, pageSize));
            Assert.Contains("tamaño de página", exception.Message.ToLower());
        }

        [Fact]
        public async Task UpdateAsync_ExistingVehicle_UpdatesSuccessfully()
        {
            // Arrange
            var existingVehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingVehicle);

            var updateDto = new CreateVehicleDto
            {
                Country = "Francia",
                Brand = "Renault",
                Model = "Clio",
                Year = 2021,
                LicensePlate = "XYZ789GH",
                VinCode = "98765432109876543"
            };

            // Act
            await _service.UpdateAsync(1, updateDto);

            // Assert
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Vehicle>(v =>
                v.Country == updateDto.Country &&
                v.Brand == updateDto.Brand &&
                v.Model == updateDto.Model &&
                v.Year == updateDto.Year &&
                v.LicensePlate == updateDto.LicensePlate &&
                v.VinCode == updateDto.VinCode
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingVehicle_ThrowsDomainException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ThrowsAsync(new DomainException("Vehículo no encontrado"));

            var updateDto = new CreateVehicleDto
            {
                Country = "Francia",
                Brand = "Renault",
                Model = "Clio",
                Year = 2021,
                LicensePlate = "XYZ789GH",
                VinCode = "98765432109876543"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _service.UpdateAsync(1, updateDto));
        }

        [Fact]
        public async Task DeleteAsync_ExistingVehicle_DeletesSuccessfully()
        {
            // Arrange
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vehicle);

            // Act
            await _service.DeleteAsync(1);

            // Assert
            _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingVehicle_ThrowsDomainException()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteAsync(1))
                .ThrowsAsync(new DomainException("Vehículo no encontrado"));

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _service.DeleteAsync(1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetAllAsync_InvalidPage_ThrowsDomainException(int page)
        {
            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() =>
                _service.GetAllAsync(page, 10));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task UpdateAsync_InvalidId_ThrowsDomainException(int id)
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _service.UpdateAsync(id, dto));
        }

        [Fact]
        public async Task UpdateAsync_InvalidData_ThrowsDomainException()
        {
            // Arrange
            var existingVehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingVehicle);

            var updateDto = new CreateVehicleDto
            {
                Country = "",  // País vacío
                Brand = "Toyota",
                Model = "Corolla",
                Year = 1800,  // Año inválido
                LicensePlate = "123", // Placa inválida
                VinCode = "123" // VIN inválido
            };

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _service.UpdateAsync(1, updateDto));
        }

        [Fact]
        public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((new List<Vehicle>(), 0));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10);

            // Assert
            Assert.Empty(items);
            Assert.Equal(0, total);
        }

        [Fact]
        public async Task GetAllAsync_WithYearFilter_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles, 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, year: 2020);

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            Assert.Equal(2020, items.First().Year);
        }

        [Fact]
        public async Task UpdateAsync_InvalidVinCode_ThrowsDomainException()
        {
            // Arrange
            var existingVehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingVehicle);

            var updateDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "123" // VIN inválido
            };

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _service.UpdateAsync(1, updateDto));
        }

        [Fact]
        public async Task GetAllAsync_WithBrandFilter_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Renault", "Clio", 2020, "XYZ789GH", "98765432109876543")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v => v.Brand == "Toyota").ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, brand: "Toyota");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            Assert.Equal("Toyota", items.First().Brand);
        }

        [Fact]
        public async Task GetAllAsync_WithLicensePlateFilter_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota", "Corolla", 2020, "XYZ789GH", "98765432109876543")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v => v.LicensePlate == "ABC123DE").ToList(), 1));


            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, licensePlate: "ABC123DE");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            Assert.Equal("ABC123DE", items.First().LicensePlate);
        }

        [Fact]
        public async Task GetAllAsync_WithVinCodeFilter_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota", "Corolla", 2020, "XYZ789GH", "98765432109876543")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v => v.VinCode == "12345678901234567").ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, vinCode: "12345678901234567");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            Assert.Equal("12345678901234567", items.First().VinCode);
        }

        [Fact]
        public async Task UpdateAsync_InvalidVehicleData_ThrowsDomainException()
        {
            // Arrange
            var existingVehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingVehicle);

            var updateDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2026, // Año inválido
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _service.UpdateAsync(1, updateDto));
        }

        [Fact]
        public async Task GetAllAsync_WithModelFilter_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota", "Camry", 2020, "XYZ789GH", "98765432109876543")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v => v.Model == "Corolla").ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, model: "Corolla");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            Assert.Equal("Corolla", items.First().Model);
        }

        [Fact]
        public async Task GetAllAsync_EmptyFilters_ReturnsAllResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("Francia", "Renault", "Clio", 2021, "XYZ789GH", "98765432109876543")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles, 2));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10);

            // Assert
            Assert.Equal(2, items.Count());
            Assert.Equal(2, total);
        }

        [Fact]
        public async Task UpdateAsync_InvalidVehicleYear_ThrowsDomainException()
        {
            // Arrange
            var existingVehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingVehicle);

            var updateDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2030, // Año futuro inválido
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _service.UpdateAsync(1, updateDto));
        }

        [Fact]
        public async Task GetAllAsync_WithCountryAndBrandFilters_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Renault", "Clio", 2021, "XYZ789GH", "98765432109876543"),
                new Vehicle("Francia", "Toyota", "Yaris", 2021, "DEF456GH", "45678901234567890")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.Country == "España" && v.Brand == "Toyota").ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, "España", "Toyota");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            var item = items.First();
            Assert.Equal("España", item.Country);
            Assert.Equal("Toyota", item.Brand);
            Assert.Equal("Corolla", item.Model);
        }

        [Fact]
        public async Task GetAllAsync_WithYearAndLicensePlateFilters_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota", "Corolla", 2020, "XYZ789GH", "98765432109876543")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.Year == 2020 && v.LicensePlate.Contains("ABC")).ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, year: 2020, licensePlate: "ABC");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            var item = items.First();
            Assert.Equal(2020, item.Year);
            Assert.Equal("ABC123DE", item.LicensePlate);
        }

        [Fact]
        public async Task GetAllAsync_WithCountryAndYearFilters_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Renault", "Clio", 2021, "XYZ789GH", "98765432109876543"),
                new Vehicle("Francia", "Toyota", "Yaris", 2020, "DEF456GH", "45678901234567890")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.Country == "España" && v.Year == 2020).ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, country: "España", year: 2020);

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            var item = items.First();
            Assert.Equal("España", item.Country);
            Assert.Equal(2020, item.Year);
        }

        [Fact]
        public async Task GetAllAsync_WithModelAndVinCodeFilters_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota", "Corolla", 2021, "XYZ789GH", "98765432109876543")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.Model == "Corolla" &&
                    v.VinCode == "12345678901234567").ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, model: "Corolla", vinCode: "12345678901234567");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            var item = items.First();
            Assert.Equal("Corolla", item.Model);
            Assert.Equal("12345678901234567", item.VinCode);
        }

        [Fact]
        public async Task GetAllAsync_WithInvalidPageSize_ThrowsDomainException()
        {
            // Arrange
            var pageSize = 150; // Tamaño de página inválido

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() =>
                _service.GetAllAsync(1, pageSize));
        }

        [Fact]
        public async Task GetAllAsync_WithCountryAndModelAndYearFilters_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota", "Corolla", 2021, "XYZ789GH", "98765432109876543"),
                new Vehicle("Francia", "Toyota", "Corolla", 2020, "DEF456GH", "45678901234567890")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.Country == "España" &&
                    v.Model == "Corolla" &&
                    v.Year == 2020).ToList(), 1));
            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, country: "España", model: "Corolla", year: 2020);

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            var item = items.First();
            Assert.Equal("España", item.Country);
            Assert.Equal("Corolla", item.Model);
            Assert.Equal(2020, item.Year);
        }

        [Fact]
        public async Task GetAllAsync_WithBrandAndLicensePlateAndVinCodeFilters_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("Francia", "Toyota", "Yaris", 2020, "ABC123DE", "98765432109876543")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
    It.IsAny<int>(),
    It.IsAny<int>(),
    It.IsAny<string>(),
    It.IsAny<string>(),
    It.IsAny<string>(),
    It.IsAny<int?>(),
    It.IsAny<string>(),
    It.IsAny<string>(),
    It.IsAny<string>(),
    It.IsAny<int?>(),
    It.IsAny<int?>(),
    It.IsAny<string[]>()))
    .ReturnsAsync((vehicles.Where(v =>
        v.Brand == "Toyota" &&
        v.LicensePlate == "ABC123DE" &&
        v.VinCode == "12345678901234567").ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, brand: "Toyota",
                licensePlate: "ABC123DE",
                vinCode: "12345678901234567");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            var item = items.First();
            Assert.Equal("Toyota", item.Brand);
            Assert.Equal("ABC123DE", item.LicensePlate);
            Assert.Equal("12345678901234567", item.VinCode);
        }

        [Fact]
        public async Task GetAllAsync_WithAllFilters_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("Francia", "Renault", "Clio", 2021, "XYZ789GH", "98765432109876543")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.Country == "España" &&
                    v.Brand == "Toyota" &&
                    v.Model == "Corolla" &&
                    v.Year == 2020 &&
                    v.LicensePlate == "ABC123DE" &&
                    v.VinCode == "12345678901234567").ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, "España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            var item = items.First();
            Assert.Equal("España", item.Country);
            Assert.Equal("Toyota", item.Brand);
            Assert.Equal("Corolla", item.Model);
            Assert.Equal(2020, item.Year);
            Assert.Equal("ABC123DE", item.LicensePlate);
            Assert.Equal("12345678901234567", item.VinCode);
        }

        [Fact]
        public async Task GetAllAsync_WithInvalidPage_ThrowsDomainException()
        {
            // Arrange
            var invalidPage = -1;

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() =>
                _service.GetAllAsync(invalidPage, 10));
        }

        [Fact]
        public async Task GetAllAsync_WithYearRangeFilter_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota", "Corolla", 2021, "XYZ789GH", "98765432109876543"),
                new Vehicle("España", "Toyota", "Corolla", 2022, "DEF456GH", "45678901234567890")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v => v.Year == 2021).ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, year: 2021);

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            Assert.Equal(2021, items.First().Year);
        }

        [Fact]
        public async Task GetAllAsync_WithPartialLicensePlate_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota", "Corolla", 2020, "XYZ789GH", "98765432109876543"),
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC456GH", "45678901234567890")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.LicensePlate.Contains("ABC")).ToList(), 2));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, licensePlate: "ABC");

            // Assert
            Assert.Equal(2, items.Count());
            Assert.Equal(2, total);
            Assert.All(items, item => Assert.Contains("ABC", item.LicensePlate));
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyResults_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((new List<Vehicle>(), 0));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10);

            // Assert
            Assert.Empty(items);
            Assert.Equal(0, total);
        }

        [Fact]
        public async Task GetAllAsync_WithPartialVinCode_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota", "Corolla", 2020, "XYZ789GH", "98765432109876543"),
                new Vehicle("España", "Toyota", "Corolla", 2020, "DEF456GH", "12345678909999999")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.VinCode.Contains("123456789012")).ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, vinCode: "123456789012");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            Assert.StartsWith("123456789012", items.First().VinCode);
        }

        [Fact]
        public async Task GetAllAsync_WithBrandAndModelAndYearFilters_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota", "Camry", 2020, "XYZ789GH", "98765432109876543"),
                new Vehicle("España", "Renault", "Corolla", 2020, "DEF456GH", "45678901234567890")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.Brand == "Toyota" &&
                    v.Model == "Corolla" &&
                    v.Year == 2020).ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, brand: "Toyota", model: "Corolla", year: 2020);

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            var item = items.First();
            Assert.Equal("Toyota", item.Brand);
            Assert.Equal("Corolla", item.Model);
            Assert.Equal(2020, item.Year);
        }

        [Fact]
        public async Task GetAllAsync_WithCountryAndLicensePlateAndVinCodeFilters_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("Francia", "Toyota", "Corolla", 2020, "ABC123DE", "98765432109876543")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                "España",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.Country == "España" &&
                    v.LicensePlate == "ABC123DE" &&
                    v.VinCode == "12345678901234567").ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, country: "España",
                licensePlate: "ABC123DE",
                vinCode: "12345678901234567");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            var item = items.First();
            Assert.Equal("España", item.Country);
            Assert.Equal("ABC123DE", item.LicensePlate);
            Assert.Equal("12345678901234567", item.VinCode);
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleCountries_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("Francia", "Toyota", "Corolla", 2020, "XYZ789GH", "98765432109876543"),
                new Vehicle("Italia", "Toyota", "Corolla", 2020, "DEF456GH", "45678901234567890")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                "España",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.Country == "España").ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, country: "España");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            Assert.All(items, item => Assert.Equal("España", item.Country));
        }

        [Fact]
        public async Task GetAllAsync_WithExactMatchFilters_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota Camry", "Corolla", 2020, "XYZ789GH", "98765432109876543")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.Brand == "Toyota").ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, brand: "Toyota");

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            Assert.Equal("Toyota", items.First().Brand);
            Assert.DoesNotContain(items, v => v.Brand == "Toyota Camry");
        }

        [Fact]
        public async Task GetAllAsync_WithPartialBrandMatch_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota Corolla", "Sedan", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota Camry", "Sedan", 2020, "XYZ789GH", "98765432109876543"),
                new Vehicle("España", "Honda Civic", "Sedan", 2020, "DEF456GH", "45678901234567890")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.Brand.Contains("Toyota")).ToList(), 2));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, brand: "Toyota");

            // Assert
            Assert.Equal(2, items.Count());
            Assert.Equal(2, total);
            Assert.All(items, item => Assert.Contains("Toyota", item.Brand));
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleFiltersAndPagination_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota", "Camry", 2020, "XYZ789GH", "98765432109876543"),
                new Vehicle("Francia", "Toyota", "Corolla", 2021, "DEF456GH", "45678901234567890")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                2, 5,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.Country == "España" &&
                    v.Brand == "Toyota" &&
                    v.Year == 2020).ToList(), 2));

            // Act
            var (items, total) = await _service.GetAllAsync(2, 5, country: "España", brand: "Toyota", year: 2020);

            // Assert
            Assert.Equal(2, items.Count());
            Assert.Equal(2, total);
            Assert.All(items, item =>
            {
                Assert.Equal("España", item.Country);
                Assert.Equal("Toyota", item.Brand);
                Assert.Equal(2020, item.Year);
            });
        }

        [Fact]
        public async Task GetAllAsync_WithCombinedModelAndYearFilter_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("España", "Toyota", "Corolla", 2021, "XYZ789GH", "98765432109876543"),
                new Vehicle("España", "Toyota", "Camry", 2020, "DEF456GH", "45678901234567890")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Where(v =>
                    v.Model == "Corolla" &&
                    v.Year == 2020).ToList(), 1));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10, model: "Corolla", year: 2020);

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            var item = items.First();
            Assert.Equal("Corolla", item.Model);
            Assert.Equal(2020, item.Year);
        }

        [Fact]
        public async Task GetAllAsync_WithPaginationLimits_ReturnsCorrectPage()
        {
            // Arrange
            var vehicles = new List<Vehicle>();
            for (int i = 1; i <= 15; i++)
            {
                vehicles.Add(new Vehicle(
                    "España",
                    "Toyota",
                    "Corolla",
                    2020,
                    $"ABC{i:D3}DE",
                    $"1234567890123{i:D3}"));
            }

            _mockRepository.Setup(r => r.GetAllAsync(
                2, 5,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles.Skip(5).Take(5).ToList(), 15));

            // Act
            var (items, total) = await _service.GetAllAsync(2, 5);

            // Assert
            Assert.Equal(5, items.Count());
            Assert.Equal(15, total);
            Assert.Equal("ABC006DE", items.First().LicensePlate);
            Assert.Equal("ABC010DE", items.Last().LicensePlate);
        }

        [Fact]
        public async Task GetAllAsync_WithNullFilters_ReturnsAllResults()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"),
                new Vehicle("Francia", "Renault", "Clio", 2021, "XYZ789GH", "98765432109876543"),
                new Vehicle("Italia", "Fiat", "500", 2022, "DEF456GH", "45678901234567890")
            };

            _mockRepository.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string[]>()))
                .ReturnsAsync((vehicles, 3));

            // Act
            var (items, total) = await _service.GetAllAsync(1, 10);

            // Assert
            Assert.Equal(3, items.Count());
            Assert.Equal(3, total);
            Assert.Contains(items, v => v.Country == "España");
            Assert.Contains(items, v => v.Country == "Francia");
            Assert.Contains(items, v => v.Country == "Italia");
        }
    }
}

