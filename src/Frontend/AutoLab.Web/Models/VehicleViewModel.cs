using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AutoLab.Web.Models
{
    public class VehicleViewModel
    {
        public VehicleViewModel()
        {
            Country = string.Empty;
            Brand = string.Empty;
            Model = string.Empty;
            LicensePlate = string.Empty;    
            VinCode = string.Empty;
            Year = DateTime.Now.Year;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "El país es obligatorio")]
        public string Country { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        public string Brand { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio")]
        public string Model { get; set; }

        [Required(ErrorMessage = "El año es obligatorio")]
        [Range(1900, 2025, ErrorMessage = "El año debe estar entre 1900 y el año actual")]
        public int Year { get; set; }

        [Required(ErrorMessage = "La patente es obligatoria")]
        [StringLength(8, MinimumLength = 6, ErrorMessage = "La patente debe tener entre 6 y 8 caracteres")]
        [RegularExpression("^[A-Z0-9]{6,8}$", ErrorMessage = "La patente debe contener solo letras mayúsculas y números")]
        public string LicensePlate { get; set; }

        [Required(ErrorMessage = "El código VIN es obligatorio")]
        [StringLength(17, MinimumLength = 14, ErrorMessage = "El código VIN debe tener entre 14 y 17 caracteres")]
        public string VinCode { get; set; }
    }
} 