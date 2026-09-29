using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MroczwareFramework.ApiModels.Access;
using MroczwareFramework.Data;
using MroczwareFramework.DataModels.Authentication;
using MroczwareFramework.Models.User;
using MroczwareFramework.Services.User;
using System.Security.Claims;

namespace MroczwareFramework.Services.Authentication
{
    public class AuthenticationCookie<TUserModel, TDbContext> : IAuthenticationCookie where TUserModel : UserModel, new() where TDbContext : FrameworkDbContext
    {
        public readonly IConfiguration _configuration;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMroczwareUserService _mroczwareUserService;
        private readonly TDbContext _dbContext;
        public AuthenticationCookie(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IMroczwareUserService mroczwareUserService, TDbContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _mroczwareUserService = mroczwareUserService;
            _dbContext = dbContext;
        }

        public async Task<string> Login(LoginRequest dto)
        {
            var user = await _mroczwareUserService.GetByEmailAsync(dto.Email)
                ?? throw new Exception("Nie znaleziono użytkownika lub niepoprawne hasło");

            if (user.Blocked)
            {
                if (user.UnblockTime == null || user.UnblockTime > DateTime.UtcNow)
                    throw new Exception("Konto zostało zablokowane");
                else
                {
                    user.Blocked = false;
                    user.FailedLoginAttempts = 0;
                    user.UnblockTime = null;
                    await _dbContext.SaveChangesAsync();
                }
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.Blocked = true;
                    user.UnblockTime = DateTime.UtcNow.AddMinutes(15);
                }
                await _dbContext.SaveChangesAsync();
                throw new Exception($"Błędne hasło. Próba {user.FailedLoginAttempts} z 5.");
            }

            user.FailedLoginAttempts = 0;
            await _dbContext.SaveChangesAsync();

            string sessionId = Guid.NewGuid().ToString();
            var session = new UserSessionModel
            {
                Id = sessionId,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            _dbContext.UserSessions.Add(session);
            await _dbContext.SaveChangesAsync();
            var csrfToken = Guid.NewGuid().ToString("N"); // losowy string
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()!),
                new Claim(ClaimTypes.Role, user.Role.StrongName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.MobilePhone, user.Phone ?? string.Empty),
                new Claim(ClaimTypes.DateOfBirth, user.DateOfBirth?.ToShortDateString() ?? string.Empty),
                new Claim(ClaimTypes.Gender, user.Gender ?? string.Empty),
                new Claim("SecurityStamp", user.SecurityStamp),
                new Claim("SessionId", sessionId),
                new Claim("CSRF-TOKEN", csrfToken),
                
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(15),
                IsPersistent = dto.RememberMe
            };

            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties
            );

            _httpContextAccessor.HttpContext!.User = principal;

            // --- Tworzenie własnego tokena CSRF ---
           

            // Zapisz token w cookie
            _httpContextAccessor.HttpContext!.Response.Cookies.Append(
                "CSRF-TOKEN",
                csrfToken,
                new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(15)
                }
            );

            // Zwrot tokena do frontendu (alternatywnie frontend może odczytać cookie)
            return csrfToken;
        }


        public async Task Logout()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                var sessionId = _httpContextAccessor.HttpContext.User.FindFirstValue("SessionId");
                if (!string.IsNullOrEmpty(sessionId))
                {
                    var session = await _dbContext.UserSessions.FindAsync(sessionId);
                    if (session != null)
                    {
                        _dbContext.UserSessions.Remove(session);
                        await _dbContext.SaveChangesAsync();
                    }
                }

                await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }

        public async Task<AuthenticationUser?> GetAuthenticatedUser()
        {
            var userIdentity = _httpContextAccessor.HttpContext?.User.Identity;

            if (userIdentity is not ClaimsIdentity claimsIdentity || !userIdentity.IsAuthenticated)
                return null;

            var userIdentifier = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdentifier) || !int.TryParse(userIdentifier, out var userId))
                return null;

            var user = await _mroczwareUserService.GetByIdAsync(userId);
            if (user == null)
                return null;

            return new AuthenticationUser
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.StrongName
            };
        }
    }
}
