using System.Numerics;
using ImGuiNET;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Client.Subsystems;
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

    [Service] private AudioManager audioManager = null!;
    [Service] private FontManager fontManager = null!;
    [Service] private MouseManager mouseManager = null!;
    
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
        AllegroFont font = fontManager.GetFont("ShareTech-Regular", Style.FontSize);
        Al.DrawText(font, Style.Foreground, (int)(Size.X*0.5f),(int)(Size.Y*0.5f - Al.GetFontLineHeight(font)*0.5f),
            FontAlignFlags.Center, Text);
        
        // draw border
        if (Style.BorderSize > 0) {
            float borderhalf = Style.BorderSize * 0.5f;
            Al.DrawRectangle(borderhalf,borderhalf, Size.X-borderhalf, Size.Y-borderhalf, 
                Style.BorderColor, Style.BorderSize);
        }
    }

    public override void Update(double delta) {
        if (isHovering) {
            mouseManager.RequestMouse(SystemMouseCursor.Link);
        }
        
        base.Update(delta);
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
            AllegroSample bopSample = audioManager.GetAudio("bap");
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
        AllegroSample bopSample = audioManager.GetAudio("bap");
        AllegroSampleID? nil = null;
        Al.PlaySample(bopSample, 1, 0, 1, PlayMode.Once, ref nil);
        Pressed?.Invoke();
    }

    public override void OnImGuiDraw() {
        ImGui.Text($"Label: \"{Text}\"");
        if (ImGui.Button("Press")) {
            Press();
        }
        base.OnImGuiDraw();
    }
}