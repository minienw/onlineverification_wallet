namespace CheckInValidation.Client.Core.Api.Token
{
    internal class TokenRequestBody
    {
        public string pubKey { get; set; }
        public string alg { get; set; }
        //public string service { get; set; }
    }
}