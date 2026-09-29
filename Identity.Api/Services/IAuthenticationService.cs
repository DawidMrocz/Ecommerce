using Identity.Api.ApiModels;
using Identity.Api.DTO;

namespace Identity.Api.Services
{
    public interface IAuthenticationService
    {
        Task Register(RegisterRequest request);
        Task<AuthResponse> Login(LoginRequest request, string idAddress);
        Task<ProfileResponse> Profile(int id);
        Task<AuthResponse> RefreshToken(string refreshToken, string ipAddress);
        Task AddPhoto(IFormFile file, int userId);
        Task<(byte[] fileContent, string mimeType, string fileName)> GetPhoto(int userId);
    }
}
