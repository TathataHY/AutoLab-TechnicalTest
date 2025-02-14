using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AutoLab.UITests
{
    public class CountrySearchTests : UITestBase
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
        public async Task BusquedaPais_DebePermitirBusquedaTipeada()
        {
            await LoginIfRequired();
            await Page.GotoAsync($"{BaseUrl}/vehicles/create");
            
            // Esperar que el select2 esté cargado
            await Page.WaitForSelectorAsync(".select2-container");
            
            // Abrir el dropdown
            await Page.ClickAsync(".select2-selection");
            
            // Escribir "spa" para buscar Spain
            await Page.FillAsync(".select2-search__field", "spa");
            
            // Esperar resultados
            await Page.WaitForSelectorAsync(".select2-results__option:has-text('Spain')");
            
            // Seleccionar Spain
            await Page.ClickAsync(".select2-results__option:has-text('Spain')");
            
            // Verificar que se seleccionó
            var selectedValue = await Page.EvaluateAsync<string>(
                "() => document.querySelector('select[name=\"Country\"]').value");
            Assert.That(selectedValue, Is.EqualTo("Spain"));
        }
    }
} 