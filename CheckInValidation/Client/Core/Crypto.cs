using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace CheckInValidation.Client.Core;

public static class Crypto
{
    public static byte[] EncryptAesCbc(byte[] plainText, byte[] secretKey, byte[] iv)
    {
        var cipher = CipherUtilities.GetCipher("AES/CBC/PKCS7");
        cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", secretKey), iv));
        return cipher.DoFinal(plainText);
    }

    public static byte[] EncryptAesGcm(byte[] plainText, byte[] secretKey, byte[] iv)
    {
        var cipher = CipherUtilities.GetCipher("AES/GCM");
        cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", secretKey), iv, iv.Length / 8, iv.Length)); //TODO I think this is wrong...
        return cipher.DoFinal(plainText);
    }

    public static RsaKeyParameters DecodeRsaPublicKeyDerBase64(string value)
        => (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(value));
    

    public static byte[] EncryptRsaOepMfg1(byte[] value, RsaKeyParameters publicKey)
    {
        var cipher = new OaepEncoding(new RsaEngine(), new Sha256Digest(), new Sha256Digest(), null);
        cipher.Init(true, publicKey);
        return cipher.ProcessBlock(value, 0, value.Length);
    }


    public static byte[] Digest(byte[] payload, AsymmetricKeyParameter privateKey)
    {
        var signer = SignerUtilities.GetSigner(X9ObjectIdentifiers.ECDsaWithSha256.Id);
        signer.Init(true, privateKey);
        signer.BlockUpdate(payload, 0, payload.Length);
        return signer.GenerateSignature();
    }

    public static string EncodeDerBase64(this AsymmetricKeyParameter key) => Convert.ToBase64String(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(key).GetDerEncoded());
    public static string EncodePrivateKeyDerBase64(this AsymmetricCipherKeyPair keys)
    {
        var textWriter = new StringWriter();
        var pemWriter = new PemWriter(textWriter);
        pemWriter.WriteObject(keys.Private);
        pemWriter.Writer.Flush();
        return textWriter.ToString();
    }
    public static AsymmetricCipherKeyPair GenerateEcKeyPair()
    {
        var domainParams = new ECDomainParameters(ECNamedCurveTable.GetByName("secp256k1"));
        var keyParams = new ECKeyGenerationParameters(domainParams, new SecureRandom());
        var generator = new ECKeyPairGenerator("ECDSA");
        generator.Init(keyParams);
        return generator.GenerateKeyPair();
    }
}