using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace AutoLab.UITests;

public class UITestBase
{
    protected IPlaywright Playwright;
    protected IBrowser Browser;
    protected IPage Page;
    protected const string BaseUrl = "https://localhost:7115";

    public virtual async Task InitializeAsync()
    {
        await TestSetup.ValidateEnvironment();
        
        // Configurar variables de entorno
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("ASPNETCORE_DETAILEDERRORS", "true");
        Environment.SetEnvironmentVariable("ASPNETCORE_SHUTDOWNTIMEOUT", "60");
        
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new()
        {
            Headless = false,
            SlowMo = 250,
            Timeout = 90000,
            Args = new[] { "--no-sandbox" }
        });
        
        var context = await Browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true // Importante para localhost con HTTPS
        }); 
        
        Page = await context.NewPageAsync();
        
        Page.SetDefaultNavigationTimeout(90000);
        Page.SetDefaultTimeout(90000);
        
        // Agregar manejo de errores de página
        Page.PageError += (_, e) => Console.WriteLine($"Error de página: {e}");
        Page.Console += (_, e) => Console.WriteLine($"Consola: {e.Text}");
    }

    public virtual async Task DisposeAsync()
    {
        if (Page != null) await Page.CloseAsync();
        if (Browser != null) await Browser.DisposeAsync();
        if (Playwright != null) Playwright.Dispose();
    }

    protected async Task LoginIfRequired()
    {
        if (Page == null)
            throw new InvalidOperationException("Page no está inicializada. Asegúrate de llamar a InitializeAsync primero.");
        
        await Page.GotoAsync($"{BaseUrl}");
        
        // Si necesitas implementar un login real, aquí es donde iría
        // Por ahora solo navegamos a la página principal
        
        // Esperar a que la página esté cargada
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
