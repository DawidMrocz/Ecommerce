using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Gateway.Api.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum KsefContextType
    {
        [EnumMember(Value = "Nip")]
        Nip,

        [EnumMember(Value = "InternalId")]
        InternalId,

        [EnumMember(Value = "NipVatUe")]
        NipVatUe,

        [EnumMember(Value = "PeppolId")]
        PeppolId
    }

    public enum ErrorCode
    {
        UnreadableContent = 21001,
        InvalidAuthorizationChallenge = 21111,
        InvalidCertificate = 21115,
        InvalidEntityIdentifierForContextType = 21117,
        InvalidCharacterEncoding = 21217,
        DocumentNotCompliantWithSchema = 21401,
        SignatureAndAuthenticationTypeConflict = 21406,
        InvalidDocument = 9101,
        MissingSignature = 9102,
        ExceededAllowedNumberOfSignatures = 9103,
        InvalidSignature = 9105,
        InputDataValidationError = 21405
    }

  

}
