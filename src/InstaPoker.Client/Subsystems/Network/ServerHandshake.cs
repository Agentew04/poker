using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using InstaPoker.Core;
using Microsoft.Extensions.Logging;

namespace InstaPoker.Client.Subsystems.Network;

/// <summary>
/// Performs the client identification exchange and upgrades the connection to TLS.
/// </summary>
/// <remarks>
/// Runs on a thread pool thread: the identification exchange uses blocking reads, and the TLS handshake
/// must not touch the window loop.
/// </remarks>
public static class ServerHandshake {

    public static async Task<SslStream> IdentifyAndSecureAsync(TcpClient client, string hostname, string username,
        Version clientVersion, ILogger logger, CancellationToken cancellationToken = default) {
        NetworkStream ns = client.GetStream();
        using BinaryWriter bw = new(ns, Encoding.UTF8, true);
        using BinaryReader br = new(ns, Encoding.UTF8, true);

        bw.Write("InstaPoker.Client"); // send client id
        bw.Write(clientVersion);
        bw.Write(username);
        bw.Flush();

        string serverId = br.ReadString();
        if (serverId != "InstaPoker.Server") {
            throw new InvalidOperationException(
                $"Could not recognize server id. Received \"{serverId}\". Expected: \"InstaPoker.Server\"");
        }

        Version serverVersion = br.ReadVersion();
        logger.LogInformation("Server is at version {version}", serverVersion.ToString());

        // we have identified the server, start ssl handshake
        SslStream sslStream = new(ns, false);
        ServerCertificateValidator validator = new();
        try {
            await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions {
                TargetHost = hostname,
                RemoteCertificateValidationCallback = validator.Validate
            }, cancellationToken);
        }
        catch (AuthenticationException e) when (validator.Failure is ServerCertificateFailure.KeyMismatch) {
            await sslStream.DisposeAsync();
            throw new ServerCertificateMismatchException(e);
        }
        catch {
            await sslStream.DisposeAsync();
            throw;
        }

        logger.LogInformation("Protocol: {protocol}; Cypher suite: {cypher}", sslStream.SslProtocol,
            sslStream.NegotiatedCipherSuite);

        X509Certificate2? serverCertificate = (X509Certificate2?)sslStream.RemoteCertificate;
        if (serverCertificate is null) {
            await sslStream.DisposeAsync();
            throw new AuthenticationException("The server did not present a certificate.");
        }

        logger.LogInformation("Server certificate info:\n\tSubject: {sub}\n\tIssuer: {iss}\n\tValid until: {valid}\n" +
                              "\tDNS Name: {dns}",
            serverCertificate.Subject,
            serverCertificate.Issuer,
            serverCertificate.NotAfter.ToString("yyyy-MM-dd"),
            serverCertificate.GetNameInfo(X509NameType.DnsName, false)
        );

        return sslStream;
    }
}
