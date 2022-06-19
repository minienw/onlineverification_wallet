namespace CheckInValidation.Client.Core;


public class HttpPostCallbackCommandArgs
{
    public string ResultToken { get; set; }
    public string EndpointUri { get; set; }
    public string ValidationAccessToken { get; set; }
}