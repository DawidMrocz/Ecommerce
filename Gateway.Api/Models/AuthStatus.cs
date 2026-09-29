namespace Gateway.Api.Models
{
    public class AuthStatus
    {
        public DateTimeOffset StartDate { get; set; }
        public AuthenticationMethodEnum AuthenticationMethod { get; set; }
        public OperationStatusInfo Status { get; set; }
        public bool? IsTokenRedeemed { get; set; }
        public DateTimeOffset? LastTokenRefreshDate { get; set; }
        public DateTimeOffset? RefreshTokenValidUntil { get; set; }
    }

    public enum AuthenticationMethodEnum
    {
        Token,
        TrustedProfile,
        InternalCertificate,
        QualifiedSignature,
        QualifiedSeal,
        PersonalSignature,
        PeppolSignature
    }

    public class OperationStatusInfo
    {
        public int Code { get; set; }
        public string Description { get; set; }
        public ICollection<string> Details { get; set; }
    }
}
