using System.Security.Cryptography;
using System.Text.Json;

namespace UOAIO.Update;

public sealed class ReleaseManifestVerifier
{
    public async Task<SignedReleaseManifest> LoadAndVerifyAsync(Uri manifestUri, string publicKeyPemPath, CancellationToken cancellationToken = default)
    {
        string json;
        if (manifestUri.IsFile)
        {
            json = await File.ReadAllTextAsync(manifestUri.LocalPath, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            using HttpClient client = new HttpClient();
            json = await client.GetStringAsync(manifestUri, cancellationToken).ConfigureAwait(false);
        }

        SignedReleaseManifest envelope = JsonSerializer.Deserialize<SignedReleaseManifest>(json, ReleaseManifestSerializer.SerializerOptions)
            ?? throw new InvalidOperationException("Manifest envelope could not be parsed.");

        string publicKeyPem = await File.ReadAllTextAsync(publicKeyPemPath, cancellationToken).ConfigureAwait(false);
        Verify(envelope, publicKeyPem);
        return envelope;
    }

    public void Verify(SignedReleaseManifest envelope, string publicKeyPem)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        byte[] manifestBytes = ReleaseManifestSerializer.GetManifestBytes(envelope.Manifest);
        byte[] signature = Convert.FromBase64String(envelope.SignatureBase64);
        bool isValid = rsa.VerifyData(manifestBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        if (!isValid)
        {
            throw new InvalidOperationException("Release manifest signature verification failed.");
        }
    }
}
