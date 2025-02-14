using AutoLab.Application.DTOs;
using AutoLab.Domain.Exceptions;
using Xunit;

namespace AutoLab.Application.Tests.DTOs
{
    public class CreateVehicleDtoTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("Es")]
        public void Validate_WithInvalidCountry_ThrowsDomainException(string country)
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = country,
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => dto.Validate());
            Assert.Contains("país", exception.Message.ToLower());
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_WithInvalidBrand_ThrowsDomainException(string brand)
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = "España",
                Brand = brand,
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => dto.Validate());
            Assert.Contains("Marca", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_WithInvalidModel_ThrowsDomainException(string model)
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = model,
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => dto.Validate());
            Assert.Contains("Modelo", exception.Message);
        }

        [Theory]
        [InlineData(1899)]
        [InlineData(2030)]
        public void Validate_WithInvalidYear_ThrowsDomainException(int year)
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = year,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => dto.Validate());
            Assert.Contains("año", exception.Message.ToLower());
        }

        [Theory]
        [InlineData("12345")]  // 5 caracteres
        [InlineData("123456789")] // 9 caracteres
        public void Validate_WithInvalidLicensePlate_ThrowsDomainException(string licensePlate)
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = licensePlate,
                VinCode = "12345678901234567"
            };

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => dto.Validate());
            Assert.Contains("patente", exception.Message.ToLower());
        }

        [Theory]
        [InlineData("1234567890123456")] // 16 caracteres
        [InlineData("123456789012345678")] // 18 caracteres
        public void Validate_WithInvalidVinCode_ThrowsDomainException(string vinCode)
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = vinCode
            };

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => dto.Validate());
            Assert.Contains("VIN", exception.Message);
        }

        [Fact]
        public void Validate_WithValidData_DoesNotThrowException()
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
            var exception = Record.Exception(() => dto.Validate());
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("     ")]
        public void Validate_WithEmptyOrWhiteSpaceLicensePlate_ThrowsDomainException(string licensePlate)
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = licensePlate,
                VinCode = "12345678901234567"
            };

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => dto.Validate());
            Assert.Contains("patente", exception.Message.ToLower());
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("     ")]
        public void Validate_WithEmptyOrWhiteSpaceVinCode_ThrowsDomainException(string vinCode)
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = vinCode
            };

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => dto.Validate());
            Assert.Contains("VIN", exception.Message);
        }

        [Theory]
        [InlineData("ABC123")]     // 6 caracteres
        [InlineData("ABC123D")]    // 7 caracteres
        [InlineData("ABC123DE")]   // 8 caracteres
        public void Validate_WithValidLicensePlate_ValidatesSuccessfully(string licensePlate)
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = licensePlate,
                VinCode = "12345678901234567"
            };

            // Act & Assert
            var exception = Record.Exception(() => dto.Validate());
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("España", "Toyota", "Corolla")]
        [InlineData("Portugal", "Honda", "Civic")]
        [InlineData("Francia", "Renault", "Clio")]
        public void Validate_WithDifferentValidBrands_ValidatesSuccessfully(string country, string brand, string model)
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = country,
                Brand = brand,
                Model = model,
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act & Assert
            var exception = Record.Exception(() => dto.Validate());
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(1900)]
        [InlineData(2020)]
        [InlineData(2024)]
        public void Validate_WithValidYears_ValidatesSuccessfully(int year)
        {
            // Arrange
            var dto = new CreateVehicleDto
            {
                Country = "España",
                Brand = "Toyota",
                Model = "Corolla",
                Year = year,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act & Assert
            var exception = Record.Exception(() => dto.Validate());
            Assert.Null(exception);
        }
    }
} 