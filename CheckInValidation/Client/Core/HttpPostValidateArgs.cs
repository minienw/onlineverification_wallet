using System.IdentityModel.Tokens.Jwt;
using CheckInValidation.Client.Core.Api.Identity;
using Org.BouncyCastle.Crypto;

namespace CheckInValidation.Client.Core
{
    public class HttpPostValidateArgs
    {
        /// <summary>
        /// Content of image file or PDF file.
        /// </summary>
        public byte[] DccArtifact { get; set; }
        public AsymmetricKeyParameter WalletPrivateKey { get; set; }
        public JwtSecurityToken InitiatingQrPayloadToken { get; set; }
        public byte[] IV { get; set; }
        public PublicKeyJwk PublicKeyJwk { get; set; }
        public JwtSecurityToken ValidatorAccessTokenObject { get; set; }
        public string ValidatorAccessToken { get; set; }
    }
}
