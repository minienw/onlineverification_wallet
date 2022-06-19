namespace CheckInValidation.Client.Core.Api.Identity;

public class VerificationMethod
{
    public string id { get; set; }
    public string type { get; set; }
    public string controller { get; set; }
    public PublicKeyJwk publicKeyJwk { get; set; }
    public string[] verificationMethods { get; set; }
}