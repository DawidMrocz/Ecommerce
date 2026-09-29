using Microsoft.EntityFrameworkCore;
using MroczwareFramework.Models.Country;
using MroczwareFramework.Models.Culture;
using MroczwareFramework.Models.Role;
using MroczwareFramework.Models.Setting;
using MroczwareFramework.Models.Template;
using MroczwareFramework.Models.User;

namespace MroczwareFramework.Data
{
    public class FrameworkDbContext : DbContext
    {
        public FrameworkDbContext(DbContextOptions options) : base(options) { }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<UserSessionModel> UserSessions { get; set; }
        public DbSet<RoleModel> Roles { get; set; }
        public DbSet<CountryModel> Countries { get; set; }
        public DbSet<CultureModel> Cultures { get; set; }
        public DbSet<SettingModel> Settings { get; set; }
        public DbSet<TemplateModel> Templates { get; set; }
    }
}
