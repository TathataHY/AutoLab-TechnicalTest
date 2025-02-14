using AutoLab.API.Controllers;
using AutoLab.Application.DTOs;
using AutoLab.Application.Interfaces;
using AutoLab.Domain.Entities;
using AutoLab.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace AutoLab.API.Tests.Controllers
{
    public class VehiclesControllerTests
    {
        private readonly Mock<IVehicleService> _mockService;
        private readonly VehiclesController _controller;

        public VehiclesControllerTests()
        {
            _mockService = new Mock<IVehicleService>();
            _controller = new VehiclesController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithPaginatedData()
        {
            // Arrange
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            var vehicleDto = new VehicleDto(vehicle);
            var vehicles = new List<VehicleDto> { vehicleDto };

            _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((vehicles, 1));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<VehicleDto>>(okResult.Value);
            Assert.Equal(1, response.Total);
            Assert.Single(response.Items);
        }

        [Fact]
        public async Task GetById_ExistingVehicle_ReturnsOkResult()
        {
            // Arrange
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            var vehicleDto = new VehicleDto(vehicle);

            _mockService.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(vehicleDto);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedVehicle = Assert.IsType<VehicleDto>(okResult.Value);
            Assert.Equal(vehicleDto.Country, returnedVehicle.Country);
        }

        [Fact]
        public async Task Create_ValidVehicle_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            var vehicle = new Vehicle(
                createDto.Country,
                createDto.Brand,
                createDto.Model,
                createDto.Year,
                createDto.LicensePlate,
                createDto.VinCode
            );
            var vehicleDto = new VehicleDto(vehicle);

            _mockService.Setup(s => s.CreateVehicleAsync(It.IsAny<CreateVehicleDto>()))
                .ReturnsAsync(vehicleDto);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(VehiclesController.GetById), createdAtActionResult.ActionName);
        }

        [Fact]
        public async Task Update_ExistingVehicle_ReturnsNoContent()
        {
            // Arrange
            var updateDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            var vehicle = new Vehicle(
                updateDto.Country,
                updateDto.Brand,
                updateDto.Model,
                updateDto.Year,
                updateDto.LicensePlate,
                updateDto.VinCode
            );
            var vehicleDto = new VehicleDto(vehicle);

            _mockService.Setup(s => s.UpdateAsync(1, It.IsAny<CreateVehicleDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(1, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockService.Verify(s => s.UpdateAsync(1, It.IsAny<CreateVehicleDto>()), Times.Once);
        }

        [Fact]
        public async Task GetAll_WithInvalidPageSize_ReturnsBadRequest()
        {
            // Arrange
            _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ThrowsAsync(new DomainException("El tamaño de página debe estar entre 1 y 100"));

            // Act
            var result = await _controller.GetAll(pageSize: 150);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("tamaño de página", badRequestResult.Value.ToString().ToLower());
        }

        [Fact]
        public async Task GetAll_WithInvalidPage_ReturnsBadRequest()
        {
            // Arrange
            _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ThrowsAsync(new DomainException("El número de página debe ser mayor a 0"));

            // Act
            var result = await _controller.GetAll(page: 0);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("página", badRequestResult.Value.ToString().ToLower());
        }

        [Fact]
        public async Task GetAll_WithValidFilters_ReturnsOkResult()
        {
            // Arrange
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567");
            var vehicleDto = new VehicleDto(vehicle);
            var vehicles = new List<VehicleDto> { vehicleDto };

            _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync((vehicles, 1));

            // Act
            var result = await _controller.GetAll(country: "España");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<VehicleDto>>(okResult.Value);
            Assert.Single(response.Items);
            Assert.Equal(1, response.Total);
        }

        [Fact]
        public async Task GetById_NonExistingVehicle_ReturnsNotFound()
        {
            // Arrange
            _mockService.Setup(s => s.GetByIdAsync(999))
                .ThrowsAsync(new DomainException("Vehículo no encontrado"));

            // Act
            var result = await _controller.GetById(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("no encontrado", notFoundResult.Value.ToString().ToLower());
        }

        [Fact]
        public async Task Create_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2030,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            _mockService.Setup(s => s.CreateVehicleAsync(It.IsAny<CreateVehicleDto>()))
                .ThrowsAsync(new DomainException("El año debe estar entre 1900 y 2024"));

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("año", badRequestResult.Value.ToString().ToLower());
        }

        [Fact]
        public async Task Update_NonExistingVehicle_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            _mockService.Setup(s => s.UpdateAsync(999, It.IsAny<CreateVehicleDto>()))
                .ThrowsAsync(new DomainException("Vehículo no encontrado"));

            // Act
            var result = await _controller.Update(999, updateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("no encontrado", notFoundResult.Value.ToString().ToLower());
        }

        [Fact]
        public async Task Create_WithNullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Create(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetAll_WithAllFilters_ReturnsFilteredResults()
        {
            // Arrange
            var vehicles = new List<VehicleDto>
            {
                new VehicleDto(new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"))
            };

            _mockService.Setup(s => s.GetAllAsync(
                1, 10, "España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"))
                .ReturnsAsync((vehicles, 1));

            // Act
            var result = await _controller.GetAll(
                page: 1,
                pageSize: 10,
                country: "España",
                brand: "Toyota",
                model: "Corolla",
                year: 2020,
                licensePlate: "ABC123DE",
                vinCode: "12345678901234567"
            );

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<VehicleDto>>(okResult.Value);
            Assert.Single(response.Items);
            var vehicle = response.Items.First();
            Assert.Equal("España", vehicle.Country);
            Assert.Equal("Toyota", vehicle.Brand);
            Assert.Equal("Corolla", vehicle.Model);
        }

        [Fact]
        public async Task Update_WithNullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Update(1, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task Update_WithInvalidId_ReturnsBadRequest(int id)
        {
            // Arrange
            var updateDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act
            var result = await _controller.Update(id, updateDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange & Act
            var result = await _controller.GetById(-1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_WithEmptyDto_ReturnsBadRequest()
        {
            // Arrange
            var emptyDto = new CreateVehicleDto();

            // Act
            var result = await _controller.Create(emptyDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GetAll_WithInvalidStringFilters_ReturnsOkWithoutFiltering(string invalidFilter)
        {
            // Arrange
            var vehicles = new List<VehicleDto>
            {
                new VehicleDto(new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"))
            };

            _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((vehicles, 1));

            // Act
            var result = await _controller.GetAll(
                country: invalidFilter,
                brand: invalidFilter,
                model: invalidFilter,
                licensePlate: invalidFilter,
                vinCode: invalidFilter
            );

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<VehicleDto>>(okResult.Value);
            Assert.Single(response.Items);
        }

        [Theory]
        [InlineData(1899)]
        [InlineData(2025)]
        public async Task Create_WithYearOutOfRange_ReturnsBadRequest(int invalidYear)
        {
            // Arrange
            var createDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = invalidYear,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            _mockService.Setup(s => s.CreateVehicleAsync(It.IsAny<CreateVehicleDto>()))
                .ThrowsAsync(new DomainException("El año debe estar entre 1900 y 2024"));

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("año", badRequestResult.Value.ToString().ToLower());
        }

        [Fact]
        public async Task Update_WithInvalidVinCode_ReturnsBadRequest()
        {
            // Arrange
            var updateDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "123" // VIN inválido
            };

            _mockService.Setup(s => s.UpdateAsync(1, It.IsAny<CreateVehicleDto>()))
                .ThrowsAsync(new DomainException("El VIN debe tener 17 caracteres"));

            // Act
            var result = await _controller.Update(1, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("vin", badRequestResult.Value.ToString().ToLower());
        }

        [Theory]
        [InlineData("@#$%^")]
        [InlineData("SELECT * FROM")]
        [InlineData("<script>")]
        public async Task GetAll_WithPotentiallyMaliciousInput_HandlesSafely(string maliciousInput)
        {
            // Arrange
            var vehicles = new List<VehicleDto>
            {
                new VehicleDto(new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"))
            };

            _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((vehicles, 1));

            // Act
            var result = await _controller.GetAll(
                country: maliciousInput,
                brand: maliciousInput
            );

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<VehicleDto>>(okResult.Value);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Create_WithExtremelyLongStrings_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateVehicleDto
            {
                Country = new string('x', 1000),
                Brand = new string('x', 1000),
                Model = new string('x', 1000),
                Year = 2020,
                LicensePlate = new string('x', 1000),
                VinCode = new string('x', 1000)
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public async Task GetById_WithExtremeIds_ReturnsBadRequest(int extremeId)
        {
            // Act
            var result = await _controller.GetById(extremeId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("   ")]
        [InlineData("\t\n")]
        [InlineData("!@#$%")]
        public async Task Create_WithInvalidCharactersInStrings_ReturnsBadRequest(string invalidInput)
        {
            // Arrange
            var createDto = new CreateVehicleDto
            {
                Country = invalidInput,
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("inválidos", badRequestResult.Value.ToString().ToLower());
        }

        [Fact]
        public async Task Update_WithConcurrentModification_ReturnsConflict()
        {
            // Arrange
            var updateDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            _mockService.Setup(s => s.UpdateAsync(1, It.IsAny<CreateVehicleDto>()))
                .ThrowsAsync(new DomainException("El vehículo ha sido modificado por otro usuario"));

            // Act
            var result = await _controller.Update(1, updateDto);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Theory]
        [InlineData(int.MaxValue, 100)]    // Página muy grande
        [InlineData(1, int.MaxValue)]      // Tamaño de página muy grande
        public async Task GetAll_WithExtremePageValues_ReturnsBadRequest(int page, int pageSize)
        {
            // Arrange
            _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new DomainException("Valores de paginación inválidos"));

            // Act
            var result = await _controller.GetAll(page: page, pageSize: pageSize);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("", "", "", "", "")]
        [InlineData(" ", " ", " ", " ", " ")]
        [InlineData(null, null, null, null, null)]
        public async Task GetAll_WithEmptyOrNullFilters_ReturnsOkResult(
            string country, string brand, string model,
            string licensePlate, string vinCode)
        {
            // Arrange
            var vehicles = new List<VehicleDto>
            {
                new VehicleDto(new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"))
            };

            _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((vehicles, 1));

            // Act
            var result = await _controller.GetAll(
                country: country,
                brand: brand,
                model: model,
                licensePlate: licensePlate,
                vinCode: vinCode
            );

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<VehicleDto>>(okResult.Value);
            Assert.Single(response.Items);
        }

        [Fact]
        public async Task Update_WithSameData_ReturnsNoContent()
        {
            // Arrange
            var updateDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            _mockService.Setup(s => s.UpdateAsync(1, updateDto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(1, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Theory]
        [InlineData("España", "Toyota", null, 2020, "ABC123DE", "12345678901234567")]
        [InlineData("España", null, "Corolla", 2020, "ABC123DE", "12345678901234567")]
        [InlineData(null, "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567")]
        public async Task Create_WithPartiallyNullData_ReturnsBadRequest(
            string country, string brand, string model,
            int year, string licensePlate, string vinCode)
        {
            // Arrange
            var createDto = new CreateVehicleDto
            {
                Country = country,
                Brand = brand,
                Model = model,
                Year = year,
                LicensePlate = licensePlate,
                VinCode = vinCode
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("requerido", badRequestResult.Value.ToString().ToLower());
        }

        [Theory]
        [InlineData("España", "トヨタ", "コロラ", 2020, "ABC123DE", "12345678901234567")]  // Caracteres Unicode
        [InlineData("España", "Škoda", "Octavia", 2020, "ABC123DE", "12345678901234567")]  // Caracteres con acentos
        public async Task Create_WithUnicodeCharacters_CreatesSuccessfully(
            string country, string brand, string model,
            int year, string licensePlate, string vinCode)
        {
            // Arrange
            var createDto = new CreateVehicleDto
            {
                Country = country,
                Brand = brand,
                Model = model,
                Year = year,
                LicensePlate = licensePlate,
                VinCode = vinCode
            };

            var vehicleDto = new VehicleDto(new Vehicle(country, brand, model, year, licensePlate, vinCode));
            _mockService.Setup(s => s.CreateVehicleAsync(It.IsAny<CreateVehicleDto>()))
                .ReturnsAsync(vehicleDto);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedDto = Assert.IsType<VehicleDto>(createdResult.Value);
            Assert.Equal(brand, returnedDto.Brand);
            Assert.Equal(model, returnedDto.Model);
        }

        [Theory]
        [InlineData("ABC-123")]
        [InlineData("ABC 123")]
        [InlineData("abc123")]
        public async Task Create_WithInvalidLicensePlateFormat_ReturnsBadRequest(string licensePlate)
        {
            // Arrange
            var createDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = licensePlate,
                VinCode = "12345678901234567"
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("formato", badRequestResult.Value.ToString().ToLower());
        }

        [Fact]
        public async Task Create_WithFutureYear_ReturnsBadRequest()
        {
            // Arrange
            var futureYear = DateTime.Now.Year + 1;
            var createDto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = futureYear,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("futuro", badRequestResult.Value.ToString().ToLower());
        }
    }
}