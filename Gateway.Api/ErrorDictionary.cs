using Gateway.Api.Enums;

namespace Gateway.Api
{
    public static class ErrorDictionary
    {
        private static readonly Dictionary<ErrorCode, string> Descriptions = new()
    {
        { ErrorCode.UnreadableContent, "Unreadable content." },
        { ErrorCode.InvalidAuthorizationChallenge, "Invalid authorization challenge." },
        { ErrorCode.InvalidCertificate, "Invalid certificate." },
        { ErrorCode.InvalidEntityIdentifierForContextType, "Invalid entity identifier for the specified context type." },
        { ErrorCode.InvalidCharacterEncoding, "Invalid character encoding." },
        { ErrorCode.DocumentNotCompliantWithSchema, "Document is not compliant with the schema (XSD)." },
        { ErrorCode.SignatureAndAuthenticationTypeConflict, "Conflict between signature and authentication type." },
        { ErrorCode.InvalidDocument, "Invalid document." },
        { ErrorCode.MissingSignature, "Missing signature." },
        { ErrorCode.ExceededAllowedNumberOfSignatures, "Exceeded the allowed number of signatures." },
        { ErrorCode.InvalidSignature, "Invalid signature." },
        { ErrorCode.InputDataValidationError, "Input data validation error." }
    };

        public static string GetDescription(ErrorCode code)
            => Descriptions[code];
    }
}
