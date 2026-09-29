using Identity.Api.ApiModels;
using Identity.Api.Data;
using Identity.Api.DTO;
using Identity.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MroczwareFramework.Services.FileService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Identity.Api.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        private readonly UserDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;

        public AuthenticationService(
            IConfiguration configuration,
            UserDbContext dbContext,
            IFileService fileService)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _fileService = fileService;
        }

        public async Task<ProfileResponse> Profile(int userId)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => new ProfileResponse
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email
                })
                .FirstOrDefaultAsync()
                ?? throw new Exception("Nie znaleziono użytkownika");
        }

        public async Task<AuthResponse> Login(LoginRequest request, string idAddress)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(request.Email))
                ?? throw new Exception("Nie znaleziono użytkownika lub nie poprawne hasło");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new Exception("Nie poprawne hasło");

            var createAccessTokenResponse = CreateAccessToken(user);
            var createRefreshTokenResponse = await CreateRefreshToken(user.Id, idAddress);

            return new AuthResponse()
            {
                AccessToken = createAccessTokenResponse.accessToken,
                RefreshToken = createRefreshTokenResponse.refreshToken.Token,
                AccessTokenExpiresIn = createAccessTokenResponse.expiresIn,
                RefreshTokenExpiresIn = createRefreshTokenResponse.expiresIn
            };
        }

        public async Task AddPhoto(IFormFile file, int userId)
        {
            var user = await _dbContext.Users.FindAsync(userId)
                ?? throw new Exception("Nie znaleziono użytkownika");

            if (user.Photo is not null)
            {
                _fileService.DeleteFromDisc(user.Photo.Name, user.Photo.Guid);
                _dbContext.Photos.Remove(user.Photo);
            }

            Guid photoGuid = Guid.NewGuid();

            var photoFile = await _fileService.Create(file, photoGuid);

            var photo = new Photo
            {
                Name = photoFile.fileName,
                Guid = photoGuid
            };

            user.Photo = photo;

            await _dbContext.SaveChangesAsync();
        }


        public async Task<(byte[] fileContent, string mimeType, string fileName)> GetPhoto(int userId)
        {
            var user = await _dbContext.Users.Include(u => u.Photo).FirstOrDefaultAsync( u => u.Id == userId)
                ?? throw new Exception("Nie znaleziono użytkownika lub nie poprawne hasło");

            if (user.Photo is null) throw new Exception("Użytkownik nie ma zdjęcia");

            return await _fileService.GetFilesFromDisc(user.Photo.Name, user.Photo.Guid);

        }

        public async Task Register(RegisterRequest request)
        {
            var model = new User()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12)
            };

            await _dbContext.Users.AddAsync(model);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Odświeżenie tokena
        /// </summary>
        /// <param name="incomingRefreshToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AuthResponse> RefreshToken(string refreshToken, string ipAddress)
        {
            var token = await _dbContext.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (token == null || !token.IsActive)
                throw new Exception("Invalid refresh token");

            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;

            var createRefreshTokenResponse = await CreateRefreshToken(token.UserId, ipAddress);

            token.ReplacedByToken = createRefreshTokenResponse.refreshToken.Token;

            var createAccessTokenResponse = CreateAccessToken(token.User);

            await _dbContext.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = createAccessTokenResponse.accessToken,
                AccessTokenExpiresIn = createAccessTokenResponse.expiresIn,
                RefreshToken = createRefreshTokenResponse.refreshToken.Token,
                RefreshTokenExpiresIn = createRefreshTokenResponse.expiresIn
            };
        }

        private (string accessToken, DateTime expiresIn) CreateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FirstName),
                new(ClaimTypes.Surname, user.LastName)
            };

            string privateKey = _configuration["JWT:PrivateKeyPem"] ?? throw new Exception("JWT private key not set");

            using RSA rsa = RSA.Create();

            rsa.ImportRSAPrivateKey(
                source: Convert.FromBase64String(privateKey),
                bytesRead: out int _);

            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSha256
            )
            {
                CryptoProviderFactory = new CryptoProviderFactory
                {
                    CacheSignatureProviders = false
                }
            };

            var expiresIn = DateTime.UtcNow.AddMinutes(15);

            return (new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? throw new Exception("Brak JWT Issuer"),
                audience: _configuration["Jwt:Audience"] ?? throw new Exception("Brak JWT Audience"),
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresIn,
                signingCredentials: signingCredentials
            )), expiresIn);
        }

        /// <summary>
        /// Funbkcja do tworzenia RefreshTokena
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        private async Task<(RefreshToken refreshToken, DateTime expiresIn)> CreateRefreshToken(int userId, string ipAddress)
        {
            var expiresIn = DateTime.UtcNow.AddDays(1);
            var model = new RefreshToken()
            {
                UserId = userId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = expiresIn,
                CreatedByIp = ipAddress
            };

            await _dbContext.RefreshTokens.AddAsync(model);
            await _dbContext.SaveChangesAsync();

            return (model, expiresIn);
        }
    }
}
