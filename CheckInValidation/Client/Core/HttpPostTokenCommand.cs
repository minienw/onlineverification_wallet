using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CheckInValidation.Client.Core.Api.Token;
using Newtonsoft.Json;

namespace CheckInValidation.Client.Core;

public class HttpPostTokenCommand {
    private readonly IHttpClientFactory _HttpClientFactory;

    public HttpPostTokenCommand(IHttpClientFactory httpClientFactory)
    {
        _HttpClientFactory = httpClientFactory;
    }

    public async Task<HttpPostTokenCommandResult> ExecuteAsync(HttpPostTokenCommandArgs args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        if (args.WalletPublicKey == null)
            throw new ArgumentNullException(nameof(HttpPostTokenCommandArgs.WalletPublicKey));

        if (string.IsNullOrWhiteSpace(args.AccessTokenServiceUrl))
            throw new ArgumentException(nameof(HttpPostTokenCommandArgs.AccessTokenServiceUrl));

        var body = new StringContent(
            JsonConvert.SerializeObject(new TokenRequestBody
            {
                pubKey = args.WalletPublicKey.EncodeDerBase64(),
                alg = "SHA256withECDSA",
            }),
            Encoding.UTF8, "application/json");

        var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            args.AccessTokenServiceUrl)
        {
            Headers =
            {
                { "authorization", new AuthenticationHeaderValue("Bearer", args.InitiatingToken).ToString()},
                {"X-Version", "2.00"}
            },
            Content = body
        };

        using (var client = _HttpClientFactory.CreateClient(nameof(HttpPostTokenCommand)))
        {
            var httpResponse = await client.SendAsync(httpRequestMessage);

            if (!httpResponse.IsSuccessStatusCode)
                throw new Exception($"Unable to get validator token response. QR code might be invalid or expired. Http response code: {(int)httpResponse.StatusCode} {httpResponse.ReasonPhrase}");

            var nonce = httpResponse.Headers.GetValues("x-nonce").FirstOrDefault()
                        ?? throw new InvalidOperationException("Unable to get validator token response: required nonce not found in response headers.");

            var encKey = httpResponse.Headers.GetValues("x-enc").FirstOrDefault()
                        ?? throw new InvalidOperationException("Unable to get validator token response: required encryption key not found in response headers.");

            var sigKey = httpResponse.Headers.GetValues("x-sig").FirstOrDefault()
                        ?? throw new InvalidOperationException("Unable to get validator token response: required sig pub key not found in response headers.");

            var decodedNonce = Convert.FromBase64String(nonce);
            var content = await httpResponse.Content.ReadAsStringAsync();
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(content);
            var validationAccessTokenPayload = jsonToken as JwtSecurityToken ?? throw new Exception("Unable to get Validation Access Token response: no valid security token received.");

            return new()
            {
                Nonce = decodedNonce,
                ValidationAccessToken = content,
                ValidationAccessTokenPayload = validationAccessTokenPayload,
                EncKeyBase64 = encKey,
                SigKeyBase64 = sigKey
            };
        }
    }
}