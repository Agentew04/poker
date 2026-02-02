using System.Numerics;
using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public class Checkbox(string name) : SceneObject(name) {
    
    public override bool UseMouse => true;
    public override bool UseKeyboard => false;
    
    public CheckboxStyle Style { get; set; }

    public bool Value { get; set; }

    private bool isHovering;
    private bool isPressed;

    public event Action<bool>? OnValueChanged;

    public override void Initialize() {
        checkVertices = new float[6];
    }

    public override void Render(RenderContext ctx) {
        ctx.UpdateTransform();
        
        // draw background
        AllegroColor background;
        if (Value) {
            background = isPressed ? Style.BackgroundCheckPressed :
                isHovering ? Style.BackgroundCheckHover :
                Style.BackgroundCheck;
        }
        else {
            background = isPressed ? Style.BackgroundPressed :
                isHovering ? Style.BackgroundHover :
                Style.Background;
        }
        Al.DrawFilledRectangle(0,0, Size.X, Size.Y, background);

        // draw border
        Al.DrawRectangle(0,0, Size.X, Size.Y, Style.BorderColor, Style.BorderSize);

        // draw check
        if (Value) {
            if (checkVertices is null) {
                throw new Exception("checkVertices not initialized in Checkbox.Render()");
            }
            
            if (cachedSize == -1 || cachedSize != (int)((Size.X + Size.Y) * 0.5f)) {
                Vector2 p0 = new(0.13129942f, 0.46729942f);
                Vector2 p1 = new(0.4f,0.736f);
                Vector2 p2 = new(0.86669048f,0.26930952f);
                float size = (Size.X + Size.Y) * 0.5f;
                checkVertices[0] = p0.X * size;
                checkVertices[1] = p0.Y * size;
                checkVertices[2] = p1.X * size;
                checkVertices[3] = p1.Y * size;
                checkVertices[4] = p2.X * size;
                checkVertices[5] = p2.Y * size;
                cachedSize = (int)size;
            }
            
            const float thickRatio = 0.112244897959184f;
            float thickness = thickRatio * ((Size.X + Size.Y) * 0.5f);

            
            Al.DrawPolyline(checkVertices, 2*sizeof(float), 3,LineJoin.Round, LineCap.Round, Style.Foreground, thickness, 8);
        }
    }

    private float[]? checkVertices;
    private int cachedSize = -1;

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
            AllegroSample bopSample = AudioManager.GetAudio("bop");
            AllegroSampleID? nil = null;
            Al.PlaySample(bopSample, 1, 0, 1, PlayMode.Once, ref nil);
            
            Value = !Value;
            OnValueChanged?.Invoke(Value);
        }
        isPressed = false;
    }
}