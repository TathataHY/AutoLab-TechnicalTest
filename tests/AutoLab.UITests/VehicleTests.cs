using NUnit.Framework;
using Microsoft.Playwright;

namespace AutoLab.UITests;

[TestFixture]
public class VehicleTests : UITestBase
{
    [SetUp]
    public async Task Setup()
    {
        await InitializeAsync();
    }

    [TearDown]
    public async Task Teardown()
    {
        await DisposeAsync();
    }

    [Test]
    public async Task CrearNuevoVehiculo_Exitoso()
    {
        await LoginIfRequired();
        await Page.GotoAsync($"{BaseUrl}/vehicles");
        
        await Page.ClickAsync("text=Nuevo Vehículo");
        await Page.WaitForSelectorAsync("form");
        
        // Llenar el formulario
        await Page.SelectOptionAsync("select[name='Country']", new[] { "Spain" });
        await Page.FillAsync("input[name='Brand']", "Toyota");
        await Page.FillAsync("input[name='Model']", "Corolla");
        await Page.FillAsync("input[name='Year']", "2020");
        await Page.FillAsync("input[name='LicensePlate']", "ABC123DE");
        await Page.FillAsync("input[name='VinCode']", "12345678901234567");
        
        // Guardar y esperar la navegación
        var navigationTask = Page.WaitForNavigationAsync(new PageWaitForNavigationOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
        
        await Page.ClickAsync("button:has-text('Guardar')");
        await navigationTask;
        
        // Verificar que aparezca en la lista
        await Page.WaitForSelectorAsync("td:has-text('ABC123DE')", 
            new PageWaitForSelectorOptions { Timeout = 60000 });
    }

    [Test]
    public async Task BuscarYFiltrarVehiculos_Exitoso()
    {
        await LoginIfRequired();
        await Page.GotoAsync($"{BaseUrl}/vehicles");

        // Esperar a que el formulario de filtros esté visible
        await Page.WaitForSelectorAsync("#filterForm", new() { State = WaitForSelectorState.Visible });
        
        // Llenar el campo de marca
        await Page.FillAsync("input[name='Brand']", "Toyota");
        
        // Hacer clic en el botón Buscar y esperar la respuesta
        await Task.WhenAll(
            Page.WaitForNavigationAsync(),
            Page.ClickAsync("button[type='submit']")
        );

        // Verificar que la tabla se haya cargado
        var table = await Page.QuerySelectorAsync(".table-responsive");
        Assert.That(table, Is.Not.Null, "La tabla no se encontró en la página");

        // Verificar los resultados
        var rows = await Page.QuerySelectorAllAsync("table tbody tr");
        Assert.That(rows.Count, Is.GreaterThan(0), "No se encontraron resultados");

        // Verificar que todas las filas contengan "Toyota"
        foreach (var row in rows)
        {
            var marcaCell = await row.QuerySelectorAsync("td:nth-child(2)");
            var marcaText = await marcaCell.TextContentAsync();
            Assert.That(marcaText.Trim().ToLower(), Does.Contain("toyota"), 
                $"Se encontró una fila con marca '{marcaText}' que no contiene 'Toyota'");
        }
    }

    [Test]
    public async Task CrearMultiplesVehiculos_ParaPruebaDeFiltros()
    {
        await LoginIfRequired();
        
        // Generar un identificador único usando timestamp y GUID
        var timestamp = DateTime.Now.ToString("MMddHHmmss");
        var guid = Guid.NewGuid().ToString("N").Substring(0, 4);
        
        // Función helper para generar VIN único de 17 caracteres
        string GenerateVin() => 
            Guid.NewGuid().ToString("N").Substring(0, 17).ToUpper();

        // Función helper para generar patente única
        string GenerateLicensePlate()
        {
            var random = new Random();
            var letters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            var numbers = "0123456789";
            return new string(new[] {
                letters[random.Next(letters.Length)],
                letters[random.Next(letters.Length)],
                letters[random.Next(letters.Length)],
                numbers[random.Next(numbers.Length)],
                numbers[random.Next(numbers.Length)],
                numbers[random.Next(numbers.Length)],
                letters[random.Next(letters.Length)],
                letters[random.Next(letters.Length)]
            });
        }

        // Datos de prueba con valores únicos
        var vehiculos = new[]
        {
            new { 
                Pais = "Spain", 
                Marca = $"Toyota", 
                Modelo = $"Corolla", 
                Año = "2020", 
                Patente = GenerateLicensePlate(), 
                Vin = GenerateVin()
            },
            new { 
                Pais = "Spain", 
                Marca = $"Honda", 
                Modelo = $"Civic", 
                Año = "2021", 
                Patente = GenerateLicensePlate(), 
                Vin = GenerateVin()
            },
            new { 
                Pais = "France", 
                Marca = $"Renault", 
                Modelo = $"Clio", 
                Año = "2019", 
                Patente = GenerateLicensePlate(), 
                Vin = GenerateVin()
            },
            new { 
                Pais = "Spain", 
                Marca = $"Toyota", 
                Modelo = $"RAV4", 
                Año = "2022", 
                Patente = GenerateLicensePlate(), 
                Vin = GenerateVin()
            },
            new { 
                Pais = "Italy", 
                Marca = $"Fiat", 
                Modelo = $"500", 
                Año = "2020", 
                Patente = GenerateLicensePlate(), 
                Vin = GenerateVin()
            }
        };

        foreach (var vehiculo in vehiculos)
        {
            await Page.GotoAsync($"{BaseUrl}/vehicles");
            await Page.ClickAsync("text=Nuevo Vehículo");
            await Page.WaitForSelectorAsync("form");

            // Llenar el formulario
            await Page.SelectOptionAsync("select[name='Country']", new[] { vehiculo.Pais });
            await Page.FillAsync("input[name='Brand']", vehiculo.Marca);
            await Page.FillAsync("input[name='Model']", vehiculo.Modelo);
            await Page.FillAsync("input[name='Year']", vehiculo.Año);
            await Page.FillAsync("input[name='LicensePlate']", vehiculo.Patente);
            await Page.FillAsync("input[name='VinCode']", vehiculo.Vin);

            // Guardar y esperar la navegación
            await Task.WhenAll(
                Page.WaitForNavigationAsync(),
                Page.ClickAsync("button:has-text('Guardar')")
            );

            // Verificar que se guardó correctamente
            await Page.WaitForSelectorAsync($"td:has-text('{vehiculo.Patente}')", 
                new() { Timeout = 30000 });
        }

        // Verificar que se pueden filtrar por marca Toyota
        await Page.GotoAsync($"{BaseUrl}/vehicles");
        await Page.WaitForSelectorAsync("#filterForm");
        await Page.FillAsync("input[name='Brand']", $"Toyota_{guid}");
        
        await Task.WhenAll(
            Page.WaitForNavigationAsync(),
            Page.ClickAsync("button[type='submit']")
        );

        // Verificar resultados del filtro
        var toyotaRows = await Page.QuerySelectorAllAsync("table tbody tr");
        Assert.That(toyotaRows.Count, Is.EqualTo(2), "Debería haber exactamente 2 Toyota");
    }

    [Test]
    public async Task VerificarBotonesAcciones_Funcionan()
    {
        await LoginIfRequired();
        await Page.GotoAsync($"{BaseUrl}/vehicles");

        // Esperar a que la tabla se cargue completamente
        await Page.WaitForSelectorAsync("#vehiclesTable", new() { State = WaitForSelectorState.Visible });
        await Task.Delay(1000); // Dar tiempo a que DataTables termine de inicializarse

        // Buscar los botones en la primera fila de la tabla
        var firstRowEditButton = await Page.QuerySelectorAsync("table tbody tr:first-child a.btn-primary");
        var firstRowDeleteButton = await Page.QuerySelectorAsync("table tbody tr:first-child button.btn-danger");

        // Verificar que los botones existen
        Assert.That(firstRowEditButton, Is.Not.Null, "El botón de editar no está presente en la primera fila");
        Assert.That(firstRowDeleteButton, Is.Not.Null, "El botón de eliminar no está presente en la primera fila");
    }

    [Test]
    public async Task BuscarYFiltrarVehiculos_ConPaginacion_Exitoso()
    {
        await LoginIfRequired();
        await Page.GotoAsync($"{BaseUrl}/vehicles");

        // Esperar a que el formulario de filtros esté visible
        await Page.WaitForSelectorAsync("#filterForm", new() { State = WaitForSelectorState.Visible });
        
        // Verificar que existe la paginación
        var paginacion = await Page.QuerySelectorAsync(".pagination");
        Assert.That(paginacion, Is.Not.Null, "No se encontró la paginación");

        // Verificar que se muestran los elementos por página correctamente
        var filas = await Page.QuerySelectorAllAsync("table tbody tr");
        Assert.That(filas.Count, Is.LessThanOrEqualTo(10), "No se están respetando los elementos por página");

        // Probar navegación a la siguiente página
        var siguientePagina = await Page.QuerySelectorAsync(".pagination .page-item:not(.disabled) a[href*='page=2']");
        if (siguientePagina != null)
        {
            await Task.WhenAll(
                Page.WaitForNavigationAsync(),
                Page.ClickAsync(".pagination .page-item:not(.disabled) a[href*='page=2']")
            );

            // Verificar que estamos en la página 2
            var paginaActual = await Page.QuerySelectorAsync(".pagination .active");
            var textoPagina = await paginaActual.TextContentAsync();
            Assert.That(textoPagina.Trim(), Is.EqualTo("2"), "No se navegó correctamente a la página 2");
        }
    }
}
