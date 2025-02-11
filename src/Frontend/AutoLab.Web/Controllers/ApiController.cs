using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Frontend.Web.Controllers
{
    public class ApiController : Controller
    {
        private readonly ICountryService _countryService;

        public ApiController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCountries(string search)
        {
            var countries = await _countryService.GetCountriesAsync();
            
            if (!string.IsNullOrEmpty(search))
            {
                countries = countries.Where(c => 
                    c.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            return Json(countries);
        }
    }
} 