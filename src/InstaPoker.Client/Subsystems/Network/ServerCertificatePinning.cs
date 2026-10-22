using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace InstaPoker.Client.Subsystems.Network;

/// <summary>
/// Why a server certificate was rejected.
/// </summary>
public enum ServerCertificateFailure {
    /// <summary>The certificate was accepted.</summary>
    None,

    /// <summary>The server did not present a certificate.</summary>
    Missing,

    /// <summary>The certificate is not valid for the requested host name.</summary>
    NameMismatch,

    /// <summary>The certificate is outside its validity period.</summary>
    Expired,

    /// <summary>The certificate's public key differs from the key pinned in this client.</summary>
    KeyMismatch
}

/// <summary>
/// Validates the server certificate against the public key pinned in the client assembly.
/// </summary>
/// <remarks>
/// The server certificate is self-signed, so the normal chain of trust cannot validate it. Instead, the
/// client compares the public key presented during the handshake with the public key embedded at build time.
/// The certificate name and validity period are still checked; chain errors are expected and ignored.
/// </remarks>
public static class ServerCertificatePinning {

    private const string ResourceName = "InstaPoker.Client.Certs.server.crt";

    private static readonly Lazy<byte[]> pinnedPublicKey = new(LoadPinnedPublicKey);

    public static bool Validate(object? sender, X509Certificate? certificate, X509Chain? chain,
        SslPolicyErrors errors) {
        return Check(certificate, errors) is ServerCertificateFailure.None;
    }

    public static ServerCertificateFailure Check(X509Certificate? certificate, SslPolicyErrors errors) {
        if (certificate is null || errors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable)) {
            return ServerCertificateFailure.Missing;
        }

        if (errors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch)) {
            return ServerCertificateFailure.NameMismatch;
        }

        using X509Certificate2 presented = X509CertificateLoader.LoadCertificate(certificate.GetRawCertData());

        DateTime now = DateTime.UtcNow;
        if (now < presented.NotBefore.ToUniversalTime() || now > presented.NotAfter.ToUniversalTime()) {
            return ServerCertificateFailure.Expired;
        }

        byte[] publicKey = presented.GetPublicKey();
        return CryptographicOperations.FixedTimeEquals(publicKey, pinnedPublicKey.Value)
            ? ServerCertificateFailure.None
            : ServerCertificateFailure.KeyMismatch;
    }

    private static byte[] LoadPinnedPublicKey() {
        using Stream? resource = typeof(ServerCertificatePinning).Assembly.GetManifestResourceStream(ResourceName);
        if (resource is null) {
            throw new InvalidOperationException($"Embedded certificate \"{ResourceName}\" was not found.");
        }

        using MemoryStream buffer = new();
        resource.CopyTo(buffer);
        using X509Certificate2 pinned = X509CertificateLoader.LoadCertificate(buffer.ToArray());
        return pinned.GetPublicKey();
    }
}
