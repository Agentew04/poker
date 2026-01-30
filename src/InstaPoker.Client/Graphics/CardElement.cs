using System.Numerics;
using InstaPoker.Client.Game;

namespace InstaPoker.Client.Graphics;

/// <summary>
/// Represents a collection of different states that controls the default behavior for cards on screen,
/// such as animations and clicks.
/// </summary>
public class CardElement {
    
    /// <summary>
    /// The value of the card that is being rendered.
    /// </summary>
    public GameCard Card { get; set; }
    
    /// <summary>
    /// The size of the card.
    /// </summary>
    public Vector2 Size { get; set; }
    
    /// <summary>
    /// The <c>centered</c> position of the card. 
    /// </summary>
    public Vector2 Position { get; set; }
    
    /// <summary>
    /// Whether the card is currently tilted or not.
    /// </summary>
    public bool IsTilted { get; private set; }
    
    /// <summary>
    /// Whether the card tilts when the user hovers its mouse above it.
    /// </summary>
    public bool TiltOnHover { get; set; }
    
    /// <summary>
    /// Defines if the user can flip the card when it clicks above it.
    /// </summary>
    public bool CanUserFlip { get; set; }

    public void Flip() {

    }

    public void MouseMove(Vector2 pos) {

    }

    public void MouseDown() {
        
    }
}