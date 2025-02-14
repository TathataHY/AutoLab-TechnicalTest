using AutoLab.Domain.Entities;
using AutoLab.Domain.Exceptions;
using Xunit;
using System.Linq;

namespace AutoLab.Domain.Tests.Entities
{
    public class VehicleTests
    {
        [Theory]
        [InlineData(1899)]
        [InlineData(2030)]
        public void Vehicle_WithInvalidYear_ThrowsDomainException(int year)
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<DomainException>(() => 
                new Vehicle("España", "Toyota", "Corolla", year, "ABC123DE", "12345678901234567"));
            
            Assert.Contains("año", exception.Message);
        }

        [Theory]
        [InlineData("12345")]  // 5 caracteres
        [InlineData("123456789")] // 9 caracteres
        public void Vehicle_WithInvalidLicensePlate_ThrowsDomainException(string licensePlate)
        {
            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => 
                new Vehicle("España", "Toyota", "Corolla", 2020, licensePlate, "12345678901234567"));
            
            Assert.Contains("patente", exception.Message);
        }

        [Theory]
        [InlineData("1234567890123")]
        [InlineData("123456789012345678")]
        public void Vehicle_WithInvalidVinCode_ThrowsDomainException(string vinCode)
        {
            var exception = Assert.Throws<DomainException>(() => 
                new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", vinCode));
            
            Assert.Contains("VIN", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Vehicle_WithEmptyCountry_ThrowsDomainException(string country)
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<DomainException>(() => 
                new Vehicle(country, "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567"));
            
            Assert.Contains("Country", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Vehicle_WithEmptyBrand_ThrowsDomainException(string brand)
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<DomainException>(() => 
                new Vehicle("España", brand, "Corolla", 2020, "ABC123DE", "12345678901234567"));
            
            Assert.Contains("Brand", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Vehicle_WithEmptyModel_ThrowsDomainException(string model)
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<DomainException>(() => 
                new Vehicle("España", "Toyota", model, 2020, "ABC123DE", "12345678901234567"));
            
            Assert.Contains("Model", exception.Message);
        }

        [Theory]
        [InlineData("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567")]
        [InlineData("Portugal", "Honda", "Civic", 2021, "XYZ789GH", "98765432109876543")]
        public void Vehicle_WithValidData_CreatesInstanceSuccessfully(
            string country, string brand, string model, 
            int year, string licensePlate, string vinCode)
        {
            // Arrange & Act
            var vehicle = new Vehicle(country, brand, model, year, licensePlate, vinCode);

            // Assert
            Assert.Equal(country, vehicle.Country);
            Assert.Equal(brand, vehicle.Brand);
            Assert.Equal(model, vehicle.Model);
            Assert.Equal(year, vehicle.Year);
            Assert.Equal(licensePlate, vehicle.LicensePlate);
            Assert.Equal(vinCode, vehicle.VinCode);
        }

        [Theory]
        [InlineData("ABC123")]  // 6 caracteres
        [InlineData("ABC123D")] // 7 caracteres
        [InlineData("ABC123DE")] // 8 caracteres
        public void Vehicle_WithValidLicensePlate_CreatesInstanceSuccessfully(string licensePlate)
        {
            // Arrange & Act
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, licensePlate, "12345678901234567");

            // Assert
            Assert.Equal(licensePlate, vehicle.LicensePlate);
        }

        [Theory]
        [InlineData(1900)]
        [InlineData(2020)]
        [InlineData(2024)]
        public void Vehicle_WithValidYear_CreatesInstanceSuccessfully(int year)
        {
            // Arrange & Act
            var vehicle = new Vehicle("España", "Toyota", "Corolla", year, "ABC123DE", "12345678901234567");

            // Assert
            Assert.Equal(year, vehicle.Year);
        }

        [Theory]
        [InlineData("12345678901234567")]  // Exactamente 17 caracteres
        public void Vehicle_WithValidVinCode_CreatesInstanceSuccessfully(string vinCode)
        {
            // Arrange & Act
            var vehicle = new Vehicle("España", "Toyota", "Corolla", 2020, "ABC123DE", vinCode);

            // Assert
            Assert.Equal(vinCode, vehicle.VinCode);
        }

        [Theory]
        [InlineData("España", "Toyota", "Corolla", 2020, "ABC123DE", "12345678901234567")]
        [InlineData("Francia", "Renault", "Clio", 2021, "XYZ789GH", "98765432109876543")]
        public void Vehicle_WithValidData_PropertiesAreImmutable(
            string country, string brand, string model, 
            int year, string licensePlate, string vinCode)
        {
            // Arrange & Act
            var vehicle = new Vehicle(country, brand, model, year, licensePlate, vinCode);

            // Assert
            var properties = typeof(Vehicle).GetProperties()
                .Where(p => p.Name != "Id");  // Excluimos Id de la verificación
            
            foreach (var prop in properties)
            {
                var setter = prop.GetSetMethod(true); // true para incluir métodos privados
                Assert.True(setter == null || !setter.IsPublic, 
                    $"La propiedad {prop.Name} debe ser inmutable");
            }
        }
    }
} 