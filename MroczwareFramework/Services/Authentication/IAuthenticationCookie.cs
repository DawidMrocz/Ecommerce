using MroczwareFramework.ApiModels.Access;
using MroczwareFramework.DataModels.Authentication;

namespace MroczwareFramework.Services.Authentication
{
    public interface IAuthenticationCookie
    {
        Task<string> Login(LoginRequest dto);
        Task Logout();
        Task<AuthenticationUser?> GetAuthenticatedUser();
    }
}
