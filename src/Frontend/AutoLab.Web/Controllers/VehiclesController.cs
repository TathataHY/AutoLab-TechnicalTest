using AutoLab.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AutoLab.Web.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public VehiclesController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(VehicleViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _httpClient.PostAsJsonAsync("api/vehicles", model);
            
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error al crear el vehículo");
            return View(model);
        }

        [HttpGet]
        public async Task<JsonResult> GetVehicles(DataTableParameters parameters)
        {
            var response = await _httpClient.GetAsync($"api/vehicles?page={parameters.Start/parameters.Length + 1}&pageSize={parameters.Length}");
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<DataTableResponse>(content);

            return Json(new {
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
            var response = await _httpClient.DeleteAsync($"api/vehicles/{id}");
            return Json(new { success = response.IsSuccessStatusCode });
        }
    }
} 