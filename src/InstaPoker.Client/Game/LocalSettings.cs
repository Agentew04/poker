namespace InstaPoker.Client.Game;

/// <summary>
/// Static class with some client-side settings that are used application-wide.
/// </summary>
public static class LocalSettings {
    
    /// <summary>
    /// The username of the current player.
    /// </summary>
    public static string Username { get; set; } = string.Empty;
}