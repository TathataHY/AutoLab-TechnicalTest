using AutoLab.Domain.Entities;

namespace AutoLab.Application.DTOs
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string Country { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public string VinCode { get; set; }

        public VehicleDto()
        {
        }

        public VehicleDto(Vehicle vehicle)
        {
            Id = vehicle.Id;
            Country = vehicle.Country;
            Brand = vehicle.Brand;
            Model = vehicle.Model;
            Year = vehicle.Year;
            LicensePlate = vehicle.LicensePlate;
            VinCode = vehicle.VinCode;
        }
    }
} 