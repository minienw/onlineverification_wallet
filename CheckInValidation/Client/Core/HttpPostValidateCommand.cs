using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CheckInValidation.Client.Core.Api.Validation;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;

namespace CheckInValidation.Client.Core;

public class HttpPostValidateCommand 
{
    private readonly IHttpClientFactory _HttpClientFactory;

    public HttpPostValidateCommand(IHttpClientFactory httpClientFactory)
    {
        _HttpClientFactory = httpClientFactory;
    }

    public async Task<HttpPostValidateResult> Execute(HttpPostValidateArgs args)
    {
        var secretKey = RandomNumberGenerator.GetBytes(32);
        var encryptedDcc = Crypto.EncryptAesGcm(args.DccArtifact, secretKey, args.IV);
        var encryptedSecretKey = Crypto.EncryptRsaOepMfg1(secretKey, Crypto.DecodeRsaPublicKeyDerBase64(args.PublicKeyJwk.x5c[0]));
        var digest = Crypto.Digest(args.DccArtifact, args.WalletPrivateKey);

        var body = new ValidationRequestBody
        {
            kid = args.PublicKeyJwk.kid,
            dcc = Convert.ToBase64String(encryptedDcc),
            sig = Convert.ToBase64String(digest),
            sigAlg = "SHA256withECDSA",
            encKey = Convert.ToBase64String(encryptedSecretKey),
            encScheme = "RsaOaepWithSha256AesGcmScheme"
        };

        var endpoint = args.ValidatorAccessTokenObject.Payload.Aud.First();

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Headers =
            {
                { "authorization", new AuthenticationHeaderValue("Bearer", args.ValidatorAccessToken).ToString() },
                {"X-Version", "2.00"}
            },
            Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
        };

        using var httpClient = _HttpClientFactory.CreateClient(nameof(HttpPostValidateCommand));
        var httpResponse = await httpClient.SendAsync(httpRequestMessage);
        var content = await httpResponse.Content.ReadAsStringAsync();

        if (!httpResponse.IsSuccessStatusCode)
            throw new Exception($"Http response code: {httpResponse.StatusCode}, message: {content}");

        return new()
        {
            Content = content
        };
    }
}