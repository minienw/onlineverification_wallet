namespace CheckInValidation.Client.Core.Api.Identity
{
    public class IdentityResponse
    {
        public string id { get; set; }
        public VerificationMethod[] verificationMethod { get; set; }
        public Service[] service { get; set; }
    }

    //public class VerificationMethod
    //{
    //    public string id { get; set; }
    //    public string type { get; set; }
    //    public string controller { get; set; }
    //    public PublicKeyJwk publicKeyJwk { get; set; }
    //}
}