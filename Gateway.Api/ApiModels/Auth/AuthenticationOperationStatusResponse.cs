namespace Gateway.Api.ApiModels.Auth
{
    public class AuthenticationOperationStatusResponse
    {
        public OperationToken AccessToken { get; set; }
        public OperationToken RefreshToken { get; set; }
    }
}
