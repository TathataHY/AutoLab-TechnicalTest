using AutoLab.Application.DTOs;
using AutoLab.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoLab.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string country = null,
            [FromQuery] string brand = null,
            [FromQuery] string model = null,
            [FromQuery] int? year = null,
            [FromQuery] string licensePlate = null,
            [FromQuery] string vinCode = null)
        {
            var result = await _vehicleService.GetAllAsync(
                page, pageSize, country, brand, model, year, licensePlate, vinCode);
            
            return Ok(new {
                items = result.Items,
                total = result.Total,
                page,
                pageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vehicle = await _vehicleService.GetByIdAsync(id);
            return Ok(vehicle);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVehicleDto dto)
        {
            var vehicle = await _vehicleService.CreateVehicleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateVehicleDto dto)
        {
            await _vehicleService.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _vehicleService.DeleteAsync(id);
            return NoContent();
        }
    }
} 