using MroczwareFramework.DataModels.Authentication;

namespace MroczwareFramework.ApiModels.User
{
    public class SessionUser : AuthenticationUser
    {
        public string CsrfToken { get; set; } = null!;
    }
}
