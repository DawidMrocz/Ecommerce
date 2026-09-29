using MroczwareFramework.Dto.User;
using MroczwareFramework.Models.Role;
using MroczwareFramework.Models.User;

namespace MroczwareFramework.Services.User
{
    public interface IMroczwareUserService
    {
        Task ActivateAccountAsync(int userId, string token);
        Task BlockUserAsync(int userId, DateTime? unblockTime = null);
        Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task ChangeRoleAsync(int userId, RoleModel newRole);
        Task DeleteAsync(int id);
        Task<UserModel?> GetByEmailAsync(string email);
        Task<UserModel?> GetByIdAsync(int id);
        Task<UserModel> Register(RegisterUserDto model);
        Task ResetPasswordAsync(int userId);
        Task UnblockUserAsync(int userId);
        Task<UserModel> UpdateAsync(UserModel model);
    }
}
