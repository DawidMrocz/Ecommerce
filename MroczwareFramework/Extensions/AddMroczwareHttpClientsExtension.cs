using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MroczwareFramework.Models.HttpClient;
using System.Security.Cryptography.X509Certificates;

namespace MroczwareFramework.Extensions
{
    /// <summary>
    /// Rejestracja HttpClients z appsettings.json
    /// </summary>
    public static class AddMroczwareHttpClientsExtension
    {
        public static IServiceCollection AddMroczwareHttpClients(this IServiceCollection services, IConfigurationManager configuration)
        {

            List<HttpClientItemDto>? clients = configuration.GetSection("HttpClients").Get<List<HttpClientItemDto>>();

            if (clients is null || !clients.Any()) return services;

            foreach (HttpClientItemDto httpClient in clients)
                services.AddHttpClient(httpClient.Name, client =>
                {
                    client.BaseAddress = new Uri(httpClient.Url);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                });

            foreach (HttpClientItemDto httpClient in clients)
            {
                IHttpClientBuilder service = services.AddHttpClient(httpClient.Name, client =>
                {
                    client.BaseAddress = new Uri(httpClient.Url);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                });

                if (httpClient.Certificate != null && httpClient.Password != null)
                    service.ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        HttpClientHandler handler = new();
                        handler.ClientCertificates.Add(new X509Certificate2(httpClient.Certificate, httpClient.Password));
                        return handler;
                    });
            }
            return services;
        }
    }
}
