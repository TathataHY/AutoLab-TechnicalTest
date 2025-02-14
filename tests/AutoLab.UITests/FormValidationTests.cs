using NUnit.Framework;
using Microsoft.Playwright;

namespace AutoLab.UITests;

[TestFixture]
public class FormValidationTests : UITestBase
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
    public async Task ValidacionFormulario_CamposRequeridos()
    {
        await LoginIfRequired();
        await Page.GotoAsync($"{BaseUrl}/vehicles");
        
        await Page.ClickAsync("text=Nuevo Vehículo");
        
        // Esperar a que el formulario esté cargado
        await Page.WaitForSelectorAsync("form");
        
        // Intentar guardar sin llenar campos
        await Page.ClickAsync("button:has-text('Guardar')");
        
        // Verificar mensajes de validación HTML5
        var invalidInputs = await Page.QuerySelectorAllAsync("input:invalid");
        Assert.That(invalidInputs.Count, Is.GreaterThan(0));
        
        // Verificar que el formulario no se envió
        var currentUrl = Page.Url;
        Assert.That(currentUrl, Does.Contain("/Vehicles/Create"));
    }

    [Test]
    [TestCase("ABC 123")] // Espacio en medio
    [TestCase("123-ABC")] // Guión
    [TestCase("AB_123")] // Guión bajo
    public async Task ValidacionFormulario_FormatoPatente(string invalidPlate)
    {
        await LoginIfRequired();
        await Page.GotoAsync($"{BaseUrl}/vehicles");
        
        await Page.ClickAsync("text=Nuevo Vehículo");
        
        // Esperar a que el formulario esté cargado
        await Page.WaitForSelectorAsync("form");
        
        // Llenar todos los campos requeridos
        await Page.SelectOptionAsync("select[name='Country']", new[] { "Spain" });
        await Page.FillAsync("input[name='Brand']", "Toyota");
        await Page.FillAsync("input[name='Model']", "Corolla");
        await Page.FillAsync("input[name='Year']", "2020");
        await Page.FillAsync("input[name='VinCode']", "12345678901234567");
        
        // Llenar la patente inválida
        await Page.FillAsync("input[name='LicensePlate']", invalidPlate);
        
        // Hacer clic en el botón guardar para activar todas las validaciones
        await Page.ClickAsync("button:has-text('Guardar')");
        
        // Esperar y verificar el mensaje de error
        var errorElement = await Page.WaitForSelectorAsync(
            "span.text-danger[data-valmsg-for='LicensePlate']", 
            new() { State = WaitForSelectorState.Visible });
        
        Assert.That(errorElement, Is.Not.Null);
        var errorText = await errorElement.TextContentAsync();
        
        // Verificar que el mensaje coincida con el definido en las Data Annotations
        Assert.That(errorText, Does.Contain("La patente debe contener solo letras mayúsculas y números"));
    }
} 