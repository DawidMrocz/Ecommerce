
using Gateway.Api.Services;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using IRouteBuilder = Gateway.Api.Services.IRouteBuilder;
using RouteBuilder = Gateway.Api.Services.RouteBuilder;

namespace Gateway.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var identityApiUrl = Environment.GetEnvironmentVariable("IDENTITY_API_URL") ?? "http://identity.api:80";

            builder.Services.AddHttpClient("IdentityApi", client =>
            {
                client.BaseAddress = new Uri(identityApiUrl);
            });

            builder.Services.AddHttpClient("KsefApi", client =>
            {
                client.BaseAddress = new Uri("https://api-demo.ksef.mf.gov.pl/v2/");
            });

            var cartApiUrl = Environment.GetEnvironmentVariable("CART_API_URL") ?? "http://identity.api:8080";

            builder.Services.AddHttpClient("CartApi", client =>
            {
                client.BaseAddress = new Uri(cartApiUrl);
            });

            var productApiUrl = Environment.GetEnvironmentVariable("PRODUCT_API_URL") ?? "http://localhost:8080";

            builder.Services.AddHttpClient("ProductApi", client =>
            {
                client.BaseAddress = new Uri(productApiUrl);
            });

            builder.Services.AddControllers();
            builder.Services.AddScoped<IRouteBuilder, RouteBuilder>();
            builder.Services.AddScoped<IAuthKsefSessionService, AuthKsefSessionService>();
            builder.Services.AddMemoryCache();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();


            builder.Services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                options.SerializerOptions.AllowTrailingCommas = true;

                #if NET10_0_OR_GREATER
                    options.SerializerOptions.AllowOutOfOrderMetadataProperties = true;
                #endif

                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddSwaggerGen(options =>
            {builder.Services.AddMemoryCache();
                options.SwaggerGeneratorOptions.SwaggerDocs.Clear();

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Task API",
                    Version = "v1",
                    Description = "Web Api do tasków",
                });
                // Dodanie opcji autoryzacji w Swaggerze
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                string docFolder = "Doc";
                if (Directory.Exists(docFolder))
                {
                    var docFiles = Directory.GetFiles(docFolder, "*.xml", SearchOption.TopDirectoryOnly);

                    foreach (var doc in docFiles)
                    {
                        options.IncludeXmlComments(doc);
                        options.EnableAnnotations(); // W³¹cza obs³ugê adnotacji
                    }
                }

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();

            app.UseSwagger(); // <<< TO JEST KLUCZOWE

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API v1");
                c.RoutePrefix = "swagger";
            });
            //app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
