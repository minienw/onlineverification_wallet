using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using CheckInValidation.Client.Core;
using CheckInValidation.Client.Core.Api.Validation;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NCrunch.Framework;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;

namespace CheckInValidation.Client.Tests;

//NB these tests are INTEGRATION tests and do not succeed unless both the airline stub and validation service (and redis server) are running.
public class ThirdPartyServicesTests
{
    //Remember to get a new token from whichever env u r testing
    //private const string QrCodeContentsBase64 = "eyJwcm90b2NvbCI6IkRDQ1ZBTElEQVRJT04iLCJwcm90b2NvbFZlcnNpb24iOiIyLjAwIiwic2VydmljZUlkZW50aXR5IjoiaHR0cDovL2xvY2FsaG9zdDo4MDgxL2lkZW50aXR5IiwicHJpdmFjeVVybCI6InByaXZhY3kgcG9saWN5IHVybC4uLiIsInRva2VuIjoiZXlKcmFXUWlPaUpNTmxSamFtNWlXbWRsTkQwaUxDSmhiR2NpT2lKU1V6STFOaUo5LmV5SnBjM04xWlhJaU9pSm9kSFJ3T2k4dmEyVnNiR0ZwY2k1amIyMGlMQ0pwWVhRaU9qRXdNREF3TURBd0xDSnpkV0lpT2lJd01USXpORFUyTnpnNVFVSkRSRVZHTURFeU16UTFOamM0T1VGQ1EwUkZSaUlzSW1WNGNDSTZNakF3TURBd01EQjkuSXcwMzNwT0RoMkpLckRaVzEwVHEyX3dBemxUaG5UbW5ZQmxQdHlhNXdyVjQ0LTA4ZHBSN2hIbnNzRDBSekVFZ3dPck5DeEt0NWkyY2p3SFZpVDhWbXpWelFVTy1ZOXR6aERTaFZ6aWZWZDhTRVU2VzZ0OWc4Z21SZE1KWFhPd2tGc0Y1Y0dCMUdDc0JfcjFoRGVLcF81Q3hEbXc5RDRKX2NQNDF1MVNqc3UtZUZLN2lBSm5BaUpsbUZpOHNneE9JQ0hGelFHWmswanBwRmlhQXhCSm1qdVUwR0hkUWJLNk9RT09LR29MUmVNYXNneS1uYk5yOUFNcHFoRnRlRFVheWlQODFVelBFdFctcGdLaE5IaHp6RHU4NW5DNkMzZG1DRUd2aDkxNUJ4WWZBNElhZ1JXd21YeGtEX3Y2dlVFWnVXM0Ftb3l0SHJfcURRSnNYbnRLNVB5cFRnMmNRbVhBZFBZWUhyNjBESWtHTFJ3eWYxa3hYam9TTjNBeVYyUDFIOEotZmRmanRCZnFuelp0VnFVTk1tajV0b24yOEtvSDlsVFdYeDBoT01iTEZaNDlMU3NMTG9UNzlNYTROeklWTG5ZQkc4aVdqem1ZTG1aYlJpeGk5ZU9VUmI3ZlNRYi12ZXhHTTF0LS1EajM1WWVNUXp6Nkx1UEVjXzViYl8xYVotZEFCSHI2VExKTTBnSTJhNURFVW5ObW11RFNlLWZCd2pERXBrTDh2M1BzcGF0OVhGekhqQWNYT1Y3OGJvam93UzdLY2V6cFJRTU1ZdnR4NTFFVWsyNnl5ckdHU3JjMEdDNFJTaTUyUUFuYkc5QllHMWh0NEluVnhPOVY3ZWRHSmdMWXV5VnZRdGJ4R1U2cEJTN05uMUtLbzE4VmJ5aWNhQkNmR0h0VHVycmMiLCJjb25zZW50IjoiaW5mb3JtZWQgY29uc2VudCB0ZXh0Li4uIiwic3ViamVjdCI6IjAxMjM0NTY3ODlBQkNERUYwMTIzNDU2Nzg5QUJDREVGIiwic2VydmljZVByb3ZpZGVyIjoiS2VsbGFpciJ9";
    //e.g. localhost:8081/starthere2
    private const string QrCodeContentsBase64 =
        "eyJraWQiOiJMNlRjam5iWmdlND0iLCJhbGciOiJSUzI1NiJ9.eyJpc3N1ZXIiOiJodHRwOi8va2VsbGFpci5jb20iLCJpYXQiOjE2NTM5MjUzODEsImV4cCI6MTY2MjU2NTM4MSwic3ViIjoiREExMDEwQjI3MkE2NEE4MDhBMzU2RkNEQzA2N0U1NEQifQ.LvtMGcPkDv3f6jgprFWMAesgrkJGsRfZqj4_aBITN0MXXt_y1ec6wION7KMgEYBlg-nDIbZG6aGzvvkQoULwRlS5Th3iDohwegSa5JzpVfsLGEz8Qx4Gps0DnxGl_Yq7EYNO2Hx_YPai7Drbnx8R4qBW5sCG0vHn-JRhlMiRluaXi88costN3iU2fvtf8JRvZe0nrLtUw1jSdSXBTR6XiXADYDcgV7xPsct6NlSuBYU0-2jdIEX1NXrBvIMvuNYmtcwB9Q6_qJ3AeyFB-9x-pBC5JtTVXRDhWkc9p__YMWvQIlEJwlfRJ8CANUvHADGdRIcdfRdtg9EZWO5Ql1INULIlACp9WwJlvb8N-TNNpJ8024h-7OGFQV9OR6nLzzUp04BB1nNtwQzi4Ttin1FiNixBOKbSVQsO56HwtJDyCpbTpI9jqhHtyX4P8GPw03i7JfWsMafN-uBlye79OFjRwnG2MjFxOsOJUChrZIxjd1Rw9OFQlVaJ94IBV4xyN21NvDEJnzCzyz0lVEGvFofZuVm5Fb4SUo_s5pmFJs0AmruPf2pxW1yp_PXCp-cGUiLgqkWOE-VCjAdg-oO1-zAnHANSyp82rDteSMVTR9vXZZksrxjrVWpiFqI1OfS2_Mn1Yd5eY3OPyf1_TPuS1FRbct1sSzh7aX7FcY_6aFL6qE8";

