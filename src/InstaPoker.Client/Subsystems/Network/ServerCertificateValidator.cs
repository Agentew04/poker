using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace InstaPoker.Client.Subsystems.Network;

/// <summary>
/// A validation callback that remembers why the certificate was rejected, so the handshake code can tell
/// a key mismatch (the client is outdated) apart from other failures.
/// </summary>
public sealed class ServerCertificateValidator {

    public ServerCertificateFailure Failure { get; private set; }

    public bool Validate(object? sender, X509Certificate? certificate, X509Chain? chain,
        SslPolicyErrors errors) {
        Failure = ServerCertificatePinning.Check(certificate, errors);
        return Failure is ServerCertificateFailure.None;
    }
}
