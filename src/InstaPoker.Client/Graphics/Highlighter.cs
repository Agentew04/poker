using System.Numerics;
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
    
    private static readonly List<Vector4> Highlights = [];
    private static AllegroTransform _identity = new();
    private static readonly AllegroColor HighlightColor = Colors.Red;
    

    /// <summary>
    /// Marks an object for highlighting on the next frame. 
    /// </summary>
    /// <param name="obj">The object to be highlighted</param>
    public static void HighlightObject(SceneObject obj) {
        Vector3 translation = obj.Transform.Translation;
        Highlights.Add(new Vector4(translation.X, translation.Y, translation.X + obj.Size.X, translation.Y + obj.Size.Y));
    }

    /// <summary>
    /// Draws all highlights of the previous frame.
    /// </summary>
    public static void DrawHighlights() {
        Al.IdentityTransform(ref _identity);
        Al.UseTransform(ref _identity);
        foreach (Vector4 highlight in Highlights) {
            Al.DrawRectangle(highlight.X, highlight.Y, highlight.Z, highlight.W, HighlightColor, 2);
        }
        Highlights.Clear();
    }
}