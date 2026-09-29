using Microsoft.EntityFrameworkCore;
using MroczwareFramework.Attributes.DependencyInjection;
using MroczwareFramework.Data;
using MroczwareFramework.Models.Setting;

namespace MroczwareFramework.Services.SettingService
{
    [DependencyInjection(typeof(ISettingService))]
    internal class SettingService<TDbContext> : ISettingService
        where TDbContext : FrameworkDbContext
    {
        private readonly DbSet<SettingModel> _dbSet;

        public SettingService(TDbContext dbContext)
        {
            _dbSet = dbContext.Set<SettingModel>();
        }
        public async Task<string> Get(string key)
        {
            var settingModel = await _dbSet.FirstOrDefaultAsync(s => s.Key == key) ?? throw new Exception($"Brak settinga {key} w bazie danych");
            return settingModel.Value;
        }
    }
}
