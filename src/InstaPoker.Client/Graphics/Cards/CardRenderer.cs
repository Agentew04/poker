using System.Numerics;
using InstaPoker.Client.Graphics.Objects;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Cards;

/// <summary>
/// Draws a <see cref="CardView"/> using cached sprites. It only applies the flip/hover transforms;
/// the scene graph already positioned the card, so drawing happens at the local origin.
/// </summary>
public sealed class CardRenderer(ICardSpriteCache sprites) {

    public void Render(RenderContext ctx, CardView card) {
        if (card.Size.X <= 0 || card.Size.Y <= 0) {
            throw new ArgumentException("Card size must be positive before rendering", nameof(card));
        }

        bool faceDown = card.IsFaceDown;
        float flipScaleX = 1f;

        if (card.IsFlipping) {
            float progress = card.FlipProgress;
            if (progress < 0.5f) {
                faceDown = card.FlipStartFaceDown;
                flipScaleX = 1f - progress * 2f;
            }
            else {
                faceDown = card.IsFaceDown;
                flipScaleX = progress * 2f - 1f;
            }
        }

        // At the exact edge of the flip the card has zero width; there is nothing visible.
        if (flipScaleX <= 0.001f) {
            return;
        }

        AllegroBitmap sprite = faceDown
            ? sprites.GetBack(card.Size, card.Style)
            : sprites.GetFront(card.Value, card.Size, card.Style);

        float hoverScale = card.ScaleOnHover ? card.HoverScale : 1f;
        Vector2 center = card.Size * 0.5f;
        bool needsTransform = MathF.Abs(flipScaleX - 1f) > 0.0001f ||
                             MathF.Abs(hoverScale - 1f) > 0.0001f;

        if (needsTransform) {
            ctx.Stack.Push();
            ctx.Stack.Multiply(Matrix4x4.CreateTranslation(center.X, center.Y, 0));
            ctx.Stack.Multiply(Matrix4x4.CreateScale(flipScaleX * hoverScale, hoverScale, 1));
            ctx.Stack.Multiply(Matrix4x4.CreateTranslation(-center.X, -center.Y, 0));
        }

        ctx.UpdateTransform();
        Al.DrawScaledBitmap(sprite,
            0, 0, Al.GetBitmapWidth(sprite), Al.GetBitmapHeight(sprite),
            0, 0, card.Size.X, card.Size.Y,
            FlipFlags.None);

        if (needsTransform) {
            ctx.Stack.Pop();
            ctx.UpdateTransform();
        }
    }
}
