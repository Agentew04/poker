using System.Numerics;
using InstaPoker.Client.Graphics.Cards;
using InstaPoker.Client.Graphics.Objects;
using Xunit;

namespace InstaPoker.Tests;

public class CardViewTests {

    private static CardView CreateCard(float width = 100, float height = 140) {
        return new CardView("card") { Size = new Vector2(width, height) };
    }

    [Fact]
    public void Clicking_a_hovered_flippable_card_starts_the_flip() {
        CardView card = CreateCard();
        card.CanUserFlip = true;
        card.OnMouseMove(new Vector2(50, 70), Vector2.Zero);

        card.OnMouseDown(MouseButton.Left);

        Assert.True(card.IsFaceDown);
        Assert.True(card.IsFlipping);
        Assert.False(card.FlipStartFaceDown);
        Assert.Equal(0f, card.FlipProgress, 3);
    }

    [Fact]
    public void Flip_progress_reaches_one_and_stops_flipping() {
        CardView card = CreateCard();
        card.Flip();

        card.Update(0.25);
        Assert.InRange(card.FlipProgress, 0.49f, 0.51f);
        Assert.True(card.IsFlipping);

        card.Update(0.25);
        Assert.Equal(1f, card.FlipProgress, 3);
        Assert.False(card.IsFlipping);
    }

    [Fact]
    public void A_zero_duration_flip_applies_immediately() {
        CardView card = CreateCard();
        card.Style = CardStyle.Default with { FlipDuration = 0 };

        card.Flip();

        Assert.True(card.IsFaceDown);
        Assert.False(card.IsFlipping);
        Assert.Equal(1f, card.FlipProgress, 3);
    }

    [Fact]
    public void Clicking_does_not_flip_when_not_hovered_or_not_allowed() {
        CardView card = CreateCard();
        card.CanUserFlip = true;
        card.OnMouseMove(new Vector2(500, 500), Vector2.Zero);

        card.OnMouseDown(MouseButton.Left);
        Assert.False(card.IsFaceDown);

        card.OnMouseMove(new Vector2(10, 10), Vector2.Zero);
        card.CanUserFlip = false;
        card.OnMouseDown(MouseButton.Left);
        Assert.False(card.IsFaceDown);
    }

    [Fact]
    public void Hover_scale_ramps_up_and_back_down_smoothly() {
        CardView card = CreateCard();
        card.ScaleOnHover = true;

        card.OnMouseMove(new Vector2(10, 10), Vector2.Zero);
        Assert.True(card.IsHovering);

        card.Update(CardStyle.Default.HoverDuration / 2);
        Assert.InRange(card.HoverScale, 1.001f, CardStyle.Default.HoverScale - 0.001f);

        card.OnMouseMove(new Vector2(-1, -1), Vector2.Zero);
        Assert.False(card.IsHovering);

        card.Update(CardStyle.Default.HoverDuration);
        Assert.Equal(1f, card.HoverScale, 3);
    }

    [Fact]
    public void Bounds_use_the_card_center_as_the_anchor() {
        CardView card = CreateCard();
        card.Position = new Vector2(200, 200);

        Assert.Equal(new Vector2(150, 130), card.Bounds.Position);
        Assert.Equal(new Vector2(100, 140), card.Bounds.Size);
    }

    [Fact]
    public void Mouse_hit_test_uses_local_card_bounds() {
        CardView card = CreateCard();

        card.OnMouseMove(Vector2.Zero, Vector2.Zero);
        Assert.True(card.IsHovering);

        card.OnMouseMove(new Vector2(99, 139), Vector2.Zero);
        Assert.True(card.IsHovering);

        card.OnMouseMove(new Vector2(100.1f, 70), Vector2.Zero);
        Assert.False(card.IsHovering);
    }

    [Fact]
    public void SetFaceDown_skips_the_flip_animation() {
        CardView card = CreateCard();

        card.SetFaceDown(true);

        Assert.True(card.IsFaceDown);
        Assert.False(card.IsFlipping);
        Assert.Equal(1f, card.FlipProgress, 3);
    }
}
