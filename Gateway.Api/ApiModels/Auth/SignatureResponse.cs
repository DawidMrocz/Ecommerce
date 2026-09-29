namespace Gateway.Api.ApiModels.Auth
{
    public class SignatureResponse
    {
        /// <summary>
        /// Numer referencyjny.
        /// </summary>
        public string ReferenceNumber { get; set; } = null!;

        /// <summary>
        /// Token uwierzytelniający.
        /// </summary>
        public OperationToken AuthenticationToken { get; set; } = null!;

    }

    public class OperationToken
    {
        /// <summary>
        /// Token uwierzytelniający służący do dostępu do chronionych zasobów API.
        /// </summary>
        public string Token { get; set; } = null!;

        /// <summary>
        /// Czas wygaśnięcia tokenu.
        /// </summary>
        public DateTime ValidUntil { get; set; }
    }
}
