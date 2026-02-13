using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Objects;

/// <summary>
/// Control responsible for drawing an image to the screen at a specified resolution.
/// </summary>
/// <param name="name"></param>
public class Image(string name) : SceneObject(name) {
    public override bool UseMouse => false;
    public override bool UseKeyboard => false;

    /// <summary>
    /// The name of the image to be rendered.
    /// </summary>
    public string ImageName { get; set; } = string.Empty;
    
    public override void Render(RenderContext ctx) {
        if (string.IsNullOrEmpty(ImageName)) {
            return;
        }
        ctx.UpdateTransform();
        AllegroBitmap bitmap = ImageManager.GetImage(ImageName, (int)Size.X, (int)Size.Y);
        Al.DrawBitmap(bitmap, 0, 0, FlipFlags.None);
        base.Render(ctx);
    }
}