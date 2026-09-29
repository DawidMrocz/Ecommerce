using MroczwareFramework.Models.Setting;

namespace MroczwareFramework.Services.SettingService
{
    public interface ISettingService
    {
        Task<string> Get(string strongName);
    }
}
