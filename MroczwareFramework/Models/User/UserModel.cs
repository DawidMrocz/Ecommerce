using MroczwareFramework.Models.Role;
using System.Text.Json.Serialization;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace MroczwareFramework.Models.User
{
    [Index(nameof(Email), IsUnique = true)]
    public class UserModel : BaseModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public RoleModel Role { get; set; } = null!;
        public int RoleId { get; set; }
        [JsonIgnore]
        public string PasswordHash { get; set; } = null!;
        public bool Blocked { get; set; } = false;
        public DateTime? UnblockTime { get; set; }

        public bool EmailConfirmed { get; set; } = false;
        public string? ActivationToken { get; set; }
        public DateTime? ActivationTokenExpiry { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public string SecurityStamp { get; set; }
    }
}
