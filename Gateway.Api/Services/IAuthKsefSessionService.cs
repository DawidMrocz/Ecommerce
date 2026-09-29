using Gateway.Api.ApiModels.Auth;

namespace Gateway.Api.Services
{
    public interface IAuthKsefSessionService
    {
        Task<AuthenticationOperationStatusResponse> AuthenticateAsync();
    }
}
