using Identity.Api.Data;
using Identity.Api.Helpers;
using Identity.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MroczwareFramework.Services.FileService;
using System.Security.Cryptography;

namespace Identity.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
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



            builder.Services.AddDbContext<UserDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

            builder.Services
             .AddAuthentication("Bearer")
             .AddJwtBearer("Bearer", options =>
             {
                 string publicKeyBase64 = builder.Configuration["JWT:PublicKeyPem"] ?? throw new Exception("JWT Public key not configured");
                 string publicKeyPem =
                 "-----BEGIN RSA PUBLIC KEY-----\n" +
                 RsaHelper.FormatBase64String(publicKeyBase64) +
                 "\n-----END RSA PUBLIC KEY-----";
                 RSA rsa = RsaHelper.CreateRsaFromPkcs1(publicKeyPem);

                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateAudience = true,
                     ValidAudience = builder.Configuration["JWT:Audience"],
                     ValidateIssuer = true,
                     ValidIssuers = new[] { builder.Configuration["JWT:Issuer"] },
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = new RsaSecurityKey(rsa),
                     RequireExpirationTime = true,
                     ValidateLifetime = true,
                     RequireSignedTokens = true,
                 };
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
