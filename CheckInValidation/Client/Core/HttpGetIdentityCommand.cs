using System.Net.Http;
using System.Threading.Tasks;
using CheckInValidation.Client.Core.Api.Identity;
using Newtonsoft.Json;

namespace CheckInValidation.Client.Core;

public sealed class HttpGetIdentityCommand
{
    private readonly IHttpClientFactory _HttpClientFactory;

    public HttpGetIdentityCommand(IHttpClientFactory httpClientFactory)
    {
        _HttpClientFactory = httpClientFactory;
    }

    public async Task<IdentityResponse> ExecuteAsync(string url)
    {
        using var httpClient = _HttpClientFactory.CreateClient(nameof(HttpGetIdentityCommand));
        var r = await httpClient.GetAsync(url);
        var jsonData = await r.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<IdentityResponse>(jsonData);
    }
}