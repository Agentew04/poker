using System.Drawing;
using System.Numerics;
using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public class Button : IRenderObject, IMouseInteractable {

    public string Label { get; set; } = string.Empty;

    public ButtonStyle Style { get; set; }
    
    public event Action? Pressed;

    private bool isHovering;
    private bool isPressed;
    
    public void Initialize() {
        
    }

    public void Render(RenderContext ctx) {
        
        ctx.Stack.Push();
        ctx.Stack.Multiply(Matrix4x4.CreateTranslation(Position.X, Position.Y, 0));
        Translation = ctx.Stack.Peek();
        ctx.UpdateTransform();

        AllegroColor effectiveBackground = Style.Background;
        if (isHovering) {
            effectiveBackground = Style.BackgroundHover;
        }

        if (isPressed) {
            effectiveBackground = Style.BackgroundPressed;
        }
        
        Al.DrawFilledRectangle(0,0, Size.X, Size.Y, effectiveBackground);
        if (Style.BorderSize > 0) {
            float borderhalf = Style.BorderSize * 0.5f;
            Al.DrawRectangle(borderhalf,borderhalf, Size.X-borderhalf, Size.Y-borderhalf, 
                Style.BorderColor, Style.BorderSize);
        }

        AllegroFont font = FontManager.GetFont("ShareTech-Regular", Style.FontSize);
        Al.SetClippingRectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        Al.DrawText(font, Style.Foreground, (int)(Size.X*0.5f),(int)(Size.Y*0.5f - Al.GetFontLineHeight(font)*0.5f),
            FontAlignFlags.Center, Label);
        Al.ResetClippingRectangle();
        ctx.Stack.Pop();
    }

    public void Update(double delta) {
        // empty
    }

    public void OnMouseMove(Vector2 pos, Vector2 delta) {
        pos = pos - new Vector2(Translation.Translation.X, Translation.Translation.Y);
        isHovering = pos.X >= 0 
                     && pos.X <= Size.X
                     && pos.Y >= 0 
                     && pos.Y <= Size.Y;
        if (isPressed && !isHovering) {
            isPressed = false;
        }
    }

    public void OnMouseDown(uint button) {
        if (button != 1) {
            return;
        }
        if (isHovering) {
            isPressed = true;
        }
    }

    public void OnMouseUp(uint button) {
        if (button != 1) {
            return;
        }

        if (isPressed && isHovering) {
            AllegroSample bopSample = AudioManager.GetAudio("bap");
            AllegroSampleID? nil = null;
            Al.PlaySample(bopSample, 1, 0, 1, PlayMode.Once, ref nil);
            Pressed?.Invoke();
        }
        isPressed = false;
    }

    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Matrix4x4 Translation { get; set; }
}