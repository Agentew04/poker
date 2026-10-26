using System.Numerics;
using InstaPoker.Client.Graphics.Objects;
using InstaPoker.Core.Games;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Cards;

/// <summary>
/// Draws a card face (front or back) into the current target bitmap. It never changes the target;
/// <see cref="CardSpriteCache"/> owns that.
/// </summary>
public sealed class CardFacePainter(FontManager fonts, ImageManager images) {

    private readonly MatrixStack stack = new();
    private AllegroTransform transform = new();

    public void PaintFront(GameCard card, Vector2 size, CardStyle style) {
        ResetTransform();
        Al.DrawFilledRectangle(0, 0, size.X, size.Y, style.FrontsideBackground);

        float margin = size.X * 0.191780821917808f;
        AllegroColor accent = card.Suit is Suit.Clubs or Suit.Spades
            ? style.BlackSuitColor
            : style.RedSuitColor;

        int fontSize = Math.Max(1, (int)(margin * 0.8f));
        AllegroFont font = fonts.GetFont(FontAlias(style), fontSize, useSizeAsHeight: true);
        string cornerText = CornerText(card.Value);
        Vector2 cornerOrigin = new(margin * 0.2f, margin * 0.2f);

        string suitAlias = SuitAlias(style, card.Suit);
        AllegroBitmap suitBitmap = images.GetImage(suitAlias);

        int textWidth = Al.GetTextWidth(font, cornerText);
        float lineHeight = Al.GetFontLineHeight(font);
        float cornerSuitSize = lineHeight * 0.8f;
        Vector2 cornerBlock = new(
            MathF.Max(textWidth, cornerSuitSize),
            lineHeight * 0.95f + cornerSuitSize);

        // Top-left corner: value above, suit below.
        stack.Push();
        stack.Multiply(Matrix4x4.CreateTranslation(cornerOrigin.X, cornerOrigin.Y, 0));
        UseTransform();
        DrawCornerContent(font, accent, suitBitmap, cornerText);
        stack.Pop();

        // Bottom-right corner: the same block rotated by 180° around its own center.
        Vector2 rightTop = new(
            size.X - cornerOrigin.X - cornerBlock.X,
            size.Y - cornerOrigin.Y - cornerBlock.Y);
        stack.Push();
        stack.Multiply(Matrix4x4.CreateRotationZ(MathF.PI));
        stack.Multiply(Matrix4x4.CreateTranslation(
            rightTop.X + cornerBlock.X,
            rightTop.Y + cornerBlock.Y, 0));
        UseTransform();
        DrawCornerContent(font, accent, suitBitmap, cornerText);
        stack.Pop();

        Vector2 innerSize = new(size.X - 2 * margin, size.Y - 2 * margin);
        stack.Push();
        stack.Multiply(Matrix4x4.CreateTranslation(margin, margin, 0));
        UseTransform();

        if (card.Value is >= 2 and <= 10) {
            DrawPips(card, innerSize, suitAlias);
        }
        else {
            DrawFaceValue(card, innerSize, accent, suitAlias, style);
        }

        stack.Pop();
        ResetTransform();

        float border = style.BorderSize > 0 ? style.BorderSize : 1f;
        Al.DrawRectangle(1, 1, size.X - 1, size.Y - 1, style.BorderColor, border);
    }

    public void PaintBack(Vector2 size, CardStyle style) {
        ResetTransform();
        Al.DrawFilledRectangle(0, 0, size.X, size.Y, style.BacksideBackground);

        Vector2 diagonalEndPoint = size with { X = -size.X };
        const float angle = MathF.PI / 4;
        Vector2 direction = new(-MathF.Cos(-angle), -MathF.Sin(-angle));
        Vector2 projectedPoint = diagonalEndPoint.ProjectOnto(direction);
        float distance = projectedPoint.Length();

        int bandCount = 2 * style.StripCount + 1;
        float stripThickness = distance / bandCount;
        float halfThick = stripThickness * 0.5f;
        Vector2 perpendicular = new(-direction.Y, direction.X);

        for (int i = 0; i < bandCount; i++) {
            if (i % 2 != 1) {
                continue;
            }

            Vector2 stripCenterPoint = direction * (i * stripThickness) + direction * halfThick;
            Vector2 rightSideIntersection = Extensions.LineLineIntersection(
                stripCenterPoint, stripCenterPoint + perpendicular,
                new Vector2(halfThick, 0), new Vector2(halfThick, 1));
            Vector2 bottomSideIntersection = Extensions.LineLineIntersection(
                stripCenterPoint, stripCenterPoint + perpendicular,
                new Vector2(0, -halfThick), new Vector2(-size.X, -halfThick));
            Vector2 topSideIntersection = Extensions.LineLineIntersection(
                stripCenterPoint, stripCenterPoint + perpendicular,
                new Vector2(0, size.Y + halfThick), new Vector2(-size.X, size.Y + halfThick));
            Vector2 leftSideIntersection = Extensions.LineLineIntersection(
                stripCenterPoint, stripCenterPoint + perpendicular,
                new Vector2(-size.X - halfThick, 0), new Vector2(-size.X - halfThick, size.Y));

            Vector2 left = Vector2.Distance(stripCenterPoint, leftSideIntersection) <
                            Vector2.Distance(stripCenterPoint, bottomSideIntersection)
                ? leftSideIntersection
                : bottomSideIntersection;
            Vector2 right = Vector2.Distance(stripCenterPoint, rightSideIntersection) <
                             Vector2.Distance(stripCenterPoint, topSideIntersection)
                ? rightSideIntersection
                : topSideIntersection;

            Al.DrawLine(
                size.X + left.X, size.Y - left.Y,
                size.X + right.X, size.Y - right.Y,
                style.BacksideForeground, stripThickness);
        }

        float border = style.BorderSize > 0 ? style.BorderSize : 1f;
        Al.DrawRectangle(1, 1, size.X - 1, size.Y - 1, style.BorderColor, border);
    }

