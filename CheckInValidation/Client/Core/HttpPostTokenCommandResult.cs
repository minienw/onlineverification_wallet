using System.IdentityModel.Tokens.Jwt;

namespace CheckInValidation.Client.Core
{
    public class HttpPostTokenCommandResult
    {
        public byte[] Nonce { get; set; }
        public JwtSecurityToken ValidationAccessTokenPayload { get; set; }
        public string ValidationAccessToken { get; set; }
        public string EncKeyBase64 { get; set; }
        public string SigKeyBase64 { get; set; }
    }
}
