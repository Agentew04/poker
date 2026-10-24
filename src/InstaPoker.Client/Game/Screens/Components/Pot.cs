using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Objects;
using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game.Screens.Components;

public class Pot : SceneObject{
    public Pot(string name) : base(name) { }
    public override bool UseMouse => false;
    public override bool UseKeyboard => false;

    private int amount = 0;
    private string amountStr = string.Empty;

    [ChildObject] private readonly Label titleLabel = new(nameof(titleLabel));
    [ChildObject] private readonly Label amountLabel = new(nameof(amountLabel));
    
    public void SetAmount(int value) {
        amount = value;
        amountStr = amount.ToString();
    }

    public override void Initialize() {
        titleLabel.Text = "Pot";
        titleLabel.FontSize = 30;
        titleLabel.VerticalAlign = VerticalAlign.Bottom;
        titleLabel.HorizontalAlign = HorizontalAlign.Center;
        
        amountLabel.Text = amountStr;
        amountLabel.FontSize = 30;
        amountLabel.VerticalAlign = VerticalAlign.Top;
        titleLabel.HorizontalAlign = HorizontalAlign.Center;
        
        base.Initialize();
    }

    public override void PositionElements() {
        const int margin = 5;
        titleLabel.Place().AtAnchor(Size.X * 0.5f, Size.Y * 0.5f - margin);
        amountLabel.Place().AtAnchor(Size.X * 0.5f, Size.Y * 0.5f + margin);
        base.PositionElements();
    }

    public override void Render(RenderContext ctx) {
        AllegroColor darkGreen = new() {
            R = 0.4117647058823529f, G = 0.8588235294117647f, B = 0.48627450980392156f, A = 1
        };
        ctx.UpdateTransform();
        Al.DrawFilledEllipse(Size.X * 0.5f, Size.Y * 0.5f, Size.X * 0.5f, Size.Y * 0.5f, darkGreen);
        Al.DrawEllipse(Size.X * 0.5f, Size.Y * 0.5f, Size.X * 0.5f, Size.Y * 0.5f, Colors.Black, 2);
        base.Render(ctx);
    }
}