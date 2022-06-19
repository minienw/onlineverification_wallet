//namespace CheckInQrWeb.Core.Models
//{
//    public class DccValidationResponse
//    {
//        public object Actor { get; set; }
//        public object[] Audiences { get; set; }
//        public Claim[] Claims { get; set; }
//        public string EncodedHeader { get; set; }
//        public string EncodedPayload { get; set; }
//        public Header Header { get; set; }
//        public object Id { get; set; }
//        public string Issuer { get; set; }
//        public Payload Payload { get; set; }
//        public object InnerToken { get; set; }
//        public object RawAuthenticationTag { get; set; }
//        public object RawCiphertext { get; set; }
//        public string RawData { get; set; }
//        public object RawEncryptedKey { get; set; }
//        public object RawInitializationVector { get; set; }
//        public string RawHeader { get; set; }
//        public string RawPayload { get; set; }
//        public string RawSignature { get; set; }
//        public object SecurityKey { get; set; }
//        public string SignatureAlgorithm { get; set; }
//        public object SigningCredentials { get; set; }
//        public object EncryptingCredentials { get; set; }
//        public object SigningKey { get; set; }
//        public string Subject { get; set; }
//        public DateTime ValidFrom { get; set; }
//        public DateTime ValidTo { get; set; }
//        public DateTime IssuedAt { get; set; }
//    }

//    public class Header
//    {
//        public string typ { get; set; }
//        public string kid { get; set; }
//        public string alg { get; set; }
//    }

//    public class Payload
//    {
//        public string sub { get; set; }
//        public string iss { get; set; }
//        public int iat { get; set; }
//        public int exp { get; set; }
//        public object[][] category { get; set; }
//        public string confirmation { get; set; }
//        public Result[] results { get; set; }
//        public string result { get; set; }
//    }

//    public class Result
//    {
//        public object[] identifier { get; set; }
//        public object[] result { get; set; }
//        public object[] type { get; set; }
//        public object[] details { get; set; }
//    }

//    public class Claim
//    {
//        public string Issuer { get; set; }
//        public string OriginalIssuer { get; set; }
//        public Properties Properties { get; set; }
//        public object Subject { get; set; }
//        public string Type { get; set; }
//        public string Value { get; set; }
//        public string ValueType { get; set; }
//    }

//    public class Properties
//    {
//    }

//}
