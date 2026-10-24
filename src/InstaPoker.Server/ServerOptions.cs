namespace InstaPoker.Server;

/// <summary>
/// Configuration for the game server. Bound from the "Server" configuration section, so it can be set with
/// appsettings.json, environment variables (Server__Port) or command line (--Server:Port=...).
/// </summary>
public sealed class ServerOptions {

    public const string SectionName = "Server";

    public int Port { get; set; } = 12512;
}
