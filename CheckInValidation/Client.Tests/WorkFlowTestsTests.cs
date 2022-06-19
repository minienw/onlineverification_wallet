using CheckInValidation.Client.Core;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckInValidation.Client.Tests;

public class WorkFlowTestsTests
{
    private const string QrCodeContentsBase64_Identity_localhost_8081 =
        "eyJwcm90b2NvbCI6IkRDQ1ZBTElEQVRJT04iLCJwcm90b2NvbFZlcnNpb24iOiIyLjAwIiwic2VydmljZUlkZW50aXR5IjoiaHR0cDovL2xvY2FsaG9zdDo4MDgxL2lkZW50aXR5IiwicHJpdmFjeVVybCI6InByaXZhY3kgcG9saWN5IHVybC4uLiIsInRva2VuIjoiZXlKcmFXUWlPaUpNTmxSamFtNWlXbWRsTkQwaUxDSmhiR2NpT2lKU1V6STFOaUo5LmV5SnBjM04xWlhJaU9pSm9kSFJ3T2k4dmEyVnNiR0ZwY2k1amIyMGlMQ0pwWVhRaU9qRXdNREF3TURBd0xDSnpkV0lpT2lJd05rUkROVUpGUTBNeVJFWTBPVGMzT1RKQk9EUkJSRVZHTTBFNE56azROQ0lzSW1WNGNDSTZNakF3TURBd01EQjkuVngzc3BjakxuQVZlNUpEN0o4ZG9FVWtZMU1feDNqMVZoX1UteldpOFBnMmF3Uml3bG52YVJHdHgwNVB0NmNhLVoyd200V3l3aEh6allBVFBZeXNmSGx1NW5TcXBiNlpGS2JYQ1VqRXhZczB1T0liQzd0SThWYlBhbHZmeDBqNFZ0V051OVh1bVB5QjdaSmlLWkp6cXdTdTFBWkNqNHNQbEZxVURyQk1qcDMteEtaeWVSUW9iNzlldXlRQlI5dG5CaTY3UWZjWnlCbU54d20wMWFQSC15c05NRG5hME5sbG40RlBiaWRHRUtiRXNnNDVOWkthckFmNk40REhHR1htMGtWRzQ2V2VpRG81c3VQUUNUcl9nNWtBWWZnakxyR0c0NlZFUHFmNnFuN0sxb0RZWGE3Z25RYWhvMFRJWWQyYTlxLVFaUGxib2VhQ0M5SkdUWnFrWGVOVWdLbmlWRk04LXVGanltdEZBT21WNVFJaUtGbUpnZ1Z4Ym44QzFvZU5tMklfWUVjS05ZcVlBU0FiekFZVmFUMHJkM1VUWHFXdm1yZHBiQllXVHFMSXpSczFUTi14cDM3ZWhGWUxwa1Zud3VqMndxMVltYkNBUlJRZ08zcHgyc0lBNE9xdjEzS0JQSF92QkJ3RWROaHRncHpGNXdLWFVRU1dnZFJ6aXNsS0laRkJRb3hQb3FQeG1rRHB5YUM0bXhIaHV4Rk1heTFDYWxyQTVnV295YjZORnpoenpGVUNjRXAtdWViUGdKX3RNaDJwSlQwYXRTOWV5TGNHVUt2ZzFZX1BxLTI0NVNTczAyVXJ0cUhEMGdIWDhibE9Ib1JjbVNfMkhPbkdkNUxfZUFFYm9ibGs2dzJoanhrZkFtZnlfRFJOMDZnUmMxa1d2OXVrUUQ5M3MxQlEiLCJjb25zZW50IjoiQnkgY2xpY2tpbmcg4oCcVXBsb2Fk4oCdIGFuZCBzZWxlY3RpbmcgYSBRUiBjb2RlIHlvdSB3aWxsIGJlIHNlbmRpbmcgeW91IERDQyBjb250YWluaW5nIHBlcnNvbmFsIGRhdGEgdG8gdGhlIHNlcnZlciB0aGF0IHdpbGwgdmFsaWRhdGUgaXQgZm9yIHlvdXIgdHJhdmVsLiBNYWtlIHN1cmUgeW91IGV4cGVjdCB0byBkbyBzby4gSWYgeW91IGFyZSBub3QgY2hlY2tpbmcgaW4gZm9yIGEgdHJpcCBhYnJvYWQsIGNsb3NlIHlvdXIgYnJvd3NlciBzY3JlZW4uO0J5IHNlbGVjdGluZyBPSyB5b3Ugd2lsbCBiZSBzZW5kaW5nIHRoZSB2YWxpZGF0aW9uIHJlc3VsdCBjb250YWluZyBwZXJzb25hbCBkYXRhIHRvIHRoZSB0cmFuc3BvcnQgY29tcGFueS4gT25seSBkbyBzbyBpZiB5b3UgYXJlIGFjdHVhbGx5IGNoZWNraW5nIGluLiIsInN1YmplY3QiOiIwNkRDNUJFQ0MyREY0OTc3OTJBODRBREVGM0E4Nzk4NCIsInNlcnZpY2VQcm92aWRlciI6IktlbGxhaXIifQ==";

    private readonly ILogger<VerificationWorkflow> _Logger;
    //private readonly ILoggerFactory _LoggerFactory;

    public WorkFlowTestsTests()
    {
        //_LoggerFactory = LoggerFactory.Create(builder => builder.); 
        var loggerMock = new Mock<ILogger<VerificationWorkflow>>();
        //TODO
        _Logger = loggerMock.Object;

        var m1 = new Mock<IWebAssemblyHostEnvironment>();
        m1.Setup(x => x.Environment).Returns("Development");
    }

    [InlineData(null)]
    [InlineData("dsdfsdffsd")]
    [Theory]
    public async Task BadQrCode(string data)
    {
        //VerificationWorkflow(httpPostTokenCommand, httpPostValidateCommand, httpGetIdentityCommand, httpPostCallbackCommand, ILogger < VerificationWorkflow > logger, IDialogService dialogService, IWebAssemblyHostEnvironment env)
        var w = new VerificationWorkflow(null, null, null, null, _Logger, null);
        await w.OnInitializedAsync(data);
        Assert.StartsWith("Could not understand QR code", w.WorkflowMessage);
        Assert.Null(w.InitiatingQrPayload);
        Assert.True(w.Exiting);
    }

    [Fact]
    public async Task BadIdentityUriInQrCode()
    {
        var w = new VerificationWorkflow(null, null, null, null, _Logger, null);
        await w.OnInitializedAsync(QrCodeContentsBase64_Identity_localhost_8081);
        Assert.StartsWith("Error contacting", w.WorkflowMessage);
        Assert.Equal("http://localhost:8081/identity", w.InitiatingQrPayload.ServiceIdentity);
        Assert.True(w.Exiting);
    }
}