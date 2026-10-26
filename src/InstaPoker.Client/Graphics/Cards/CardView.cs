using System.Numerics;
using ImGuiNET;
using InstaPoker.Client.Graphics.Objects;
using InstaPoker.Client.Subsystems;
using InstaPoker.Core.Games;
using SubC.AllegroDotNet.Enums;

namespace InstaPoker.Client.Graphics.Cards;

/// <summary>
/// A card in the scene graph. Position, size and input are handled like any other
/// <see cref="SceneObject"/>; the visual state (face, flip and hover) lives here, while drawing
/// is delegated to <see cref="CardRenderer"/>.
/// </summary>
public sealed class CardView : SceneObject {

    [Service] private CardRenderer renderer = null!;

    public CardView(string name) : base(name) {
        // Cards are usually positioned by their center, like a hand fan.
        HorizontalAlign = InstaPoker.Client.Graphics.Styles.HorizontalAlign.Center;
        VerticalAlign = InstaPoker.Client.Graphics.Styles.VerticalAlign.Center;
        Size = new Vector2(100, 140);
    }

    public override bool UseMouse => true;
    public override bool UseKeyboard => false;

    public GameCard Value { get; set; }

    public CardStyle Style { get; set; } = CardStyle.Default;

    /// <summary>Whether the back face is the current face.</summary>
    public bool IsFaceDown { get; private set; }

    public bool CanUserFlip { get; set; }

    public bool ScaleOnHover { get; set; }

    public bool IsHovering { get; private set; }

    /// <summary>The face that was visible when the current flip started.</summary>
    public bool FlipStartFaceDown { get; private set; }

    public bool IsFlipping { get; private set; }

    /// <summary>0 at the start of a flip, 1 at the end. Always 1 when not flipping.</summary>
    public float FlipProgress => !IsFlipping || Style.FlipDuration <= 0
        ? 1f
        : Math.Clamp((float)(flipElapsed / Style.FlipDuration), 0f, 1f);

    public float HoverScale { get; private set; } = 1f;

    private double flipElapsed;
    private double hoverElapsed;

    /// <summary>Toggles the face and starts the flip animation.</summary>
    public void Flip() {
        if (IsFlipping) {
            return;
        }

        FlipStartFaceDown = IsFaceDown;
        IsFaceDown = !IsFaceDown;
        flipElapsed = 0;
        IsFlipping = Style.FlipDuration > 0;
    }

    /// <summary>Sets the current face without playing the flip animation.</summary>
    public void SetFaceDown(bool faceDown) {
        IsFaceDown = faceDown;
        FlipStartFaceDown = faceDown;
        IsFlipping = false;
        flipElapsed = 0;
    }

    public override void Update(double delta) {
        if (IsFlipping) {
            flipElapsed += delta;
            if (flipElapsed >= Style.FlipDuration) {
                flipElapsed = Style.FlipDuration;
                IsFlipping = false;
            }
        }

        if (Style.HoverDuration <= 0) {
            hoverElapsed = ScaleOnHover && IsHovering ? Style.HoverDuration : 0;
        }
        else if (ScaleOnHover && IsHovering) {
            hoverElapsed = Math.Min(Style.HoverDuration, hoverElapsed + delta);
        }
        else {
            hoverElapsed = Math.Max(0, hoverElapsed - delta);
        }

        float progress = Style.HoverDuration <= 0
            ? (ScaleOnHover && IsHovering ? 1f : 0f)
            : Math.Clamp((float)(hoverElapsed / Style.HoverDuration), 0f, 1f);
        // smoothstep keeps the hover pop from feeling abrupt
        progress = progress * progress * (3f - 2f * progress);
        HoverScale = 1f + (Style.HoverScale - 1f) * progress;

        base.Update(delta);
    }

    public override void OnMouseMove(Vector2 pos, Vector2 delta) {
        IsHovering = pos.X >= 0 && pos.X <= Size.X && pos.Y >= 0 && pos.Y <= Size.Y;
    }

    public override void OnMouseDown(MouseButton button) {
        if (button == MouseButton.Left && IsHovering && CanUserFlip) {
            Flip();
        }
    }

    public override void Render(RenderContext ctx) {
        if (renderer is null) {
            throw new InvalidOperationException(
                "CardView was not initialized with services. Add it to the scene graph through InitializeWithServices/AddChild.");
        }

        renderer.Render(ctx, this);
    }

    public override void OnImGuiDraw() {
        ImGui.Text($"Card: {Value.Value} of {Value.Suit}");
        ImGui.Text($"Face down: {IsFaceDown} | Flipping: {IsFlipping} ({FlipProgress:F2})");
        ImGui.Text($"Hover: {IsHovering} | Scale: {HoverScale:F3}");
    }
}
