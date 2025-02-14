namespace AutoLab.Web;

using AutoLab.Domain.Interfaces;
using AutoLab.Infrastructure.ExternalServices;
using AutoLab.Infrastructure.Options;
using Microsoft.Extensions.Options;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews()
            .AddViewOptions(options =>
            {
                options.HtmlHelperOptions.ClientValidationEnabled = true;
            });

        // HTTP Client Factory
        builder.Services.AddHttpClient();

        // Services
        builder.Services.Configure<CountryApiOptions>(
            builder.Configuration.GetSection("CountryApi"));

        builder.Services.AddHttpClient<ICountryService, CountryApiService>()
            .ConfigureHttpClient((sp, client) => 
            {
                var options = sp.GetRequiredService<IOptions<CountryApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", options.Token);
                client.Timeout = TimeSpan.FromSeconds(10);
            });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseMiddleware<WebErrorHandlingMiddleware>();
        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}
