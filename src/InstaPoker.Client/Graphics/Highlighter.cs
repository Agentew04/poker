using System.Numerics;
using InstaPoker.Client.Graphics.Layout;
using InstaPoker.Client.Graphics.Objects;
using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

/// <summary>
/// Static class to debug visual elements. Any elements registered in one frame will be highlighted on the
/// next frame.
/// </summary>
public static class Highlighter {
    
    private static readonly List<(SceneObject, Bounds)> Highlights = [];
    private static AllegroTransform _identity = new();
    private static readonly AllegroColor HighlightColor = Colors.Red;
    

    /// <summary>
    /// Marks an object for highlighting on the next frame. 
    /// </summary>
    /// <param name="obj">The object to be highlighted</param>
    public static void HighlightObject(SceneObject obj) {
        Vector3 translation = obj.Transform.Translation;
        Bounds bounds = new(translation.ToVec2(), obj.Size);
        Highlights.Add((obj, bounds));
    }

    /// <summary>
    /// Draws all highlights of the previous frame.
    /// </summary>
    public static void DrawHighlights() {
        Al.IdentityTransform(ref _identity);
        Al.UseTransform(ref _identity);
        foreach ((SceneObject obj, Bounds bounds) in Highlights) {
            // bbox
            Al.DrawRectangle(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom, HighlightColor, 2);
            // anchored position
            Al.DrawFilledCircle(bounds.Left + obj.PivotOffset.X, bounds.Top + obj.PivotOffset.Y, 4, HighlightColor);
            // custom gizmo drawing
            obj.OnDebugDraw(bounds);
        }
        Highlights.Clear();
    }
}