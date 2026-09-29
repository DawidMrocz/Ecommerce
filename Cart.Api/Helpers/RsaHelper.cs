using System.Security.Cryptography;
using System.Text;

namespace Identity.Api.Helpers
{
    public static class RsaHelper
    {
        public static string FormatBase64String(string base64)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < base64.Length; i += 64)
                sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
            return sb.ToString();
        }

        public static RSA CreateRsaFromPkcs1(string publicKeyPem)
        {
            string publicKeyBase64 = publicKeyPem
                .Replace("-----BEGIN RSA PUBLIC KEY-----", "")
                .Replace("-----END RSA PUBLIC KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", "");

            byte[] pkcs1Bytes = Convert.FromBase64String(publicKeyBase64);
            byte[] pkcs8Bytes = ConvertPkcs1ToPkcs8(pkcs1Bytes);
            RSA rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(pkcs8Bytes, out _);
            return rsa;
        }

        public static byte[] ConvertPkcs1ToPkcs8(byte[] pkcs1Bytes)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(pkcs1Bytes, out _);
                return rsa.ExportSubjectPublicKeyInfo();
            }
        }
    }
}
