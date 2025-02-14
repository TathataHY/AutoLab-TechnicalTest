using AutoLab.Domain.Exceptions;

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

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Country) || Country.Length < 3)
                throw new DomainException("El país es requerido y debe tener al menos 3 caracteres");

            if (string.IsNullOrWhiteSpace(Brand))
                throw new DomainException("La Marca es requerida");

            if (string.IsNullOrWhiteSpace(Model))
                throw new DomainException("El Modelo es requerido");

            if (Year > DateTime.Now.Year)
                throw new DomainException("El año no puede ser mayor al actual");
            if (Year < 1900)
                throw new DomainException("El año no puede ser menor a 1900");

            if (string.IsNullOrWhiteSpace(LicensePlate) || LicensePlate.Length < 6 || LicensePlate.Length > 8)
                throw new DomainException("La patente debe tener entre 6 y 8 caracteres");

            if (string.IsNullOrWhiteSpace(VinCode) || VinCode.Length != 17)
                throw new DomainException("El VIN debe tener exactamente 17 caracteres");
        }
    }
}