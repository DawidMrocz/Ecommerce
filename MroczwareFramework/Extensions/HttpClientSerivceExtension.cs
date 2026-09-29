using MroczwareFramework.Exceptions;
using System.Net.Http.Json;
using System.Reflection;

namespace MroczwareFramework.Extensions
{
    public static class HttpClientSerivceExtension
    {
        /// <summary>
        /// Akcja GET
        /// </summary>
        /// <typeparam name="R">Obiekt odpowiedzi</typeparam>
        /// <param name="client">Instancja HttpCLienta</param>
        /// <param name="requestUrl">Ścieżka zapytania</param>
        /// <param name="headers">Ewentualne nagłówki</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<R> Get<R, T>(this HttpClient client, string requestUrl, T body, Dictionary<string, string?>? headers = null) where T : class
        {
            try
            {
                UriBuilder uriBuilder = new()
                {
                    Path = requestUrl
                };

                // Tworzenie query string z właściwości obiektu
                Dictionary<string, string> queryParams = typeof(T)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.GetValue(body) is not null)
                    .ToDictionary(
                        p => p.Name,
                        p => p.GetValue(body)!.ToString()!
                    );

                if (queryParams.Count != 0)
                    uriBuilder.Query = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

                string pathWithQueryParams = uriBuilder.Path + (uriBuilder.Query.Length > 0 ? "?" + uriBuilder.Query.TrimStart('?') : string.Empty);

                HttpRequestMessage request = new(HttpMethod.Get, pathWithQueryParams);

                if (headers is not null)
                    foreach (KeyValuePair<string, string?> header in headers.Where(h => !string.IsNullOrWhiteSpace(h.Value)))
                        request.Headers.Add(header.Key, header.Value);

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedException("Brak autoryzacji");

                response.EnsureSuccessStatusCode();

                R resource = await response.Content.ReadFromJsonAsync<R>()
                    ?? throw new Exception("Bład podczas deserializacji odpowiedzi HttpClient'a");

                return resource;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Zapytanie HttpClient zwróciło bład:{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// Akcja GET
        /// </summary>
        /// <typeparam name="R">Obiekt odpowiedzi</typeparam>
        /// <param name="client">Instancja HttpCLienta</param>
        /// <param name="requestUrl">Ścieżka zapytania</param>
        /// <param name="headers">Ewentualne nagłówki</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<R> Get<R>(this HttpClient client, string requestUrl, Dictionary<string, string?>? headers = null)
        {
            try
            {
                HttpRequestMessage request = new(HttpMethod.Get, requestUrl);

                if (headers is not null)
                    foreach (KeyValuePair<string, string?> header in headers.Where(h => !string.IsNullOrWhiteSpace(h.Value)))
                        request.Headers.Add(header.Key, header.Value);

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedException("Brak autoryzacji");

                response.EnsureSuccessStatusCode();

                R resource = await response.Content.ReadFromJsonAsync<R>()
                    ?? throw new Exception("Bład podczas deserializacji odpowiedzi HttpClient'a");

                return resource;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Zapytanie HttpClient zwróciło bład:{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// Akcja Post
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="client">Instancja HttpCLienta</param>
        /// <param name="requestUrl">Ścieżka zapytania</param>
        /// <param name="body">Ciało zapytania</param>
        /// <param name="headers">Ewentualne nagłówki</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<R?> Create<R, T>(this System.Net.Http.HttpClient client, string requestUrl, T body, Dictionary<string, string?>? headers = null)
        {
            try
            {
                HttpRequestMessage request = new(HttpMethod.Post, requestUrl);

                if (headers is not null)
                    foreach (KeyValuePair<string, string?> header in headers.Where(h => !string.IsNullOrWhiteSpace(h.Value)))
                        request.Headers.Add(header.Key, header.Value);

                request.Content = JsonContent.Create(body);

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedException("Brak autoryzacji");

                response.EnsureSuccessStatusCode();

                if (!string.IsNullOrWhiteSpace(await response.Content.ReadAsStringAsync()))
                {
                    return await response.Content.ReadFromJsonAsync<R>()
                        ?? throw new Exception("Bład podczas deserializacji odpowiedzi HttpClient'a");
                }
                else
                {
                    return default;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Zapytanie HttpClient zwróciło bład:{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// Akcja Post
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="client">Instancja HttpCLienta</param>
        /// <param name="requestUrl">Ścieżka zapytania</param>
        /// <param name="body">Ciało zapytania</param>
        /// <param name="headers">Ewentualne nagłówki</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task Create<T>(this System.Net.Http.HttpClient client, string requestUrl, T body, Dictionary<string, string?>? headers = null)
        {
            try
            {
                HttpRequestMessage request = new(HttpMethod.Post, requestUrl);

                if (headers is not null)
                    foreach (KeyValuePair<string, string?> header in headers.Where(h => !string.IsNullOrWhiteSpace(h.Value)))
                        request.Headers.Add(header.Key, header.Value);

                request.Content = JsonContent.Create(body);

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedException("Brak autoryzacji");

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Zapytanie HttpClient zwróciło bład:{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// Akcja Post
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="client">Instancja HttpCLienta</param>
        /// <param name="requestUrl">Ścieżka zapytania</param>
        /// <param name="body">Ciało zapytania</param>
        /// <param name="headers">Ewentualne nagłówki</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task Create(this System.Net.Http.HttpClient client, string requestUrl, Dictionary<string, string?>? headers = null)
        {
            try
            {
                HttpRequestMessage request = new(HttpMethod.Post, requestUrl);

                if (headers is not null)
                    foreach (KeyValuePair<string, string?> header in headers.Where(h => !string.IsNullOrWhiteSpace(h.Value)))
                        request.Headers.Add(header.Key, header.Value);

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedException("Brak autoryzacji");

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Zapytanie HttpClient zwróciło bład:{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// Akcja Post
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="client">Instancja HttpCLienta</param>
        /// <param name="requestUrl">Ścieżka zapytania</param>
        /// <param name="body">Ciało zapytania</param>
        /// <param name="headers">Ewentualne nagłówki</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task Update<T>(this System.Net.Http.HttpClient client, string requestUrl, T body, Dictionary<string, string?>? headers = null)
        {
            try
            {
                HttpRequestMessage request = new(HttpMethod.Put, requestUrl);

                if (headers is not null)
                    foreach (KeyValuePair<string, string?> header in headers.Where(h => !string.IsNullOrWhiteSpace(h.Value)))
                        request.Headers.Add(header.Key, header.Value);

                request.Content = JsonContent.Create(body);

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedException("Brak autoryzacji");

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Zapytanie HttpClient zwróciło bład:{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// Akcja PUT
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="client">Instancja HttpCLienta</param>
        /// <param name="requestUrl">Ścieżka zapytania</param>
        /// <param name="body">Ciało zapytania</param>
        /// <param name="headers">Ewentualne nagłówki</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<R?> Update<R, T>(this System.Net.Http.HttpClient client, string requestUrl, T body, Dictionary<string, string?>? headers = null)
        {
            try
            {
                HttpRequestMessage request = new(HttpMethod.Put, requestUrl);

                if (headers is not null)
                    foreach (KeyValuePair<string, string?> header in headers.Where(h => !string.IsNullOrWhiteSpace(h.Value)))
                        request.Headers.Add(header.Key, header.Value);

                request.Content = JsonContent.Create(body);

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedException("Brak autoryzacji");

                response.EnsureSuccessStatusCode();

                if (!string.IsNullOrWhiteSpace(await response.Content.ReadAsStringAsync()))
                {
                    return await response.Content.ReadFromJsonAsync<R>()
                        ?? throw new Exception("Bład podczas deserializacji odpowiedzi HttpClient'a");
                }
                else
                {
                    return default;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Zapytanie HttpClient zwróciło bład:{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// Akcja PUT
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="client">Instancja HttpCLienta</param>
        /// <param name="requestUrl">Ścieżka zapytania</param>
        /// <param name="body">Ciało zapytania</param>
        /// <param name="headers">Ewentualne nagłówki</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<R?> Update<R>(this HttpClient client, string requestUrl, Dictionary<string, string?>? headers = null)
        {
            try
            {
                HttpRequestMessage request = new(HttpMethod.Put, requestUrl);

                if (headers is not null)
                    foreach (KeyValuePair<string, string?> header in headers.Where(h => !string.IsNullOrWhiteSpace(h.Value)))
                        request.Headers.Add(header.Key, header.Value);

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedException("Brak autoryzacji");

                response.EnsureSuccessStatusCode();

                if (!string.IsNullOrWhiteSpace(await response.Content.ReadAsStringAsync()))
                {
                    return await response.Content.ReadFromJsonAsync<R>()
                        ?? throw new Exception("Bład podczas deserializacji odpowiedzi HttpClient'a");
                }
                else
                {
                    return default;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Zapytanie HttpClient zwróciło bład:{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// Akcja PUT
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="client">Instancja HttpCLienta</param>
        /// <param name="requestUrl">Ścieżka zapytania</param>
        /// <param name="body">Ciało zapytania</param>
        /// <param name="headers">Ewentualne nagłówki</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task Update(this HttpClient client, string requestUrl, Dictionary<string, string?>? headers = null)
        {
            try
            {
                HttpRequestMessage request = new(HttpMethod.Put, requestUrl);

                if (headers is not null)
                    foreach (KeyValuePair<string, string?> header in headers.Where(h => !string.IsNullOrWhiteSpace(h.Value)))
                        request.Headers.Add(header.Key, header.Value);

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedException("Brak autoryzacji");

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Zapytanie HttpClient zwróciło bład:{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// Akcja DELETE
        /// </summary>
        /// <param name="client">Instancja HttpCLienta</param>
        /// <param name="requestUrl">Ścieżka zapytania</param>
        /// <param name="headers">Ewentualne nagłówki</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task Delete(this System.Net.Http.HttpClient client, string requestUrl, Dictionary<string, string?>? headers = null)
        {
            try
            {
                HttpRequestMessage request = new(HttpMethod.Delete, requestUrl);


                if (headers is not null)
                    foreach (KeyValuePair<string, string?> header in headers.Where(h => !string.IsNullOrWhiteSpace(h.Value)))
                        request.Headers.Add(header.Key, header.Value);

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedException("Brak autoryzacji");

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Zapytanie HttpClient zwróciło bład:{ex.InnerException?.Message ?? ex.Message}");
            }
        }


    }
}
