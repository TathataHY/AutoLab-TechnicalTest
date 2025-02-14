using AutoLab.Application.DTOs;
using AutoLab.Application.Interfaces;
using AutoLab.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace AutoLab.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private const int MAX_ID = 100000000; // Valor razonable para ID máximo
        private const int MAX_STRING_LENGTH = 500;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int pageSize = 10,
            string country = null,
            string brand = null,
            string model = null,
            int? year = null,
            string licensePlate = null,
            string vinCode = null)
        {
            try
            {
                if (page <= 0)
                    return BadRequest("El número de página debe ser mayor a 0");

                if (pageSize <= 0 || pageSize > 100)
                    return BadRequest("El tamaño de página debe estar entre 1 y 100");

                var (vehicles, total) = await _vehicleService.GetAllAsync(
                    page, pageSize, country, brand, model, year, licensePlate, vinCode);
                return Ok(new PaginatedResponse<VehicleDto>(vehicles, total));
            }
            catch (DomainException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                if (id <= 0 || id > MAX_ID)
                    return BadRequest("ID de vehículo inválido");

                var vehicle = await _vehicleService.GetByIdAsync(id);
                return Ok(vehicle);
            }
            catch (DomainException ex) when (ex.Message.Contains("no encontrado"))
            {
                return NotFound(ex.Message);
            }
            catch (DomainException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateVehicleDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { error = "Los datos del vehículo son requeridos" });

                // Primero validar campos nulos
                if (HasNullOrEmptyRequiredFields(dto))
                {
                    return BadRequest(new { error = "Todos los campos son requeridos" });
                }

                // Luego validar caracteres inválidos (incluyendo strings vacíos)
                if (ContainsInvalidCharacters(dto.Country) ||
                    ContainsInvalidCharacters(dto.Brand) ||
                    ContainsInvalidCharacters(dto.Model))
                {
                    return BadRequest(new { error = "Los campos contienen caracteres inválidos" });
                }

                // Validar longitud máxima de campos
                if (HasExtremelyLongStrings(dto))
                {
                    return BadRequest(new { error = "Los campos exceden la longitud máxima permitida" });
                }

                // Validar formato de matrícula
                var invalidChars = new[] { ' ', '-', '_' };
                if (dto.LicensePlate?.Any(c => invalidChars.Contains(c) || char.IsLower(c)) == true)
                {
                    return BadRequest(new { error = "El formato de la patente no es válido" });
                }

                // Validar año futuro
                if (dto.Year > DateTime.Now.Year)
                {
                    return BadRequest(new { error = "El año no puede ser futuro" });
                }

                try
                {
                    dto.Validate();
                }
                catch (DomainException ex)
                {
                    return BadRequest(new { error = ex.Message });
                }

                var result = await _vehicleService.CreateVehicleAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateVehicleDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { error = "Los datos del vehículo son requeridos" });

                if (id <= 0 || id > MAX_ID)
                    return BadRequest(new { error = "ID de vehículo inválido" });

                // Validar formato de matrícula
                if (!string.IsNullOrEmpty(dto.LicensePlate))
                {
                    var invalidChars = new[] { ' ', '-', '_' };
                    if (dto.LicensePlate.Any(c => invalidChars.Contains(c) || char.IsLower(c)))
                    {
                        return BadRequest(new { error = "El formato de la matrícula no es válido" });
                    }
                }

                // Validar caracteres especiales en campos de texto
                if (ContainsInvalidCharacters(dto.Country) ||
                    ContainsInvalidCharacters(dto.Brand) ||
                    ContainsInvalidCharacters(dto.Model))
                {
                    return BadRequest(new { error = "Los campos contienen caracteres inválidos" });
                }

                try
                {
                    dto.Validate();
                }
                catch (DomainException ex)
                {
                    return BadRequest(new { error = ex.Message });
                }

                await _vehicleService.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (DomainException ex) when (ex.Message.Contains("no encontrado"))
            {
                return NotFound(new { error = ex.Message });
            }
            catch (DomainException ex) when (ex.Message.Contains("modificado"))
            {
                return Conflict(new { error = ex.Message });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _vehicleService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term, [FromQuery] string type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || string.IsNullOrWhiteSpace(type))
                    return BadRequest("El término de búsqueda y tipo son requeridos");

                if (type != "licensePlate" && type != "vinCode")
                    return BadRequest("El tipo de búsqueda debe ser 'licensePlate' o 'vinCode'");

                var results = await _vehicleService.SearchAsync(term, type);
                return Ok(results);
            }
            catch (DomainException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool HasExtremelyLongStrings(CreateVehicleDto dto)
        {
            return (dto.Country?.Length > MAX_STRING_LENGTH ||
                    dto.Brand?.Length > MAX_STRING_LENGTH ||
                    dto.Model?.Length > MAX_STRING_LENGTH ||
                    dto.LicensePlate?.Length > MAX_STRING_LENGTH ||
                    dto.VinCode?.Length > MAX_STRING_LENGTH);
        }

        private bool ContainsInvalidCharacters(string value)
        {
            if (value == null) return false;
            if (string.IsNullOrWhiteSpace(value)) return true;

            var validChars = new[] { 'ñ', 'Ñ', 'á', 'é', 'í', 'ó', 'ú', 'Á', 'É', 'Í', 'Ó', 'Ú' };
            return value.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c) && !validChars.Contains(c));
        }

        private bool HasNullOrEmptyRequiredFields(CreateVehicleDto dto)
        {
            return dto.Country == null ||
                   dto.Brand == null ||
                   dto.Model == null ||
                   dto.LicensePlate == null ||
                   dto.VinCode == null;
        }

        private bool HasInvalidCharacters(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
        }

        private bool HasInvalidFormat(CreateVehicleDto dto)
        {
            var licensePlatePattern = @"^[A-Z0-9]{6,8}$";
            var vinPattern = @"^[A-HJ-NPR-Z0-9]{17}$";

            return !Regex.IsMatch(dto.LicensePlate, licensePlatePattern) ||
                   !Regex.IsMatch(dto.VinCode, vinPattern);
        }

        private bool HasFutureYear(CreateVehicleDto dto)
        {
            return dto.Year > DateTime.Now.Year;
        }
    }
}