    //Still has to work with an actual DCC string
    private const string Dcc =
        "HC1:NCF7X3W08$$Q030:+HGHBFO026M6M4NHSU5S8CUHC9:R7YL8YW7.6JUVM0$U7UQ%-B/$5X$59498$BLQH*CT4TJ603TRTIHKK5H6%EJTEPTSIWEJFAYMPRMQ%BOPUGIL3P2CHZMZ35D:2G7JU5E0B180OZ0W7:O/C5TAVVO5X$D+BTE$C:EWP 1Z$29%IDM5-6Q+F56U0ZBI0*8UEADP2LBLM9QH7HWPIYMGCXA5773R7 HFOV0-VG::AP9POXD6-J/WM4*R*O7HZSJG3NYTLQFT5D51V 4C5D8M-JOYLNTC*%MG5LWI1 H7%DNGUQ%3RQ$H4DQULL905%*1ZV6S9GIUHL103BHVUVZU9 0I%:DVF1H21LCCLCB$W4HVN%2BN7CLK07BG/PS:W3$M91WI5N02 QASQNY81+6BJ9OFERUK-AHWU5I7L-%2ZY9J/G:-A/UFGIIETL6 G41W00JIRP-NMJLP.W7 7BHJ0J V%%HR/HMWQG+CVX9NQABH8129 G6KD38L4I6G$6WT.73ZR80WH+582DUZ1O3G.9NGWNTZJ*.6NCVLUR0IH2SLH.SMPU 0PL:MDAOENSYUIJ9NH9OK8BZ6S:38IME6OE*QVUSR1FW+:QU0";

    private readonly IWebAssemblyHostEnvironment _Env;
    private readonly ILogger<VerificationWorkflow> _Logger;
    private readonly ILoggerFactory _LoggerFactory;

