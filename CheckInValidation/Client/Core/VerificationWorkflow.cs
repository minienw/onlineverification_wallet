using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckInValidation.Client.Core.Api.Identity;
using CheckInValidation.Client.Core.Api.Token;
using CheckInValidation.Client.Core.Helpers;
using MudBlazor;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;

namespace CheckInValidation.Client.Core
{
    //Scoped
    public class VerificationWorkflow
    {
        //Error or finished
        public bool Exiting { get; private set; }
        public string WorkflowMessage { get; private set; } //Error message or try different DCC.

        //After initialize success
        public bool ShowDccUpload { get; private set; }

        //After dcc upload/verification attempt
        public bool AfterDccValidation { get; private set; }
        public DccExtract DccExtract { get; private set; }

        public FailureItem[] FailureMessages { get; private set; }
        public List<string> DebugMessages { get; } = new();

        private readonly HttpGetIdentityCommand _HttpGetIdentityCommand;
        private readonly HttpPostTokenCommand _HttpPostTokenCommand;
        private readonly HttpPostValidateCommand _HttpPostValidateCommand;
        private readonly HttpPostCallbackCommand _HttpPostCallbackCommand;

        public InitiatingQrPayload InitiatingQrPayload { get; private set; }
        public JwtSecurityToken ResultToken { get; private set; }
        public bool ShowNotify { get; private set; }

        private JwtSecurityToken _InitiatingQrPayloadToken;
        private string _ResultTokenServiceUrl;
        private HttpPostTokenCommandResult _PostTokenResult;
        private AsymmetricCipherKeyPair _WalletSigningKeyPair;

        private readonly ILogger _Logger;
        private string _AccessTokenServiceUrl;
        private HttpPostValidateResult _HttpPostValidateResult;
        private string _ResultClaimValue;

        private readonly IDialogService _DialogService;

        public VerificationWorkflow(HttpPostTokenCommand httpPostTokenCommand, HttpPostValidateCommand httpPostValidateCommand, HttpGetIdentityCommand httpGetIdentityCommand, HttpPostCallbackCommand httpPostCallbackCommand, ILogger<VerificationWorkflow> logger, IDialogService dialogService)
        {
            _HttpGetIdentityCommand = httpGetIdentityCommand;
            _HttpPostTokenCommand = httpPostTokenCommand;
            _HttpPostValidateCommand = httpPostValidateCommand;
            _HttpPostCallbackCommand = httpPostCallbackCommand;
            _DialogService = dialogService;
            _Logger = logger;
        }

        public async Task OnInitializedAsync(string qrBytesBase64)
        {
            _Logger.LogDebug($"Starting.");
            _Logger.BeginScope(nameof(OnInitializedAsync));
            try
            {
                try
                {
                    var valueBytes = Convert.FromBase64String(qrBytesBase64);
                    var jsonData = Encoding.UTF8.GetString(valueBytes);
                    InitiatingQrPayload = JsonConvert.DeserializeObject<InitiatingQrPayload>(jsonData);
                    _InitiatingQrPayloadToken = InitiatingQrPayload?.Token.ToJwtSecurityToken();
                    _Logger.LogInformation($"Initiating QR Code: {jsonData}");
                }
                catch (Exception e)
                {
                    DebugMessages.Add($"Error reading Initiating QR Code: {e}");
                    _Logger.LogError($"Error reading Initiating QR Code: {e}");
                    WorkflowMessage = "Could not understand QR code. Unable to continue.";
                    Exiting = true;
                    return;
                }

                IdentityResponse airlineIdentity;
                try
                {
                    airlineIdentity = await _HttpGetIdentityCommand.ExecuteAsync(InitiatingQrPayload.ServiceIdentity);
                }
                catch (Exception e)
                {
                    DebugMessages.Add($"Error contacting service provider: {e}");
                    _Logger.LogError($"Error contacting service provider: {e}");
                    WorkflowMessage = "Error contacting service provider. Unable to continue.";
                    Exiting = true;
                    return;
                }

                try
                {
                    _AccessTokenServiceUrl = airlineIdentity.GetServiceUrl("AccessTokenService");
                    _ResultTokenServiceUrl = airlineIdentity.GetServiceUrl("ResultTokenService");
                    _Logger.LogInformation($"AccessTokenServiceUrl :{_AccessTokenServiceUrl}");
                    _Logger.LogInformation($"ResultTokenServiceUrl :{_ResultTokenServiceUrl}");
                    DebugMessages.Add($"AccessTokenServiceUrl :{_AccessTokenServiceUrl}");
                    DebugMessages.Add($"ResultTokenServiceUrl :{_ResultTokenServiceUrl}");
                    ShowDccUpload = true;
                }
                catch (Exception e)
                {
                    var m = $"Error reading identity document of service provider: {e}";
                    DebugMessages.Add(m);
                    _Logger.LogError(m);
                    Exiting = true;
                }

            }
            finally
            {
                _Logger.LogDebug("End initialise.");
            }
        }

