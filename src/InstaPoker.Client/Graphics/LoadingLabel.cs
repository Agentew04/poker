using System.Numerics;
using System.Text;
using InstaPoker.Client.Graphics.Objects;
using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public class LoadingLabel(string name) : SceneObject(name) {
    
    public override bool UseMouse => false;
    public override bool UseKeyboard => false;
    
    public string Text { get; set; } = string.Empty;
    public int FontSize { get; set; } = 26;
    public AllegroColor Foreground { get; set; } = Colors.Black;

    private double lifetime = 0;
    private const double DotDelay = 1;
    private int dots = 0;
    private StringBuilder sb = new();

    public bool ShowDots { get; set; } = true;

    public override void Render(RenderContext ctx) {
        if (!IsEnabled) {
            return;
        }
        
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

    public override void Update(double delta) {
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
}