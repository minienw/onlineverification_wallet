namespace CheckInValidation.Client.Core.Api.Identity;

public class PublicKeyJwk
{
    public string[] x5c { get; set; }
    public string kid { get; set; }
    public string alg { get; set; }
    public string use { get; set; }
}