        //'By clicking “Upload” and selecting a QR code you will be sending you DCC containing personal data to the server that will validate it for your travel. Make sure you expect to do so. If you are not checking in for a trip abroad, close your browser screen.;By selecting OK you will be sending the validation result containg personal data to the transport company. Only do so if you are actually checking in.'
        /// <summary>
        /// Actual DCC extracted from provided file or the raw contents of the file.
        /// </summary>
        /// <returns></returns>
        public async Task ValidateDccAsync(byte[] dccArtifact)
        {

            _Logger.LogDebug("Start validation.");
            var args = new HttpPostValidateArgs();

            var consented = await _DialogService.ShowMessageBox("Continue?", InitiatingQrPayload.Consent.Split(";")[0], yesText: "Yes", noText: "No");

            if (!consented ?? false)
            {
                WorkflowMessage = "You have chosen not to consent to send your DCC for verification at this time.";
                Exiting = true;
                return;
            }
            
            try
            {
                try
                {
                    _WalletSigningKeyPair = Crypto.GenerateEcKeyPair();
                    _Logger.LogInformation("Wallet key pair generated.");
                }
                catch (Exception e) //TODO narrower...
                {
                    var m = $"Did not generate wallet key pair: {e}";
                    _Logger.LogError(m);
                    DebugMessages.Add(m);
                    WorkflowMessage = "Could not complete initialisation of validation (Code 1). Please try again later.";
                    Exiting = true;
                    return;
                }

                try
                {
                    _PostTokenResult = await _HttpPostTokenCommand.ExecuteAsync(new HttpPostTokenCommandArgs
                    {
                        AccessTokenServiceUrl = _AccessTokenServiceUrl,
                        InitiatingToken = InitiatingQrPayload.Token,
                        WalletPublicKey = _WalletSigningKeyPair.Public,
                    });
                    var m = "POST token completed.";
                    _Logger.LogInformation(m);
                    DebugMessages.Add(m);
                }
                catch (Exception e) //TODO narrower...
                {
                    var m = $"POST token failed: {e}";
                    _Logger.LogError(m);
                    DebugMessages.Add(m);
                    WorkflowMessage = "Could not complete initialisation of validation (Code 2). Please try again later.";
                    Exiting = true;
                    return;
                }


                try
                {
                    var encKeyJwk = JsonConvert.DeserializeObject<PublicKeyJwk>(Encoding.UTF8.GetString(Convert.FromBase64String(_PostTokenResult.EncKeyBase64)));
                    args.InitiatingQrPayloadToken = _InitiatingQrPayloadToken;
                    args.DccArtifact = dccArtifact;
                    args.PublicKeyJwk = encKeyJwk;
                    args.IV = _PostTokenResult.Nonce;
                    args.WalletPrivateKey = _WalletSigningKeyPair.Private;
                    args.ValidatorAccessTokenObject = _PostTokenResult.ValidationAccessTokenPayload;
                    args.ValidatorAccessToken = _PostTokenResult.ValidationAccessToken;
                    _HttpPostValidateResult = await _HttpPostValidateCommand.Execute(args);
                    var m = "POST validate completed.";
                    _Logger.LogInformation(m);
                    DebugMessages.Add(m);
                }
                catch (Exception e) //TODO narrower...
                {
                    var m = $"POST validate failed: {e}";
                    _Logger.LogError(m);
                    DebugMessages.Add(m);
                    WorkflowMessage = "Could not complete validation. Please try again later.";
                    Exiting = true;
                    return;
                }

                DebugMessages.Add($"Validation response: {JsonConvert.SerializeObject(_HttpPostValidateResult)}");
                ResultToken = (JwtSecurityToken)(new JwtSecurityTokenHandler().ReadToken(_HttpPostValidateResult.Content));

                _ResultClaimValue = ResultToken.Claims.FirstOrDefault(x => x.Type == "result")?.Value;
                if (_ResultClaimValue == null)
                {
                    var m = $"Result is missing entry for 'result'.";
                    _Logger.LogError(m);
                    DebugMessages.Add(m);
                    WorkflowMessage = "Unexpected response from validation service. Please try again later.";
                    Exiting = true;
                    return;
                }

                var validationSucceeded = _ResultClaimValue!.Equals("OK", StringComparison.InvariantCultureIgnoreCase);
                if (!validationSucceeded)
                {
                    try
                    {
                        FailureMessages = ResultToken.Claims
                            .Where(x => x.Type == "results") //Flat and repeated.
                            .Select(x => JsonConvert.DeserializeObject<FailureItem>(x?.Value))
                            .ToArray();

                        if (FailureMessages.Length == 0)
                        {
                            FailureMessages = new[] { new FailureItem { type = "-", ruleIdentifier = "-", customMessage = "DCC Health Certificate QR Code could not be verified." } };
                        }

                        AfterDccValidation = true;
                        ShowNotify = false;
                        Exiting = true;
                        return;
                    }
                    catch (Exception ex) //TODO narrower!
                    {
                        var m = $"Result could not parse entry for Validation Failures: {ex}";
                        _Logger.LogError(m);
                        DebugMessages.Add(m);
                        WorkflowMessage = "Unexpected response from validation service. Please try again later.";
                        return;
                    }
                }

                //Validation succeeded - parse results
                try
                {
                    var dccExtractJson = ResultToken.Claims.FirstOrDefault(x => x.Type == "personalinfodccextract")?.Value;
                    if (dccExtractJson == null)
                    {
                        var m = $"Result is missing entry for 'personalInfoExtract'.";
                        _Logger.LogError(m);
                        DebugMessages.Add(m);
                        WorkflowMessage = "Unexpected response from validation service. Please try again later.";
                        Exiting = true;
                    }
                    DccExtract = JsonConvert.DeserializeObject<DccExtract>(dccExtractJson);
                    AfterDccValidation = true;
                    ShowNotify = true;
                }
                catch (Exception ex) //TODO narrower!
                {
                    var m = $"Result could not parse entry for DCC Extract: {ex}";
                    _Logger.LogError(m);
                    DebugMessages.Add(m);
                    WorkflowMessage = "Unexpected response from validation service. Please try again later.";
                }
            }
            catch (Exception e) //TODO narrower!
            {
                var m = $"Unexpected error {e}.";
                _Logger.LogError(m);
                DebugMessages.Add(m);
                WorkflowMessage = "There was an unexpected error while validating. Please try again later.";
                Exiting = true;
            }
            finally
            {
                //Hint to memory manager
                args = null;
                dccArtifact = null;
                _Logger.LogDebug("End validation.");
            }
        }