    [Fact]
    public void AirlineIdentity()
    {
        var fac = new Mock<IHttpClientFactory>();
        fac.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient());

        var subject = new HttpGetIdentityCommand(fac.Object);
        var response = subject.ExecuteAsync("http://localhost:9001/identity").GetAwaiter().GetResult();
        Assert.True(response.id.StartsWith("http") && response.id.EndsWith("/identity"));
    }

    [Fact]
    public void SigTest()
    {
        var keyPair = Crypto.GenerateEcKeyPair();
        Trace.WriteLine("Public: " + keyPair.Public.EncodeDerBase64());
        //Trace.WriteLine("Private: " + keyPair.EncodePrivateKeyDerBase64());
        var test = "test123";
        var sig = Crypto.Digest(Encoding.UTF8.GetBytes(test), keyPair.Private);

        Trace.WriteLine("Sig: " + Convert.ToBase64String(sig));
    }

    [Fact]
    public (AsymmetricCipherKeyPair keyPair, HttpPostTokenCommandResult response) AirlineToken()
    {
        var fac = new Mock<IHttpClientFactory>();
        fac.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

        var keyPair = Crypto.GenerateEcKeyPair();
        Trace.WriteLine("Public: " + keyPair.Public.EncodeDerBase64());
        Trace.WriteLine("Private: " + keyPair.EncodePrivateKeyDerBase64());

        var initiatingToken = QrCodeContentsBase64;
        var args = new HttpPostTokenCommandArgs
        {
            AccessTokenServiceUrl = "http://localhost:9001/token",
            InitiatingToken = initiatingToken,
            WalletPublicKey = keyPair.Public
        };

        var subject = new HttpPostTokenCommand(fac.Object);
        var response = subject.ExecuteAsync(args).GetAwaiter().GetResult();
        //Assert.Equal("http://localhost:8081/identity", id.id);

        return (keyPair, response);
    }

    public byte[] Digest(byte[] payload, string privateKey)
    {
        var bytes = Convert.FromBase64String(privateKey);
        var span = new ReadOnlySpan<byte>(bytes);
        var alg = ECDsa.Create();
        alg.ImportECPrivateKey(span, out var _);
        //var signer = SignerUtilities.GetSigner(X9ObjectIdentifiers.ECDsaWithSha256.Id);
        return alg.SignData(payload, HashAlgorithmName.SHA256);
        //signer.BlockUpdate(payload, 0, payload.Length);
        //return signer.GenerateSignature();
    }

    //[Fact]
    //public void MakeValidateRequest()
    //{
    //    var dccArtifact = File.ReadAllBytes(GetProjectFileName("..\\DccParserTests\\Bobby.jpg"));
    //    var iv =Convert.FromBase64String("EVW+otSpl+JG3EXq5m3qWA==");
    //    var secretKey = RandomNumberGenerator.GetBytes(32);
    //    var encryptedDcc = Crypto.EncryptAesCbc(dccArtifact, secretKey, iv);
    //    var publicKey = Crypto.GetRsaPublicKey("MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEArTl66RAAnSYtDm6rb2dCclaozgPSr6Oxu/whHlknTke5E28YGiIWKSiuTR530GO/Wie22FHIIUoIyaZT6mmCC3XTZiQ8V+fqFGaqr7uQooNzJT6sNXRj+iqZxueDKEClry/6Rsq8mfZw+K7UD7hdn9EWfFR5VWY+PgbWPZkSaRVldCpjZrNwECAsyBNTFSDZcMJ7hoofrp/g5+qms8OjwPuc1Jw3yg0qNVig3sSDNqbXSkGimrmWWCpGZ255zCgVJbQTwOgRqrpZAoIq2sJNdKaVQ8aCwKQeZo85jcXS1iB8meG0GFiWI8/A8+mNodiAZNLxxrbiRFkh6posVbmxo/gyvlVmyaYXg09CZrNNCmicTyQ4tC7Oz0PNrr+/ZQA7UvyPnPQs1j9YGCeG1HhHwT58d9d6/01a29YHuxa+bwr/Qey4QEOX+n1+DDTGrRN9TySr/+uP+CJk2yeXBwHbywKPfC/3mOur47jCyy3aaozWkDsSZsNePfHpPjULyawt817IQ6/b3Le0oklmlpB8I+5BeicO8oEmPoFr9QCq6IxhJ1RDNJquESX5s71HS3Y8nZ98TQrZUpigI+w06IsaQgR4VCVhbn5LvE93A+RWOldaM+WvpZwHh4UoUHOBPmxof8cb5xoCUBbgel/ASMz66H9zSiFWBr2c3lXafbfMV20CAwEAAQ==");
    //    var encryptedSecretKey = publicKey.Encrypt(secretKey, RSAEncryptionPadding.OaepSHA256);
    //    var digest = Digest(encryptedDcc, "MIIBUQIBAQQgXHK9867y3YR98P5ibYDpnOXVnizKloYF12mUEQPMd5GggeMwgeACAQEwLAYHKoZIzj0BAQIhAP////////////////////////////////////7///wvMEQEIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABCAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABwRBBHm+Zn753LusVaBilc6HCwcCm/zbLc4o2VnygVsW+BeYSDradyajxGVdpPv8DhEIqP0XtEimhVQZnEfQj/sQ1LgCIQD////////////////////+uq7c5q9IoDu/0l6M0DZBQQIBAaFEA0IABBuArigwIKTwhJ9P1FyfFzGQrJMfu7ibINRwvyNLUAnFctrep0iqqlT7y45qYQt/MfhvTCyPHJMevpfjNDKvuV4=");

    //    var body = new ValidationRequestBody
    //    {
    //        kid = "8s/rDoZc3G4 =",
    //        dcc = Convert.ToBase64String(encryptedDcc),
    //        sig = Convert.ToBase64String(digest),
    //        sigAlg = "SHA256withECDSA",
    //        encKey = Convert.ToBase64String(encryptedSecretKey),
    //        encScheme = "RsaOaepWithSha256AesCbcScheme",
    //    };

    //    Trace.WriteLine(JsonConvert.SerializeObject(body, Formatting.Indented));
    //}

    private static string GetProjectFileName(string name)
    {
        return Path.Combine(Path.GetDirectoryName(NCrunchEnvironment.GetOriginalProjectPath()), name);
    }


    //TODO does not encrypt DCC
    //[Fact]
    //public void Validate()
    //{
    //    var fac = new Mock<IHttpClientFactory>();
    //    fac.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient());

    //    var dbp = new Mock<IDialogService>();
    //    dbp.Setup(x => x.ShowMessageBox(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
    //        It.IsAny<string>(), null)).Returns(() => Task.FromResult<bool?>(true));

    //    var loggerMock = new Mock<ILogger<VerificationWorkflow>>();
    //    //TODO
    //    var logger = loggerMock.Object;

    //    var workflow = new VerificationWorkflow(
    //        new HttpPostTokenCommand(fac.Object),
    //        new HttpPostValidateCommand(fac.Object),
    //        new HttpGetIdentityCommand(fac.Object),
    //        new HttpPostCallbackCommand(fac.Object),
    //        logger,
    //        dbp.Object
    //    );

    //    workflow.OnInitializedAsync(QrCodeContentsBase64);
    //    Assert.True(!workflow.Exiting);

    //    workflow.ValidateDccAsync(Encoding.UTF8.GetBytes(Dcc)).GetAwaiter().GetResult();

    //    //TODO need better test data BUT the services are producing the appropriate results
    //    Assert.False(!workflow.Exiting);
    //    Trace.WriteLine(workflow.WorkflowMessage);
    //    Assert.True(workflow.WorkflowMessage.StartsWith("Security token received from DCC va"));

    //    workflow.NotifyServiceProvider().GetAwaiter().GetResult();
    //    Assert.True(workflow.ShowNotify);
    //}
}