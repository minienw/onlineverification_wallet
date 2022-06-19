using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CheckInValidation.Client.Core.Api.Callback;
using Newtonsoft.Json;

namespace CheckInValidation.Client.Core;

public class HttpPostCallbackCommand 
{
    private readonly IHttpClientFactory _HttpClientFactory;

    public HttpPostCallbackCommand(IHttpClientFactory httpClientFactory)
    {
        _HttpClientFactory = httpClientFactory;
    }

    public async Task<bool> ExecuteAsync(HttpPostCallbackCommandArgs args)
    {
        var body = new CallbackRequestBody
        {
            confirmationToken = args.ResultToken
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, args.EndpointUri)
        {
            Headers =
            {
                { "authorization", new AuthenticationHeaderValue("Bearer", args.ValidationAccessToken).ToString() },
                {"X-Version", "2.00"}
            },
            Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
        };

        using var httpClient = _HttpClientFactory.CreateClient(nameof(HttpPostValidateCommand));
        var httpResponse = await httpClient.SendAsync(httpRequestMessage);
        return httpResponse.IsSuccessStatusCode;
    }
}