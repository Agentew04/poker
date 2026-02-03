using System.Numerics;
using InstaPoker.Client.Game;
using InstaPoker.Core.Games;
using SubC.AllegroDotNet;

namespace InstaPoker.Client.Graphics;

/// <summary>
/// Represents a collection of different states that controls the default behavior for cards on screen,
/// such as animations and clicks.
/// </summary>
public class CardElement {
    
    /// <summary>
    /// The value of the card that is being rendered.
    /// </summary>
    public GameCard Value { get; set; }
    
    /// <summary>
    /// The size of the card.
    /// </summary>
    public Vector2 Size { get; set; }
    
    /// <summary>
    /// The <c>centered</c> position of the card. 
    /// </summary>
    public Vector2 Position { get; set; }
    
    /// <summary>
    /// Whether the card is being hovered by the user.
    /// </summary>
    public bool IsHovering { get; private set; }
    
    /// <summary>
    /// Whether the card slightly increased in size when the user hovers it.
    /// </summary>
    public bool ScaleOnHover { get; set; }
    
    /// <summary>
    /// Defines if the user can flip the card when it clicks above it.
    /// </summary>
    public bool CanUserFlip { get; set; }
    
    /// <summary>
    /// If the value of the card is currently hidder, and facing down.
    /// </summary>
    public bool IsFacingDown { get; set; }

    private double flipAnimationStart;
    private const double FlipAnimationDuration = 0.5f;
    public double FlipAnimationProgress => (Al.GetTime() - flipAnimationStart) / FlipAnimationDuration;
    
    private double scaleAnimationStart;
    private const double ScaleAnimationDuration = 0.25f;
    public double ScaleAnimationProgress => (Al.GetTime() - scaleAnimationStart) / ScaleAnimationDuration;
    

    public void Flip() {
        flipAnimationStart = Al.GetTime();
    }

    public void MouseMove(Vector2 pos) {
        bool wasHovering = IsHovering;
        IsHovering = pos.X >= Position.X - Size.X * 0.5f
                     && pos.X <= Position.X + Size.X * 0.5f
                     && pos.Y >= Position.Y - Size.Y * 0.5f
                     && pos.Y <= Position.Y + Size.Y * 0.5f;
        if (!wasHovering && IsHovering) {
            scaleAnimationStart = Al.GetTime();
        }
    }

    public void MouseDown() {
        if (IsHovering && CanUserFlip) {
            Flip();
        }
    }
}