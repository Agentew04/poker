using System.Numerics;

namespace InstaPoker.Client.Graphics.Layout;

/// <summary>
/// Represents a bounding rectangle.
/// </summary>
public readonly struct Bounds(Vector2 position, Vector2 size) {
    public Vector2 Position { get; } = position;
    public Vector2 Size { get; } = size;
    
    /// <summary>
    /// The coordinate at the left of the rectangle.
    /// </summary>
    public float Left => Position.X;
    
    /// <summary>
    /// The coordinate at the right of the rectangle.
    /// </summary>
    public float Right => Position.X + Size.X;
    
    /// <summary>
    /// The coordinate at the top of the rectangle.
    /// </summary>
    public float Top => Position.Y;
    
    /// <summary>
    /// Represents the coordinate at the bottom.
    /// </summary>
    public float Bottom => Position.Y + Size.Y;
    
    /// <summary>
    /// Returns a point in the center of these bounds.
    /// </summary>
    public Vector2 Center => Position + Size * 0.5f;
}