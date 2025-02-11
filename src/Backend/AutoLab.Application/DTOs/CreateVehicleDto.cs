namespace AutoLab.Application.DTOs
{
    public class CreateVehicleDto
    {
        public string Country { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public string VinCode { get; set; }
    }
} 