using System.Numerics;
using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Objects;

public class Button(string name) : SceneObject(name) {
    public override bool UseMouse => true;
    public override bool UseKeyboard => false;
    
    public string Text { get; set; } = string.Empty;

    public ButtonStyle Style { get; set; }
    
    public event Action? Pressed;

    private bool isHovering;
    private bool isPressed;

    public override void Initialize() {
        Clip = true;
        base.Initialize();
    }

    public override void Render(RenderContext ctx) {
        AllegroColor effectiveBackground = Style.Background;
        if (isHovering) {
            effectiveBackground = Style.BackgroundHover;
        }

        if (isPressed) {
            effectiveBackground = Style.BackgroundPressed;
        }
        
        ctx.UpdateTransform();
        
        // draw background
        Al.DrawFilledRectangle(0,0, Size.X, Size.Y, effectiveBackground);

        // draw label
        AllegroFont font = FontManager.GetFont("ShareTech-Regular", Style.FontSize);
        Al.DrawText(font, Style.Foreground, (int)(Size.X*0.5f),(int)(Size.Y*0.5f - Al.GetFontLineHeight(font)*0.5f),
            FontAlignFlags.Center, Text);
        
        // draw border
        if (Style.BorderSize > 0) {
            float borderhalf = Style.BorderSize * 0.5f;
            Al.DrawRectangle(borderhalf,borderhalf, Size.X-borderhalf, Size.Y-borderhalf, 
                Style.BorderColor, Style.BorderSize);
        }
    }

    public override void OnMouseMove(Vector2 pos, Vector2 delta) {
        isHovering = pos.X >= 0 
                     && pos.X <= Size.X
                     && pos.Y >= 0 
                     && pos.Y <= Size.Y;
        if (isPressed && !isHovering) {
            isPressed = false;
        }
    }

    public override void OnMouseDown(MouseButton button) {
        if (button != MouseButton.Left) {
            return;
        }
        if (isHovering) {
            isPressed = true;
        }
    }

    public override void OnMouseUp(MouseButton button) {
        if (button != MouseButton.Left) {
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
    
    /// <summary>
    /// Simulates a button press by code.
    /// </summary>
    public void Press() {
        AllegroSample bopSample = AudioManager.GetAudio("bap");
        AllegroSampleID? nil = null;
        Al.PlaySample(bopSample, 1, 0, 1, PlayMode.Once, ref nil);
        Pressed?.Invoke();
    }
}