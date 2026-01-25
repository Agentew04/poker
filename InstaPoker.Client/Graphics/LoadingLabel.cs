using System.Numerics;
using System.Text;
using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public class LoadingLabel : IRenderObject {

    public string Text { get; set; } = string.Empty;
    public int FontSize { get; set; } = 26;
    public AllegroColor Foreground { get; set; } = AllegroColor.Black;

    private double lifetime = 0;
    private const double DotDelay = 1;
    private int dots = 0;
    private StringBuilder sb = new();

    public bool IsEnabled { get; private set; }

    public bool ShowDots { get; set; } = true;

    public void Initialize() {
    }

    public void Render(RenderContext ctx) {
        if (!IsEnabled) {
            return;
        }
        
        ctx.Stack.Push();
        ctx.Stack.Multiply(Matrix4x4.CreateTranslation(Position.X, Position.Y, 0));
        Translation = ctx.Stack.Peek();
        ctx.UpdateTransform();

        AllegroFont font = FontManager.GetFont("ShareTech-Regular", FontSize);

        if (ShowDots) {
            sb.Clear();
            sb.Append(Text);
            sb.Append(new string('.', dots));
        }
        string dottedText = ShowDots ? sb.ToString() : Text;

        // measure from text, append dots after
        int w = Al.GetTextWidth(font, Text);

        Al.DrawText(font, Foreground, (int)(Size.X*0.5f - w*0.5f), (int)(Size.Y*0.5f - Al.GetFontLineHeight(font)),
            FontAlignFlags.Left, dottedText);
    }

    public void Update(double delta) {
        if (!IsEnabled) {
            return;
        }
        lifetime += delta;
        if (!(lifetime >= DotDelay)) {
            return;
        }
        lifetime -= DotDelay;
        dots++;
    }

    public void Show() {
        IsEnabled = true;
        lifetime = 0;
        dots = 0;
    }

    public void Hide() {
        IsEnabled = false;
    }


    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Matrix4x4 Translation { get; set; }
}