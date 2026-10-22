namespace InstaPoker.Client.Subsystems.Network;

/// <summary>
/// Outcome of an attempt to connect to the remote server.
/// </summary>
public enum ConnectionResult {
    Success,

    /// <summary>The server presented a certificate with a different key. The client must be updated.</summary>
    CertificateMismatch,

    /// <summary>The connection attempt timed out.</summary>
    Timeout,

    /// <summary>The server refused the connection.</summary>
    ConnectionRefused,

    /// <summary>Any other failure (protocol mismatch, invalid server id, closed connection, ...).</summary>
    Failed
}
