using System;
using System.Net.Http;
using System.Threading.Tasks;

public static class TestSetup
{
    public const string ApiUrl = "https://localhost:7240/swagger";
    public const string WebUrl = "https://localhost:7115";

    public static async Task ValidateEnvironment()
    {
        using var client = new HttpClient();
        try
        {
            var apiResponse = await client.GetAsync(ApiUrl);
            var webResponse = await client.GetAsync(WebUrl);

            if (!apiResponse.IsSuccessStatusCode || !webResponse.IsSuccessStatusCode)
            {
                throw new Exception(
                    "Por favor, asegúrate de que tanto la API como la Web estén ejecutándose:\n" +
                    $"API ({ApiUrl}): {apiResponse.StatusCode}\n" +
                    $"Web ({WebUrl}): {webResponse.StatusCode}"
                );
            }
        }
        catch (HttpRequestException)
        {
            throw new Exception(
                "No se puede conectar con la API o la Web. " +
                "Por favor, asegúrate de que ambos servicios estén ejecutándose."
            );
        }
    }
} 