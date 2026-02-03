using System.Numerics;
using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Objects;

public class Label : SceneObject {
    
    public override bool UseMouse => false;
    public override bool UseKeyboard => false;

    /// <summary>
    /// The text to be displayed in the label.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The size of the text to be displayed.
    /// </summary>
    public int FontSize { get; set; } = 16;

    /// <summary>
    /// The color of the text. Default value is black.
    /// </summary>
    public AllegroColor Foreground { get; set; } = Colors.Black;

    public Label(string name) : base(name) {
        HorizontalAlign = HorizontalAlign.Center;
        VerticalAlign = VerticalAlign.Center;
    }

    public override void PositionElements() {
        Vector2 size = Size;
        AllegroFont font = FontManager.GetFont("ShareTech-Regular", FontSize);
        size.X = Text.GetUtfStringWidth(font);
        size.Y = Al.GetFontAscent(font);
        Size = size;
        base.PositionElements();
    }

    public override void Render(RenderContext ctx) {
        ctx.UpdateTransform();
        AllegroFont font = FontManager.GetFont("ShareTech-Regular", FontSize);
        Vector3 translation = Transform.Translation;
        float xDiff = MathF.Floor(translation.X) - translation.X;
        float yDiff = MathF.Floor(translation.Y) - translation.Y;
        Text.DrawUtfString(font, xDiff, yDiff, Foreground);
    }
}