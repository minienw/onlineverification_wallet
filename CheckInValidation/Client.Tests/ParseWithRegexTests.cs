using CheckInValidation.Client.Core.Api.Identity;
using NCrunch.Framework;

namespace CheckInValidation.Client.Tests;

public class ParseWithRegexTests
{
    private const string RegexValue = "^http.*#ConfirmationService(?:[-]{1}[0-9]+)?$";

    private static string GetProjectFileName(string name)
    {
        return Path.Combine(Path.GetDirectoryName(NCrunchEnvironment.GetOriginalProjectPath()), name);
    }

    [InlineData("https://pinggg.mywire.org/wallet/identity/v2#ConfIrmationService")]
    [InlineData("https://pinggg.mywire.org/wallet/identity/v2#ConfirmationService-1")]
    [InlineData("https://pinggg.mywire.org/wallet/identity/v2#ConfirmationService-99")]
    [Theory]
    public void Parse(string text)
    {
        Assert.True(IdentityQueries.Hit(text, "ConfirmationService"));
    }

    [InlineData("https://pinggg.mywire.org/wallet/identity/v2#ConfirmationService-99B")]
    [InlineData("https://pinggg.mywire.org/wallet/identity/v2#ConfirmationService-")]
    [Theory]
    public void ParseFails(string text)
    {
        Assert.False(IdentityQueries.Hit(text, "ConfirmationService"));
    }
}