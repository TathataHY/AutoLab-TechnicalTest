using NUnit.Framework;
using System.Threading.Tasks;

namespace AutoLab.UITests
{
    public class DataTableTests : UITestBase
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
        public async Task DataTable_FiltroBusquedaFunciona()
        {
            await LoginIfRequired();
            await Page.GotoAsync($"{BaseUrl}/vehicles");

            // Esperar que la tabla cargue
            await Page.WaitForSelectorAsync(".dataTables_wrapper");

            // Buscar por marca "Toyota"
            await Page.FillAsync("input[data-column='Marca']", "Toyota");

            // Esperar que la tabla se actualice
            await Task.Delay(500); // Dar tiempo al server-side processing

            // Verificar que los resultados contienen Toyota
            var cells = await Page.QuerySelectorAllAsync("td:has-text('Toyota')");
            Assert.That(cells.Count, Is.GreaterThan(0), "La búsqueda no retornó resultados");
        }
    }
}