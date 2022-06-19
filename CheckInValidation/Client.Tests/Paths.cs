using NCrunch.Framework;

namespace CheckInValidation.Client.Tests;

public static class Paths
{
    public static string Bob => Path.Combine(Path.GetDirectoryName(NCrunchEnvironment.GetOriginalProjectPath())
        , @"Bob_Bouwer_1-1-1960_3_3.JPG");

    public static string NotBob => Path.Combine(Path.GetDirectoryName(NCrunchEnvironment.GetOriginalProjectPath())
        , @"Erikea Musermann 12-8-1964.png");

    public static string BobFailsWithNull => Path.Combine(
        Path.GetDirectoryName(NCrunchEnvironment.GetOriginalProjectPath())
        , @"bobby_bouwer is ouwer - fails AF.png");

    public static string BobCroppedFailsWithNull => Path.Combine(
        Path.GetDirectoryName(NCrunchEnvironment.GetOriginalProjectPath())
        , @"bobby_bouwer is ouwer - cropped.png");


    public static string SourceDocument1 => Path.Combine(
        Path.GetDirectoryName(NCrunchEnvironment.GetOriginalProjectPath())
        , @"Test Cert SK 2022-01.pdf");

    public static string SourceDocument2 => Path.Combine(
        Path.GetDirectoryName(NCrunchEnvironment.GetOriginalProjectPath())
        , @"Vacc Cert SK 2022-01.pdf");
}