using System.Numerics;

namespace InstaPoker.Client.Graphics;

public class Fader : IRenderObject {

    public IRenderObject? Content { get; set; } = null;

    private double startTime;
    private double showDuration;
    private double delay;
    private bool active;
    private double lifetime;
    
    public void Initialize() {
    }

    public void Render(RenderContext ctx) {
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
        Content.Position = Position;

        ctx.AlphaStack.Push();
        ctx.AlphaStack.Multiply(alpha);
        Content.Render(ctx);
        ctx.AlphaStack.Pop();
    }

    public void Update(double delta) {
        if (!active) return;

        lifetime += delta;

        Content?.Update(delta);

        if (lifetime > delay + showDuration)
            active = false;
    }

    public void ShowFor(double duration) {
        delay = 0;
        showDuration = duration;
        lifetime = 0;
        active = true;
    }

    public void ShowAfter(double delay, double duration) {
        this.delay = delay;
        showDuration = duration;
        lifetime = 0;
        active = true;
    }

    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
}