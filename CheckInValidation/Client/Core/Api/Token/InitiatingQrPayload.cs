namespace CheckInValidation.Client.Core.Api.Token
{
    public class InitiatingQrPayload
    {
        public string Protocol { get; set; }
        public string ProtocolVersion { get; set; }
        public string ServiceIdentity { get; set; }
        public string PrivacyUrl { get; set; }
        public string Token { get; set; }
        public string Consent { get; set; }
        public string Subject { get; set; }
        public string ServiceProvider { get; set; }
    }
}
