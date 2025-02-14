using System.Net;
using System.Net.Http.Json;
using Xunit;
using AutoLab.Application.DTOs;
using AutoLab.API;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using AutoLab.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AutoLab.IntegrationTests
{
    public class RequirementsTests : IntegrationTestBase
    {
        public RequirementsTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Requirement1_CreateVehicle_WithValidData_ReturnsCreated()
        {
            // Arrange
            await ResetDatabase();
            
            var vehicle = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/vehicles", vehicle);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdVehicle = await response.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(createdVehicle);
            Assert.Equal(vehicle.Brand, createdVehicle.Brand);
            Assert.Equal(vehicle.Model, createdVehicle.Model);
        }

        [Theory]
        [InlineData(2030)] // Año futuro
        [InlineData(1899)] // Año muy antiguo
        public async Task Requirement2_CreateVehicle_WithInvalidYear_ReturnsBadRequest(int invalidYear)
        {
            // Arrange
            var vehicle = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Corolla",
                Year = invalidYear,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/vehicles", vehicle);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var error = await response.Content.ReadAsStringAsync();
            Assert.Contains("año", error.ToLower());
        }

        [Theory]
        [InlineData("123")] // Muy corta
        [InlineData("ABC123456789")] // Muy larga
        [InlineData("!@#123DE")] // Caracteres especiales
        [InlineData("ABC 123")] // Espacios no permitidos
        public async Task CreateVehicle_WithInvalidLicensePlate_ReturnsBadRequest(string invalidPlate)
        {
            // Arrange
            await ResetDatabase();
            
            var vehicle = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = invalidPlate,
                VinCode = "12345678901234567"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/vehicles", vehicle);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var error = await response.Content.ReadAsStringAsync();
            Assert.Contains("patente", error.ToLower());
        }

        [Theory]
        [InlineData("")]  // Vacío
        [InlineData(" ")] // Espacios en blanco
        [InlineData("AB CD123")] // Espacios en medio
        [InlineData("123-ABC")] // Guiones
        [InlineData("ABC_123")] // Guión bajo
        public async Task CreateVehicle_WithInvalidLicensePlateFormat_ReturnsBadRequest(string invalidPlate)
        {
            // Arrange
            await ResetDatabase();
            
            var vehicle = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = invalidPlate,
                VinCode = "12345678901234567"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/vehicles", vehicle);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var error = await response.Content.ReadAsStringAsync();
            
            // Verificamos que el error sea sobre el formato o que sea requerido
            Assert.True(
                error.ToLower().Contains("patente") || 
                error.ToLower().Contains("formato") || 
                error.ToLower().Contains("requerido"),
                $"El mensaje de error debería contener 'patente', 'formato' o 'requerido'. Mensaje actual: {error}"
            );
        }

        [Theory]
        [InlineData("123")] // Muy corto
        [InlineData("12345678901234567890")] // Muy largo
        [InlineData("ABC@#$%^&*()")] // Caracteres especiales
        public async Task CreateVehicle_WithInvalidVinCode_ReturnsBadRequest(string invalidVin)
        {
            // Arrange
            await ResetDatabase();
            
            var vehicle = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = invalidVin
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/vehicles", vehicle);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var error = await response.Content.ReadFromJsonAsync<object>();
            Assert.Contains("vin", error.ToString().ToLower());
        }

        [Fact]
        public async Task GetVehicleById_ExistingVehicle_ReturnsOk()
        {
            // Arrange
            await ResetDatabase();
            var createDto = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/vehicles", createDto);
            var createdVehicle = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();

            // Act
            var response = await _client.GetAsync($"/api/vehicles/{createdVehicle.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var vehicle = await response.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(vehicle);
            Assert.Equal(createDto.Brand, vehicle.Brand);
            Assert.Equal(createDto.Model, vehicle.Model);
        }

        [Fact]
        public async Task CreateVehicle_WithInvalidCharacters_ReturnsBadRequest()
        {
            // Arrange
            var vehicle = new CreateVehicleDto
            {
                Country = "Spain!@#",  // Caracteres especiales
                Brand = "Toyota<script>",  // Intento de XSS
                Model = "Corolla;DROP TABLE",  // Intento de SQL Injection
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/vehicles", vehicle);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var error = await response.Content.ReadAsStringAsync();
            Assert.Contains("caracteres inválidos", error.ToLower());
        }

        [Fact]
        public async Task CreateVehicle_WithExtremelyLongStrings_ReturnsBadRequest()
        {
            // Arrange
            var longString = new string('A', 1000);
            var vehicle = new CreateVehicleDto
            {
                Country = longString,
                Brand = longString,
                Model = longString,
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/vehicles", vehicle);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var error = await response.Content.ReadAsStringAsync();
            Assert.Contains("longitud máxima", error.ToLower());
        }

        [Fact]
        public async Task VehicleCRUD_FullLifecycle_Success()
        {
            // Arrange - Limpiar BD
            await ResetDatabase();
            
            // Create
            var createDto = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };
            
            var createResponse = await _client.PostAsJsonAsync("/api/vehicles", createDto);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var createdVehicle = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.NotNull(createdVehicle);
            
            // Read
            var getResponse = await _client.GetAsync($"/api/vehicles/{createdVehicle.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var retrievedVehicle = await getResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.Equal(createDto.Brand, retrievedVehicle.Brand);
            
            // Update - Mantenemos todos los campos requeridos
            var updateDto = new CreateVehicleDto
            {
                Country = retrievedVehicle.Country,
                Brand = "Honda",
                Model = "Civic",
                Year = 2021,
                LicensePlate = retrievedVehicle.LicensePlate,
                VinCode = retrievedVehicle.VinCode
            };
            
            // Validar el DTO antes de enviarlo
            updateDto.Validate();
            
            var jsonContent = JsonContent.Create(updateDto);
            var updateResponse = await _client.PutAsync($"/api/vehicles/{createdVehicle.Id}", jsonContent);
            
            if (updateResponse.StatusCode != HttpStatusCode.NoContent)
            {   
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                var fullError = $"Update failed with status {updateResponse.StatusCode}. " +
                               $"Request content: {await jsonContent.ReadAsStringAsync()}. " +
                               $"Error: {errorContent}";
                Assert.True(false, fullError);
            }
            
            // Verify Update
            var verifyResponse = await _client.GetAsync($"/api/vehicles/{createdVehicle.Id}");
            Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
            var updatedVehicle = await verifyResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.Equal(updateDto.Brand, updatedVehicle.Brand);
            Assert.Equal(updateDto.Model, updatedVehicle.Model);
            
            // Delete
            var deleteResponse = await _client.DeleteAsync($"/api/vehicles/{createdVehicle.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            
            // Verify Delete
            var notFoundResponse = await _client.GetAsync($"/api/vehicles/{createdVehicle.Id}");
            Assert.Equal(HttpStatusCode.NotFound, notFoundResponse.StatusCode);
        }

        [Fact]
        public async Task UpdateVehicle_WithValidData_ReturnsNoContent()
        {
            // Arrange
            await ResetDatabase();
            
            // Primero creamos un vehículo
            var createDto = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };
            
            var createResponse = await _client.PostAsJsonAsync("/api/vehicles", createDto);
            var createdVehicle = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();
            
            // Luego lo actualizamos
            var updateDto = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Camry",
                Year = 2021,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };

            // Act
            var updateResponse = await _client.PutAsJsonAsync($"/api/vehicles/{createdVehicle.Id}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
            
            // Verificamos que se actualizó correctamente
            var getResponse = await _client.GetAsync($"/api/vehicles/{createdVehicle.Id}");
            var updatedVehicle = await getResponse.Content.ReadFromJsonAsync<VehicleDto>();
            Assert.Equal("Camry", updatedVehicle.Model);
            Assert.Equal(2021, updatedVehicle.Year);
        }

        [Fact]
        public async Task GetVehicles_WithPagination_ReturnsCorrectItems()
        {
            // Arrange
            await ResetDatabase();
            
            // Crear 5 vehículos
            for (int i = 1; i <= 5; i++)
            {
                var createDto = new CreateVehicleDto
                {
                    Country = "Spain",
                    Brand = $"Brand{i}",
                    Model = $"Model{i}",
                    Year = 2020,
                    LicensePlate = $"ABC{i}23DE",
                    VinCode = $"1234567890123456{i}"
                };
                await _client.PostAsJsonAsync("/api/vehicles", createDto);
            }

            // Act
            var response = await _client.GetAsync("/api/vehicles?pageSize=2&page=2");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<VehicleDto>>();
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(5, result.Total);
        }

        [Fact]
        public async Task GetVehicles_WithFiltersAndPagination_ReturnsCorrectResults()
        {
            // Arrange
            await ResetDatabase();
            
            var brands = new[] { "Toyota", "Honda", "Toyota", "Nissan", "Toyota" };
            var counter = 1;
            foreach (var brand in brands)
            {
                var vehicle = new CreateVehicleDto
                {
                    Country = "Spain",
                    Brand = brand,
                    Model = "TestModel",
                    Year = 2020,
                    LicensePlate = $"ABC{counter:000}DE",  // Usando un contador incremental
                    VinCode = $"12345678901234{counter:000}"
                };
                counter++;
                
                var createResponse = await _client.PostAsJsonAsync("/api/vehicles", vehicle);
                Console.WriteLine($"Creating vehicle {brand} with plate {vehicle.LicensePlate}: {createResponse.StatusCode}");
                
                if (!createResponse.IsSuccessStatusCode)
                {
                    var error = await createResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error creating vehicle: {error}");
                }
                
                Assert.True(createResponse.IsSuccessStatusCode, $"Failed to create vehicle with brand {brand}");
            }

            // Verificar el estado actual de la base de datos
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AutoLabDbContext>();
            var count = await context.Vehicles.CountAsync();
            Console.WriteLine($"Total vehicles in database: {count}");
            var toyotaCount = await context.Vehicles.CountAsync(v => v.Brand == "Toyota");
            Console.WriteLine($"Total Toyota vehicles: {toyotaCount}");

            // Act
            var getResponse = await _client.GetAsync("/api/vehicles?brand=Toyota&pageSize=2&page=1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var result = await getResponse.Content.ReadFromJsonAsync<PaginatedResponse<VehicleDto>>();
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(3, result.Total);
            Assert.All(result.Items, item => Assert.Equal("Toyota", item.Brand));
        }

        [Theory]
        [InlineData("ABC123", "licensePlate")]
        [InlineData("12345678901234567", "vinCode")]
        public async Task SearchVehicles_ByLicensePlateOrVin_ReturnsMatchingVehicles(string searchTerm, string searchType)
        {
            // Arrange
            await ResetDatabase();
            
            var vehicle = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };
            await _client.PostAsJsonAsync("/api/vehicles", vehicle);

            // Act
            var response = await _client.GetAsync($"/api/vehicles/search?term={searchTerm}&type={searchType}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = await response.Content.ReadFromJsonAsync<List<VehicleDto>>();
            Assert.NotEmpty(results);
            Assert.Contains(results, v => 
                (searchType == "licensePlate" && v.LicensePlate.Contains(searchTerm)) ||
                (searchType == "vinCode" && v.VinCode.Contains(searchTerm)));
        }

        [Fact]
        public async Task CreateVehicle_WithDuplicateLicensePlate_ReturnsBadRequest()
        {
            // Arrange
            await ResetDatabase();
            
            var vehicle1 = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "ABC123DE",
                VinCode = "12345678901234567"
            };
            
            await _client.PostAsJsonAsync("/api/vehicles", vehicle1);
            
            var vehicle2 = new CreateVehicleDto
            {
                Country = "Spain",
                Brand = "Honda",
                Model = "Civic",
                Year = 2021,
                LicensePlate = "ABC123DE", // Misma placa
                VinCode = "12345678901234568"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/vehicles", vehicle2);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var error = await response.Content.ReadAsStringAsync();
            Assert.Contains("ya existe", error.ToLower());
        }
    }
} 