        public async Task NotifyServiceProvider()
        {
            _Logger.LogDebug("Start notification.");

            var consented = await _DialogService.ShowMessageBox("Continue?", InitiatingQrPayload.Consent.Split(";")[^1], yesText: "Yes", noText: "No");
            if (!consented ?? false)
            {
                WorkflowMessage = "You have chosen not to consent to send your result to the service provider at this time.";
                Exiting = true;
                return;
            }

            try
            {
                var args = new HttpPostCallbackCommandArgs
                {
                    EndpointUri = _ResultTokenServiceUrl,
                    ResultToken = ResultToken.Claims.First(x => x.Type == "confirmation").Value,
                    ValidationAccessToken = _PostTokenResult.ValidationAccessToken
                };

                if (!await _HttpPostCallbackCommand.ExecuteAsync(args))
                {
                    var m = $"Could not complete validation. Please try again later.";
                    _Logger.LogError(m);
                    DebugMessages.Add(m);
                    WorkflowMessage = "Unexpected response notifying service provider. Please try again later.";
                    Exiting = true;
                }
                else
                {
                    var m = $"Service provider notified.";
                    _Logger.LogInformation(m);
                    DebugMessages.Add(m);
                    WorkflowMessage = "You have completed verification and notified the service provider.";
                    Exiting = true;
                }
            }
            catch (Exception e)
            {
                var m = $"Unexpected error {e}.";
                _Logger.LogError(m);
                DebugMessages.Add(m);
                WorkflowMessage = "There was an unexpected error while notifying the service provider. Please try again later.";
                Exiting = true;
            }
            finally
            {
                _Logger.LogDebug("End notification.");
            }
        }
    }
}