    private void DrawCornerContent(AllegroFont font, AllegroColor accent, AllegroBitmap suitBitmap,
        string text) {
        Al.DrawText(font, accent, 0, 0, FontAlignFlags.Left, text);

        int textWidth = Al.GetTextWidth(font, text);
        float lineHeight = Al.GetFontLineHeight(font);
        float bitmapSide = lineHeight * 0.8f;
        float suitX = (textWidth - bitmapSide) * 0.5f;

        Al.DrawScaledBitmap(suitBitmap,
            0, 0, Al.GetBitmapWidth(suitBitmap), Al.GetBitmapHeight(suitBitmap),
            suitX, lineHeight * 0.95f, bitmapSide, bitmapSide,
            FlipFlags.None);
    }

    private void DrawPips(GameCard card, Vector2 innerSize, string suitAlias) {
        float iconSize = Math.Min(innerSize.X * 0.2f, innerSize.Y * 0.125f);
        AllegroBitmap suitIcon = images.GetImage(suitAlias, (int)iconSize, (int)iconSize);

        foreach (int icon in CardIcons[card.Value]) {
            Vector2 pos = IconPositions[icon - 1] * innerSize;
            Al.DrawBitmap(suitIcon,
                pos.X - iconSize * 0.5f,
                pos.Y - iconSize * 0.5f,
                icon >= 10 ? FlipFlags.Vertical : FlipFlags.None);
        }
    }

    private void DrawFaceValue(GameCard card, Vector2 innerSize, AllegroColor accent,
        string suitAlias, CardStyle style) {
        string text = card.Value switch {
            0 => "Joker",
            1 => "A",
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => string.Empty
        };
        if (text.Length == 0) {
            return;
        }

        int fontSize = Math.Max(1, (int)(innerSize.Y * 0.42f));
        AllegroFont font = fonts.GetFont(FontAlias(style), fontSize, useSizeAsHeight: true);
        float textWidth = Al.GetTextWidth(font, text);
        float textHeight = Al.GetFontLineHeight(font);
        Al.DrawText(font, accent,
            (innerSize.X - textWidth) * 0.5f,
            (innerSize.Y - textHeight) * 0.5f - innerSize.Y * 0.06f,
            FontAlignFlags.Left, text);

        float iconSize = Math.Min(innerSize.X, innerSize.Y) * 0.22f;
        AllegroBitmap suit = images.GetImage(suitAlias, (int)iconSize, (int)iconSize);
        Al.DrawBitmap(suit,
            (innerSize.X - iconSize) * 0.5f,
            innerSize.Y * 0.7f - iconSize * 0.5f,
            FlipFlags.None);
    }

    private void ResetTransform() {
        while (stack.Count > 1) {
            stack.Pop();
        }
        UseTransform();
    }

    private void UseTransform() {
        stack.AsTransform(ref transform);
        Al.UseTransform(ref transform);
    }

    private static string CornerText(int value) {
        return value switch {
            0 => string.Empty,
            1 => "A",
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => value.ToString()
        };
    }

    private static string SuitAlias(CardStyle style, Suit suit) {
        string prefix = string.IsNullOrWhiteSpace(style.SuitIconPrefix) ? "suit-" : style.SuitIconPrefix;
        string name = suit switch {
            Suit.Diamonds => "diamonds",
            Suit.Clubs => "clubs",
            Suit.Hearts => "hearts",
            Suit.Spades => "spades",
            _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null)
        };
        return prefix + name;
    }

    private static string FontAlias(CardStyle style) {
        return string.IsNullOrWhiteSpace(style.FontAlias) ? "ShareTech-Regular" : style.FontAlias;
    }

    private static readonly Dictionary<int, List<int>> CardIcons = new() {
        { 2, [2, 14] },
        { 3, [2, 8, 14] },
        { 4, [1, 3, 13, 15] },
        { 5, [1, 3, 8, 13, 15] },
        { 6, [1, 3, 7, 9, 13, 15] },
        { 7, [1, 3, 5, 7, 9, 13, 15] },
        { 8, [1, 3, 5, 7, 9, 11, 13, 15] },
        { 9, [1, 3, 4, 6, 8, 10, 12, 13, 15] },
        { 10, [1, 3, 4, 5, 6, 10, 11, 12, 13, 15] }
    };

    private static readonly Vector2[] IconPositions = [
        new(0.2f, 0.125f),
        new(0.5f, 0.125f),
        new(0.8f, 0.125f),
        new(0.2f, 0.3125f),
        new(0.5f, 0.3125f),
        new(0.8f, 0.3125f),
        new(0.2f, 0.5f),
        new(0.5f, 0.5f),
        new(0.8f, 0.5f),
        new(0.2f, 0.6875f),
        new(0.5f, 0.6875f),
        new(0.8f, 0.6875f),
        new(0.2f, 0.875f),
        new(0.5f, 0.875f),
        new(0.8f, 0.875f)
    ];
}
