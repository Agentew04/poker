using System.Security.Authentication;

namespace InstaPoker.Client.Subsystems.Network;

/// <summary>
/// Raised when the server presents a certificate whose public key does not match the key pinned in this client.
/// The user has to update the client to talk to that server again.
/// </summary>
public sealed class ServerCertificateMismatchException : AuthenticationException {

    public ServerCertificateMismatchException()
        : base("The server's certificate does not match the one this client was built with. Update the client.") {
    }

    public ServerCertificateMismatchException(Exception innerException)
        : base("The server's certificate does not match the one this client was built with. Update the client.",
            innerException) {
    }
}
