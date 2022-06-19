namespace CheckInValidation.Client.Core
{
    public class FailureItem
    {
        public string type { get; set; }
        public string customMessage { get; set; }
        public string ruleIdentifier { get; set; }
    }

    
    public class DccExtract
    {
        public string fnt { get; set; }
        public string gnt { get; set; }
        public string dob { get; set; }
    }
}
