using System;
using NUnit.Framework;
using System.Threading.Tasks;

namespace AutoLab.UITests
{
    public class YearValidationTests : UITestBase
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
        public async Task Year_NoDebePermitirAñoFuturo()
        {
            await LoginIfRequired();
            await Page.GotoAsync($"{BaseUrl}/vehicles/create");
            
            var nextYear = DateTime.Now.Year + 1;
            
            // Intentar ingresar año futuro
            await Page.FillAsync("input[name='Year']", nextYear.ToString());
            await Page.ClickAsync("button:has-text('Guardar')");
            
            // Verificar mensaje de error
            var errorElement = await Page.WaitForSelectorAsync(
                "span.text-danger[data-valmsg-for='Year']");
            var errorText = await errorElement.TextContentAsync();
            
            // Aceptar tanto el mensaje en español como el mensaje por defecto en inglés
            Assert.That(errorText, Does.Contain("año actual").Or.Contains("less than or equal to"));
        }
    }
} 