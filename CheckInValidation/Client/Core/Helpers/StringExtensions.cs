using System.IdentityModel.Tokens.Jwt;

namespace CheckInValidation.Client.Core.Helpers
{
    public static class StringExtensions
    {
        public static JwtSecurityToken ToJwtSecurityToken(this string jwtTokenString)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwtTokenString);
            var result = jsonToken as JwtSecurityToken;
            return result;
        }
        
        //TODO is this all that is accepted?
        public static bool IsInternationalDccString(this string dccQrJson)
        {
            return dccQrJson?.StartsWith("HC1:") ?? false;
        }
    }
}
