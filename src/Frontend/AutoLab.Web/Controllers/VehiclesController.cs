using AutoLab.Domain.Interfaces;
using AutoLab.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net;
using AutoLab.Domain.Exceptions;
using AutoLab.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace AutoLab.Web.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ICountryService _countryService;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ICountryService countryService, ILogger<VehiclesController> logger)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]);
            _countryService = countryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            string country = null, 
            string brand = null, 
            string model = null, 
            int? year = null, 
            string licensePlate = null, 
            string vinCode = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var queryParams = new List<string>();
                
                if (!string.IsNullOrEmpty(country))
                    queryParams.Add($"country={Uri.EscapeDataString(country)}");
                if (!string.IsNullOrEmpty(brand))
                    queryParams.Add($"brand={Uri.EscapeDataString(brand)}");
                if (!string.IsNullOrEmpty(model))
                    queryParams.Add($"model={Uri.EscapeDataString(model)}");
                if (year.HasValue)
                    queryParams.Add($"year={year}");
                if (!string.IsNullOrEmpty(licensePlate))
                    queryParams.Add($"licensePlate={Uri.EscapeDataString(licensePlate)}");
                if (!string.IsNullOrEmpty(vinCode))
                    queryParams.Add($"vinCode={Uri.EscapeDataString(vinCode)}");
                
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/vehicles?{queryString}");
                var content = await response.Content.ReadAsStringAsync();
                
                var result = JsonSerializer.Deserialize<PaginatedResponse<VehicleViewModel>>(content);
                ViewBag.Countries = await _countryService.GetCountriesAsync();
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = result?.Total ?? 0;
                ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / (double)pageSize);
                
                return View(result?.Items ?? new List<VehicleViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar vehículos: {Message}", ex.Message);
                if (ex is JsonException jsonEx)
                {
                }
                ViewBag.Countries = new List<string>();
                ViewBag.ErrorMessage = "Error al cargar los vehículos";
                return View(new List<VehicleViewModel>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var countries = await _countryService.GetCountriesAsync();
                ViewBag.Countries = countries;
                return View();
            }
            catch (Exception ex)
            {
                // Log the error
                ViewBag.Countries = new List<string>(); // Lista vacía como fallback
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleViewModel model)
        {
            // Limpiamos el ModelState actual
            ModelState.Clear();

            var formData = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());

            // Asignamos los valores directamente al modelo existente en lugar de crear uno nuevo
            model.Country = formData["Country"];
            model.Brand = formData["Brand"];
            model.Model = formData["Model"];
            model.Year = int.Parse(formData["Year"]);
            model.LicensePlate = formData["LicensePlate"];
            model.VinCode = formData["VinCode"];

            // Revalidamos el modelo con los nuevos valores
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                ViewBag.Countries = await _countryService.GetCountriesAsync();
                return View(model);
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/vehicles", model);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));

                // Manejar errores específicos del backend
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(responseContent);
                        if (errorResponse?.error != null)
                        {
                            // Agregar el error al div de alertas
                            TempData["ErrorMessage"] = errorResponse.error;
                            ModelState.AddModelError("", errorResponse.error);
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Error al crear el vehículo";
                            ModelState.AddModelError("", "Error al crear el vehículo");
                        }
                    }
                    catch
                    {
                        TempData["ErrorMessage"] = responseContent;
                        ModelState.AddModelError("", responseContent);
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError("", "El vehículo ya existe o ha sido modificado");
                }
                else
                {
                    ModelState.AddModelError("", "Error al procesar la solicitud");
                }

                ViewBag.Countries = await _countryService.GetCountriesAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error de conexión con el servidor");
                ViewBag.Countries = await _countryService.GetCountriesAsync();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetVehicles([FromQuery] DataTableParameters parameters)
        {
            var queryParams = new List<string>();
            
            // Obtener los filtros del Request.Query directamente
            var country = Request.Query["country"].ToString();
            var brand = Request.Query["brand"].ToString();
            var model = Request.Query["model"].ToString();
            var year = Request.Query["year"].ToString();
            var licensePlate = Request.Query["licensePlate"].ToString();
            var vinCode = Request.Query["vinCode"].ToString();
            
            if (!string.IsNullOrEmpty(country))
                queryParams.Add($"country={Uri.EscapeDataString(country)}");
            if (!string.IsNullOrEmpty(brand))
                queryParams.Add($"brand={Uri.EscapeDataString(brand)}");
            if (!string.IsNullOrEmpty(model))
                queryParams.Add($"model={Uri.EscapeDataString(model)}");
            if (!string.IsNullOrEmpty(year))
                queryParams.Add($"year={year}");
            if (!string.IsNullOrEmpty(licensePlate))
                queryParams.Add($"licensePlate={Uri.EscapeDataString(licensePlate)}");
            if (!string.IsNullOrEmpty(vinCode))
                queryParams.Add($"vinCode={Uri.EscapeDataString(vinCode)}");

            queryParams.Add($"page={parameters.Start / parameters.Length + 1}");
            queryParams.Add($"pageSize={parameters.Length}");

            var queryString = string.Join("&", queryParams);
            Console.WriteLine($"Query string: {queryString}"); // Para debugging

            var response = await _httpClient.GetAsync($"api/vehicles?{queryString}");
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Response: {content}"); // Para debugging
            
            var result = JsonSerializer.Deserialize<DataTableResponse>(content);

            return Json(new
            {
                draw = parameters.Draw,
                recordsTotal = result.Total,
                recordsFiltered = result.Total,
                data = result.Items
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"api/vehicles/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var vehicle = await response.Content.ReadFromJsonAsync<VehicleViewModel>();
            return View(vehicle);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, VehicleViewModel model)
        {
            // Limpiamos el ModelState actual
            ModelState.Clear();

            var formData = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());

            // Asignamos los valores directamente al modelo existente en lugar de crear uno nuevo
            model.Id = int.Parse(formData["Id"]);
            model.Country = formData["Country"];
            model.Brand = formData["Brand"];
            model.Model = formData["Model"];
            model.Year = int.Parse(formData["Year"]);
            model.LicensePlate = formData["LicensePlate"];
            model.VinCode = formData["VinCode"];

            // Revalidamos el modelo con los nuevos valores
            TryValidateModel(model);

            if (!ModelState.IsValid)
                return View(model);

            var response = await _httpClient.PutAsJsonAsync($"api/vehicles/{id}", model);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error al actualizar el vehículo");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/vehicles/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Error al eliminar el vehículo" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el vehículo" });
            }
        }
    }
}