using System.Numerics;

namespace InstaPoker.Client.Graphics.Objects;

/// <summary>
/// A simple component that controls the alpha value of an inner object. 
/// </summary>
public class Fader(string name) : SceneObject(name) {
    
    public override bool UseMouse => Content?.UseMouse ?? false;

    public override bool UseKeyboard => Content?.UseKeyboard ?? false;

    // WARNING: NAO CONVERTER EM IMPLICIT! DOCFX NAO SUPORTA ESSA FEATURE DO C# 14/.NET 10 AINDA!!!
    private SceneObject? content;
    
    /// <summary>
    /// The inner object that will be rendered.
    /// </summary>
    public SceneObject? Content {
        get => content;
        set {
            if (GetChildren().Count > 0) {
                RemoveChild(GetChildren()[0]);
            }

            content = value;
            if (value is not null) {
                AddChild(value);
            }
        }
    }

    private double startTime;
    private double showDuration;
    private double delay;
    private bool active;
    private double lifetime;

    public override void Render(RenderContext ctx) {
        if (!active || Content == null) {
            return;
        }
        
        double t = lifetime;
        if (t < delay) {
            return;
        }

        t -= delay;
        float alpha = 1f;
        const double fadeIn = 0.2;
        const double fadeOut = 0.2;

        if (t < fadeIn) {
            alpha = (float)(t / fadeIn);
        }else if (t > showDuration - fadeOut) {
            alpha = (float)((showDuration - t) / fadeOut);
        }

        alpha = Math.Clamp(alpha, 0f, 1f);

        Content.Size = Size;
        Content.Position = Vector2.Zero;

        ctx.AlphaStack.Push();
        ctx.AlphaStack.Multiply(alpha);
        base.Render(ctx);
        ctx.AlphaStack.Pop();
    }

    public override void Update(double delta) {
        if (!active) return;

        lifetime += delta;

        base.Update(delta);
        
        if (lifetime > delay + showDuration)
            active = false;
    }

    /// <summary>
    /// Shows the content for a specified amount of time.
    /// </summary>
    /// <param name="duration">How long the content appears on-screen</param>
    public void ShowFor(double duration) {
        delay = 0;
        showDuration = duration;
        lifetime = 0;
        active = true;
    }

    /// <summary>
    /// Shows the content after a delay, for a duration of time.
    /// </summary>
    /// <param name="delay">How long to wait before showing the content</param>
    /// <param name="duration">How long the content shows on the screen</param>
    public void ShowAfter(double delay, double duration) {
        this.delay = delay;
        showDuration = duration;
        lifetime = 0;
        active = true;
    }
}