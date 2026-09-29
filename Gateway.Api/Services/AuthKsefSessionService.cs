
using Gateway.Api.ApiModels.Access;
using Gateway.Api.ApiModels.Auth;
using Gateway.Api.ApiModels.Exceptions;
using Gateway.Api.Extensions;
using Gateway.Api.Models;
using Gateway.Api.TestCert;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Threading;
using static Gateway.Api.Services.AuthKsefSessionService;

namespace Gateway.Api.Services
{
    public class AuthKsefSessionService : IAuthKsefSessionService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IRouteBuilder _routeBuilder;
        private readonly IConfiguration _configuration;

        public AuthKsefSessionService(IHttpClientFactory clientFactory, IRouteBuilder routeBuilder, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _routeBuilder = routeBuilder;
            _configuration = configuration;
        }

        public async Task<AuthenticationOperationStatusResponse> AuthenticateAsync()
        {
            var client = _clientFactory.CreateClient("KsefApi");

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "auth/challenge");

            var response = await client.SendAsync(httpRequest);

            var content = await response.Content.ReadAsStringAsync();

            // ✅ 200 – OK
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get challenge from KSEF API. Status code: {response.StatusCode}, Response: {content}");
            }

            string contextIdentifier = "5090878038";

            var challengeResponse =
                    JsonSerializer.Deserialize<KsefChallengeResponse>(
                        content,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

            var authIdentifierType = AuthenticationTokenSubjectIdentifierTypeEnum.CertificateSubject;

            AuthenticationTokenAuthorizationPolicy? authorizationPolicy = null;

            AuthenticationTokenRequest authTokenRequest =
            AuthTokenRequestBuilder
                .Create()
                .WithChallenge(challengeResponse.Challenge)
                .WithContext(AuthenticationTokenContextIdentifierType.Nip, contextIdentifier)
                .WithIdentifierType(authIdentifierType)    // optional
                .WithAuthorizationPolicy(authorizationPolicy)    // optional
                .Build();

             string unsignedXml = AuthenticationTokenRequestSerializer.SerializeToXmlString(authTokenRequest);

            //X509Certificate2 certificate =
            //GetPersonalCertificate("Jan", "Kowalski", "TINPL", contextIdentifier, "M B");


            string path = _configuration["Ksef:CertificatePath"]
                ?? throw new InvalidOperationException("Ksef:CertificatePath is not configured");
            string password = _configuration["Ksef:CertificatePassword"]
                ?? throw new InvalidOperationException("Ksef:CertificatePassword is not configured");

            X509Certificate2 certificate = new X509Certificate2(
                path,
                password,
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.PersistKeySet |
                X509KeyStorageFlags.Exportable
            );

            string signedXml = SignatureService.Sign(unsignedXml, certificate);

            ArgumentException.ThrowIfNullOrWhiteSpace(signedXml);

            var authOperationInfo = await SubmitXadesAuthRequestAsync(signedXml,client,false);

            //bool verifyCertificateChain = false;

            //string url = $"auth/xades-signature?verifyCertificateChain={verifyCertificateChain.ToString().ToLower(CultureInfo.CurrentCulture)}";

            //var httpRequestWithSignedXml = new HttpRequestMessage(HttpMethod.Post, url);

            //httpRequestWithSignedXml.Content = new StringContent(signedXml, Encoding.UTF8, "application/xml");

            //var authOperationInfoResponse = await client.SendAsync(httpRequestWithSignedXml);

            //var authOperationInfoResponseContent = await authOperationInfoResponse.Content.ReadAsStringAsync();

            //if (!authOperationInfoResponse.IsSuccessStatusCode)
            //{
            //    throw new Exception($"Failed to get challenge from KSEF API. Status code: {response.StatusCode}, Response: {content}");
            //}

            //var /*authOperationInfo*/ = JsonSerializer.Deserialize<SignatureResponse>(
            //    authOperationInfoResponseContent,
            //    new JsonSerializerOptions
            //    {
            //        PropertyNameCaseInsensitive = true
            //    });



            AuthStatus? authorizationStatus;
            int maxRetry = 5;
            int currentLoginAttempt = 0;
            TimeSpan sleepTime = TimeSpan.FromSeconds(1);

            do
            {
                if (currentLoginAttempt >= maxRetry)
                {
                    throw new InvalidOperationException("Autoryzacja nieudana - przekroczono liczbę dozwolonych prób logowania.");
                }

                await Task.Delay(sleepTime + TimeSpan.FromSeconds(currentLoginAttempt));

                ArgumentException.ThrowIfNullOrWhiteSpace(authOperationInfo.AuthenticationToken.Token);



                authorizationStatus = await GetAuthStatusAsync(
                    authOperationInfo.ReferenceNumber,
                    authOperationInfo.AuthenticationToken.Token,
                    client); ;

                currentLoginAttempt++;
            }
            while (authorizationStatus.Status.Code != 200);

            // Uzyskanie accessToken w celu uwierzytelniania 
            AuthenticationOperationStatusResponse? accessTokenResult = await GetAccessTokenAsync(authOperationInfo.AuthenticationToken.Token, client);

            return accessTokenResult;
        }

        public async Task<AuthStatus?> GetAuthStatusAsync(string authenticationToken, string authOperationReferenceNumber, HttpClient client)
        {
            if (string.IsNullOrWhiteSpace(authenticationToken))
                throw new ArgumentException("authenticationToken cannot be null or whitespace", nameof(authenticationToken));

            string endpoint = Routes.Authorization.Status(Uri.EscapeDataString(authOperationReferenceNumber));

            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationToken);

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var authStatus = System.Text.Json.JsonSerializer.Deserialize<AuthStatus>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return authStatus;
        }

        // Twój zmodyfikowany SubmitXadesAuthRequestAsync
        public async Task<SignatureResponse?> SubmitXadesAuthRequestAsync(
            string signedXML,
            HttpClient client,
            bool verifyCertificateChain = false)
        {
            if (string.IsNullOrWhiteSpace(signedXML))
                throw new ArgumentException("signedXML cannot be null or whitespace", nameof(signedXML));

            string endpoint = Routes.Authorization.XadesSignature +
                $"?verifyCertificateChain={verifyCertificateChain.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture)}";

            string path = _routeBuilder.Build(endpoint);

            using var request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Content = new StringContent(signedXML, System.Text.Encoding.UTF8, "application/xml");

            using var response = await client.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var error = JsonSerializer.Deserialize<ErrorResponse>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (error?.Exception?.ExceptionDetailList != null && error.Exception.ExceptionDetailList.Any())
                    {
                        var messages = error.Exception.ExceptionDetailList
                            .Select(e => $"Code: {e.ExceptionCode}, Description: {e.ExceptionDescription}, Details: {string.Join(", ", e.Details ?? new List<string>())}");

                        throw new InvalidOperationException($"KSeF API returned error(s): {string.Join(" | ", messages)}");
                    }
                }
                catch (JsonException)
                {
                    // Jeżeli JSON nie jest parsowalny, rzuć oryginalny błąd
                    throw new InvalidOperationException($"KSeF API returned status {response.StatusCode}: {content}");
                }
            }

            // Jeżeli status OK, deserializujemy normalnie
            return JsonSerializer.Deserialize<SignatureResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<AuthenticationOperationStatusResponse?> GetAccessTokenAsync(
        string authenticationToken,
        HttpClient client)
        {
            if (string.IsNullOrWhiteSpace(authenticationToken))
                throw new ArgumentException("authenticationToken cannot be null or whitespace", nameof(authenticationToken));

            using var request = new HttpRequestMessage(HttpMethod.Post, Routes.Authorization.Token.Redeem);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationToken);
            request.Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json"); // jeśli nie ma body, wysyłamy pusty JSON

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<AuthenticationOperationStatusResponse>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result;
        }

        /// <summary>
        /// Tworzy testowy, samopodpisany certyfikat przeznaczony do składania podpisu (XAdES).
        /// </summary>
        /// <param name="givenName">Imię właściciela certyfikatu.</param>
        /// <param name="surname">Nazwisko właściciela certyfikatu.</param>
        /// <param name="serialNumberPrefix">Prefiks numeru seryjnego.</param>
        /// <param name="serialNumber">Numer seryjny.</param>
        /// <param name="commonName">Wspólna nazwa (CN) certyfikatu.</param>
        /// <param name="encryptionType">Rodzaj certyfikatu</param>
        /// <returns><see cref="X509Certificate2"/> będący samopodpisanym certyfikatem do podpisu.</returns>
        public static X509Certificate2 GetPersonalCertificate(
            string givenName,
            string surname,
            string serialNumberPrefix,
            string serialNumber,
            string commonName,
            EncryptionMethodEnum encryptionType = EncryptionMethodEnum.Rsa
            )
        {
            X509Certificate2 certificate = SelfSignedCertificateForSignatureBuilder
                        .Create()
                        .WithGivenName(givenName)
                        .WithSurname(surname)
                        .WithSerialNumber($"{serialNumberPrefix}-{serialNumber}")
                        .WithCommonName(commonName)
                        .AndEncryptionType(encryptionType)
                        .Build();
            return certificate;
        }

        public enum EncryptionMethodEnum
        {
            ECDsa,
            Rsa
        }

        /// <summary>
        /// Pomocnicza klasa startowa do tworzenia buildera certyfikatu do podpisu.
        /// </summary>
        public static class SelfSignedCertificateForSignatureBuilder
        {
            /// <summary>
            /// Tworzy nowy builder samopodpisanego certyfikatu do podpisu.
            /// </summary>
            /// <returns>Interfejs startowy buildera.</returns>
            public static ISelfSignedCertificateForSignatureBuilder Create() =>
                SelfSignedCertificateForSignatureBuilderImpl.Create();
        }

        
    }
}
