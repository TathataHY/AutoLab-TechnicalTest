using AutoLab.Domain.Exceptions;

namespace AutoLab.Domain.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Country { get; private set; }
        public string Brand { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }
        public string LicensePlate { get; private set; }
        public string VinCode { get; private set; }

        // Constructor con validaciones
        public Vehicle(string country, string brand, string model, int year, 
            string licensePlate, string vinCode)
        {
            ValidateYear(year);
            ValidateLicensePlate(licensePlate);
            ValidateVinCode(vinCode);
            ValidateRequired(country, nameof(Country));
            ValidateRequired(brand, nameof(Brand));
            ValidateRequired(model, nameof(Model));

            Country = country;
            Brand = brand;
            Model = model;
            Year = year;
            LicensePlate = licensePlate;
            VinCode = vinCode;
        }

        private void ValidateYear(int year)
        {
            if (year > DateTime.Now.Year)
                throw new DomainException("El año no puede ser mayor al actual");
            if (year < 1900)
                throw new DomainException("El año no puede ser menor a 1900");
        }

        private void ValidateLicensePlate(string licensePlate)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                throw new DomainException("La patente no puede estar vacía");
            }

            if (licensePlate.Length < 6 || licensePlate.Length > 8)
            {
                throw new DomainException("La patente debe tener entre 6 y 8 caracteres");
            }

            // Nueva validación con regex para solo permitir letras y números
            if (!System.Text.RegularExpressions.Regex.IsMatch(licensePlate, "^[A-Za-z0-9]+$"))
            {
                throw new DomainException("La patente solo puede contener letras y números");
            }
        }

        private void ValidateVinCode(string vinCode)
        {
            if (string.IsNullOrEmpty(vinCode))
                throw new DomainException("El código VIN es obligatorio");
            if (vinCode.Length < 14 || vinCode.Length > 17)
                throw new DomainException("El código VIN debe tener entre 14 y 17 caracteres");
        }

        private void ValidateRequired(string value, string fieldName)
        {
            if (string.IsNullOrEmpty(value))
                throw new DomainException($"El campo {fieldName} es obligatorio");
        }

        // Más métodos de validación y lógica de dominio
    }
} 