using System.Security.Cryptography.X509Certificates;

namespace InstaPoker.Server;

/// <summary>
/// Provides the TLS certificate used by the server. The certificate and its private key are embedded in the
/// server assembly so the server can be shipped as a single executable.
/// </summary>
public static class ServerTls {

    private const string ResourceName = "InstaPoker.Server.Certs.server.pfx";
    private const string PfxPassword = "InstaPoker-Server-2026";

    private static readonly Lazy<X509Certificate2> certificate = new(LoadCertificate);

    public static X509Certificate2 Certificate => certificate.Value;

    private static X509Certificate2 LoadCertificate() {
        using Stream? resource = typeof(ServerTls).Assembly.GetManifestResourceStream(ResourceName);
        if (resource is null) {
            throw new InvalidOperationException($"Embedded certificate \"{ResourceName}\" was not found.");
        }

        using MemoryStream buffer = new();
        resource.CopyTo(buffer);
        // SChannel (Windows) cannot use an ephemeral private key for server authentication, so the key is
        // imported into the per-user key store for the lifetime of the process. PersistKeySet is deliberately
        // not set, so the temporarily imported key container is cleaned up when the certificate is disposed.
        return X509CertificateLoader.LoadPkcs12(buffer.ToArray(), PfxPassword, X509KeyStorageFlags.UserKeySet);
    }
}
