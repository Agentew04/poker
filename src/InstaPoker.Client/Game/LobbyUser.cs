using InstaPoker.Client.Graphics;

namespace InstaPoker.Client.Game;

/// <summary>
/// Class representing a user inside a lobby.
/// </summary>
public class LobbyUser {
    
    /// <summary>
    /// The username of the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the user is the owner of the lobby or not. 
    /// </summary>
    public bool IsOwner { get; set; }
    
    /// <summary>
    /// Whether the user is the current player or not.
    /// </summary>
    public bool IsLocal { get; set; }
    
    /// <summary>
    /// Reference to the button that appears next to the user's name.
    /// Normally is a 'Kick' or 'Leave' button.
    /// </summary>
    public Button? Button { get; set; } = null;
}