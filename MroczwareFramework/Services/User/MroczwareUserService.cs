using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MroczwareFramework.Data;
using MroczwareFramework.Dto.User;
using MroczwareFramework.Models.Role;
using MroczwareFramework.Models.User;
using MroczwareFramework.Services.EmailService;
using MroczwareFramework.Services.FileService;
using MroczwareFramework.Services.SettingService;
using System.Security.Cryptography;
using ILogger = MroczwareFramework.Services.LoggerService.ILogger;

namespace MroczwareFramework.Services.User
{
    internal partial class MroczwareUserService<TDbContext> : IMroczwareUserService
        where TDbContext : FrameworkDbContext
    {
        private readonly TDbContext _dbContext;
        private readonly DbSet<UserModel> _dbSet;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;
        private readonly IFileService _fileService;
        private readonly ISettingService _settingService;
        private readonly IConfiguration _configuration;

        public MroczwareUserService(ILogger logger, IFileService fileService, TDbContext dbContext, IEmailService emailService, ISettingService settingService, IConfiguration configuration)
        {
            _logger = logger;
            _fileService = fileService;
            _dbContext = dbContext;
            _dbSet = dbContext.Set<UserModel>();
            _emailService = emailService;
            _settingService = settingService;
            _configuration = configuration;
        }


        public async Task<UserModel> Register(RegisterUserDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var existingUser = await _dbSet
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
                throw new Exception($"Użytkownik z adresem email {dto.Email} już istnieje.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Hasło nie może być puste");

            UserModel model = new();
            model.Email = dto.Email;
            model.FirstName = dto.FirstName;
            model.LastName = dto.LastName;
            model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);

            var role = (await _dbContext.Set<RoleModel>()
                .FirstOrDefaultAsync(r => r.Name == "User"))
                ?? throw new Exception("Nie znaleziono domyślnej roli 'User' w bazie.");

            model.Blocked = true;
            model.Role = role;
            model.UnblockTime = null;
            model.EmailConfirmed = false;
            model.ActivationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            model.ActivationTokenExpiry = DateTime.UtcNow.AddHours(24);

            await _dbSet.AddAsync(model);
            await _dbContext.SaveChangesAsync();

            var host = _configuration["Host"];

            var activationLink = $"{host}/activate?userId={model.Id}&token={model.ActivationToken}";

            var mailFrom = _configuration["Email:User"]
               ?? throw new Exception("Nie ustawiono adresu nadawcy e-maili");

            var bodyParams = new Dictionary<string, string>
            {
                { "activationLink", activationLink }
            };

            await _emailService.SendEmail(model.Email, "AcctivateAccount", mailFrom,"PL",true, bodyParams);

            

            return model;
        }

        public async Task ActivateAccountAsync(int userId, string token)
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("Użytkownik nie istnieje");

            if (user.ActivationToken != token)
                throw new Exception("Nieprawidłowy token aktywacyjny");

            if (user.ActivationTokenExpiry < DateTime.UtcNow)
                throw new Exception("Token aktywacyjny wygasł");

            user.Blocked = false;
            user.EmailConfirmed = true;
            user.ActivationToken = null;
            user.ActivationTokenExpiry = null;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserModel?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<UserModel?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<UserModel> UpdateAsync(UserModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var existingUser = await _dbSet.FirstOrDefaultAsync(u => u.Id == model.Id)
                ?? throw new Exception("Użytkownik nie istnieje");

            if (string.IsNullOrWhiteSpace(model.Email))
                throw new ArgumentException("Email nie może być pusty");

            var emailExists = await _dbSet
                .AnyAsync(u => u.Email.ToLower() == model.Email.ToLower() && u.Id != model.Id);
            if (emailExists)
                throw new Exception("Użytkownik z takim emailem już istnieje");

            existingUser.Email = model.Email;

            var properties = typeof(UserModel).GetProperties()
                .Where(p => p.CanWrite &&
                            p.Name != nameof(UserModel.Id) &&
                            p.Name != nameof(UserModel.PasswordHash) &&
                            p.Name != nameof(UserModel.Role) &&
                            p.Name != nameof(UserModel.RoleId));

            foreach (var prop in properties)
            {
                var value = prop.GetValue(model);
                prop.SetValue(existingUser, value);
            }

            existingUser.Blocked = model.Blocked;
            existingUser.UnblockTime = model.UnblockTime;

            await _dbContext.SaveChangesAsync();

            return existingUser;
        }

        public async Task DeleteAsync(int id)
        {
            var existingUser = await _dbSet.FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new Exception("Użytkownik nie istnieje");

            _dbSet.Remove(existingUser);
            await _dbContext.SaveChangesAsync();
        }

        public async Task ResetPasswordAsync(int userId)
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("Użytkownik nie istnieje");

            var newPassword = GenerateRandomPassword();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);

            await _dbContext.SaveChangesAsync();

            var mailFrom = _configuration["Email:User"]
               ?? throw new Exception("Nie ustawiono adresu nadawcy e-maili");

            var bodyParams = new Dictionary<string, string>
            {
                { "newPassword", newPassword }
            };

            await _emailService.SendEmail(user.Email, "ResetPassword", mailFrom);
        }

        private string GenerateRandomPassword(int length = 12)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_-+=<>?";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("Użytkownik nie istnieje");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                throw new Exception("Błędne aktualne hasło");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);
            await _dbContext.SaveChangesAsync();
        }

        public async Task BlockUserAsync(int userId, DateTime? unblockTime = null)
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("Użytkownik nie istnieje");

            user.Blocked = true;
            user.UnblockTime = unblockTime;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UnblockUserAsync(int userId)
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("Użytkownik nie istnieje");

            user.Blocked = false;
            user.UnblockTime = null;
            await _dbContext.SaveChangesAsync();
        }

        public async Task ChangeRoleAsync(int userId, RoleModel newRole)
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("Użytkownik nie istnieje");

            user.Role = newRole;
            await _dbContext.SaveChangesAsync();
        }
    }
}
