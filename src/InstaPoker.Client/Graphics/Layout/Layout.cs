using System.Numerics;

namespace InstaPoker.Client.Graphics.Layout;

/// <summary>
/// Pure layout math. Every method returns the position the target should have, preserving the axis
/// that the operation does not touch.
/// </summary>
public static class Layout {

    /// <summary>Places the target to the left of the anchor, separated by a margin.</summary>
    public static Vector2 LeftOf(Bounds target, Bounds anchor, float margin = 0)
        => new(anchor.Left - margin - target.Size.X, target.Position.Y);

    /// <summary>Places the target to the right of the anchor, separated by a margin.</summary>
    public static Vector2 RightOf(Bounds target, Bounds anchor, float margin = 0)
        => new(anchor.Right + margin, target.Position.Y);

    /// <summary>Places the target above the anchor, separated by a margin.</summary>
    public static Vector2 Above(Bounds target, Bounds anchor, float margin = 0)
        => new(target.Position.X, anchor.Top - margin - target.Size.Y);

    /// <summary>Places the target below the anchor, separated by a margin.</summary>
    public static Vector2 Below(Bounds target, Bounds anchor, float margin = 0)
        => new(target.Position.X, anchor.Bottom + margin);

    /// <summary>Aligns the left edges of the target and the anchor.</summary>
    public static Vector2 AlignLeftWith(Bounds target, Bounds anchor)
        => new(anchor.Left, target.Position.Y);

    /// <summary>Aligns the right edges of the target and the anchor.</summary>
    public static Vector2 AlignRightWith(Bounds target, Bounds anchor)
        => new(anchor.Right - target.Size.X, target.Position.Y);

    /// <summary>Aligns the top edges of the target and the anchor.</summary>
    public static Vector2 AlignTopWith(Bounds target, Bounds anchor)
        => new(target.Position.X, anchor.Top);

    /// <summary>Aligns the bottom edges of the target and the anchor.</summary>
    public static Vector2 AlignBottomWith(Bounds target, Bounds anchor)
        => new(target.Position.X, anchor.Bottom - target.Size.Y);

    /// <summary>Centers the target horizontally on the anchor.</summary>
    public static Vector2 CenterHorizontallyWith(Bounds target, Bounds anchor)
        => new(anchor.Center.X - target.Size.X * 0.5f, target.Position.Y);

    /// <summary>Centers the target vertically on the anchor.</summary>
    public static Vector2 CenterVerticallyWith(Bounds target, Bounds anchor)
        => new(target.Position.X, anchor.Center.Y - target.Size.Y * 0.5f);

    /// <summary>Centers the target inside the container.</summary>
    public static Vector2 CenterIn(Bounds target, Bounds container)
        => container.Position + (container.Size - target.Size) * 0.5f;

    /// <summary>Centers the target horizontally inside the container.</summary>
    public static Vector2 CenterHorizontallyIn(Bounds target, Bounds container)
        => new(container.Position.X + (container.Size.X - target.Size.X) * 0.5f, target.Position.Y);

    /// <summary>Centers the target vertically inside the container.</summary>
    public static Vector2 CenterVerticallyIn(Bounds target, Bounds container)
        => new(target.Position.X, container.Position.Y + (container.Size.Y - target.Size.Y) * 0.5f);
}
