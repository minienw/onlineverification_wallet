namespace CheckInValidation.Client.Core.Api.Validation
{
    public class ValidationRequestBody
    {
        public string kid { get; set; }
        public string dcc { get; set; }
        public string sig { get; set; }
        public string encKey { get; set; }
        public string encScheme { get; set; }
        public string sigAlg { get; set; }
    }
}