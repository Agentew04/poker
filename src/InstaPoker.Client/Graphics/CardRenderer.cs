using System.Numerics;
using ImGuiNET;
using InstaPoker.Client.Game;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Core.Games;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public class CardRenderer {
    public Vector2 CardSize { get; set; }

    public CardStyle Style { get; set; }

    private AllegroTransform transform = new();

    private readonly MatrixStack stack = new();

    public RenderContext RenderContext { get; set; } = null!;

    public void Render(CardElement element) {
        bool isScaling = element.ScaleAnimationProgress >= 0 && element.ScaleAnimationProgress <= 1; 
        if (isScaling) {
            Console.WriteLine("scale");
            var translate = RenderContext.Stack.Peek().Translation;
            RenderContext.Stack.Push();
            RenderContext.Stack.Multiply(Matrix4x4.CreateTranslation(-translate));
            RenderContext.Stack.Multiply(Matrix4x4.CreateScale(((float)element.ScaleAnimationProgress).Map(0,1,1,1.1f)));
            RenderContext.Stack.Multiply(Matrix4x4.CreateTranslation(translate));
        }  
        RenderAt(element.Value, element.Position, element.IsFacingDown,
            element.FlipAnimationProgress > 1 ? 0 : (float)element.FlipAnimationProgress);
        if (isScaling) {
            RenderContext.Stack.Pop();
        }  
    }
    
    /// <summary>
    /// Draws a playing card centralized at <paramref name="pos"/> with size <see cref="CardSize"/>.
    /// </summary>
    /// <param name="card">The value of the card that will be rendered</param>
    /// <param name="pos">The centralized position of the card</param>
    /// <param name="faceDown">Whether the card is facing down or up</param>
    /// <param name="t">How far in the flip animation the card is. 0 is equals to <c>!faceDown</c>
    /// and values &gt;= 1 result in <c>faceDown</c> </param>
    /// <param name="theta">Rotation of the card in radians</param>
    public void RenderAt(GameCard card, Vector2 pos, bool faceDown, float t) {
        t = Math.Clamp(t, 0, 1);
        // The flip animation is split into two phases around t = 0.5:
        // - First half (t in [0, 0.5]): scale the current face from full width down to zero.
        // - Second half (t in (0.5, 1]): flip the face, then scale the new face from zero up to full width.
        if (t > 0.5f) {
            // invert and rotate
            t = t.Map(0.5f, 1, 0, 1);
            faceDown = !faceDown;
        }
        else {
            t = t.Map(0, 0.5f, 1, 0);
        }
        Al.ResetClippingRectangle();

        AllegroBitmap? prevTarget = Al.GetTargetBitmap();
        AllegroBitmap cardBitmap = ImageManager.Borrow((int)CardSize.X, (int)CardSize.Y);
        Al.SetTargetBitmap(cardBitmap);

        if (faceDown) {
            DrawBackFace();
        }
        else {
            DrawFrontFace(card);
        }

        // draw border
        Al.DrawRectangle((int)1, (int)1, (int)CardSize.X - 1, (int)CardSize.Y - 1, Style.BorderColor, 2);

        Al.SetTargetBitmap(prevTarget);
        var before = RenderContext.Stack.Peek().Translation;
        RenderContext.Stack.Push();
        RenderContext.Stack.Multiply(Matrix4x4.CreateTranslation(-before));
        RenderContext.Stack.Multiply(Matrix4x4.CreateTranslation(new Vector3(-pos,0)));
        RenderContext.Stack.Multiply(Matrix4x4.CreateScale(t,1,1));
        RenderContext.Stack.Multiply(Matrix4x4.CreateTranslation(new Vector3(pos,0)));
        RenderContext.Stack.Multiply(Matrix4x4.CreateTranslation(before));
        RenderContext.UpdateTransform();
        Al.DrawBitmap(cardBitmap, pos.X - CardSize.X*0.5f, pos.Y-CardSize.Y*0.5f, FlipFlags.None);
        RenderContext.Stack.Pop();
    }

    private void DrawBackFace() {
        Al.DrawFilledRectangle(0, 0, CardSize.X, CardSize.Y, Style.BacksideBackground);

        Vector2 diagonalEndPoint = CardSize with { X = -CardSize.X };
        const float angle = MathF.PI / 4; // 45 degrees
        Vector2 r = new(-MathF.Cos(-angle), -MathF.Sin(-angle)); // (-1,0) rotated by 'angle' degrees
        Vector2 projectedPoint = diagonalEndPoint.ProjectOnto(r);
        float distance = projectedPoint.Length(); // distance between projectedPoint and (0,0)
        int bandCount = 2 * Style.StripCount + 1;
        float stripThickness = distance / bandCount;
        float halfThick = stripThickness * 0.5f;
        Vector2 r90 = new(-r.Y, r.X);
        for (int i = 0; i < bandCount; i++) {
            if (i % 2 != 1) {
                continue;
            }

            Vector2 stripCenterPoint = r * (i * stripThickness) + r * halfThick;
            Vector2 rightSideIntersection =
                Extensions.LineLineIntersection(stripCenterPoint, stripCenterPoint + r90, new Vector2(halfThick, 0), new Vector2(halfThick, 1));
            Vector2 bottomSideIntersection =
                Extensions.LineLineIntersection(stripCenterPoint, stripCenterPoint + r90, new Vector2(0, -halfThick),
                    new Vector2(-CardSize.X, -halfThick));
            Vector2 topSideIntersection =
                Extensions.LineLineIntersection(stripCenterPoint, stripCenterPoint + r90, new Vector2(0, CardSize.Y + halfThick),
                    new Vector2(-CardSize.X, CardSize.Y + halfThick));
            Vector2 leftSideIntersection =
                Extensions.LineLineIntersection(stripCenterPoint, stripCenterPoint + r90, new Vector2(-CardSize.X - halfThick, 0),
                    new Vector2(-CardSize.X - halfThick, CardSize.Y));

            Vector2 left = Vector2.Distance(stripCenterPoint, leftSideIntersection) < Vector2.Distance(stripCenterPoint, bottomSideIntersection)
                ? leftSideIntersection
                : bottomSideIntersection;
            Vector2 right = Vector2.Distance(stripCenterPoint, rightSideIntersection) < Vector2.Distance(stripCenterPoint, topSideIntersection)
                ? rightSideIntersection
                : topSideIntersection;

            Al.DrawLine(CardSize.X + left.X, CardSize.Y - left.Y,
                CardSize.X + right.X, CardSize.Y - right.Y,
                Style.BacksideForeground, stripThickness);
        }
    }

    private void DrawFrontFace(GameCard card) {
        stack.AsTransform(ref transform);
        Al.UseTransform(ref transform);
        Al.DrawFilledRectangle(0, 0, CardSize.X, CardSize.Y, Style.FrontsideBackground);

        const float marginRate = 0.191780821917808f;
        float margin = CardSize.X * marginRate;
        // Al.DrawRectangle(margin, margin,
        //     CardSize.X - margin, CardSize.Y - margin,
        //     Colors.Black, 2);

        AllegroColor accent = card.Suit is Suit.Clubs or Suit.Spades ? Style.BlackSuitColor : Style.RedSuitColor;

        // draw corner indicators
        AllegroFont font = FontManager.GetFont("ShareTech-Regular", (int)margin, true);
        string cornerText = card.Value switch {
            0 => string.Empty,
            1 => "A",
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => card.Value.ToString()
        };
        Vector2 leftCorner = new(margin * 0.5f, margin * 0.5f);
        Vector2 rightCorner = new(CardSize.X - margin * 0.5f, CardSize.Y - margin * 0.5f);
        Vector2 offset = new(-Al.GetTextWidth(font, cornerText) * 0.5f, -Al.GetFontLineHeight(font) * 0.5f);

        string bitmapAlias = "suit-" + card.Suit switch {
            Suit.Diamonds => "diamonds",
            Suit.Clubs => "clubs",
            Suit.Hearts => "hearts",
            Suit.Spades => "spades",
            _ => throw new Exception("Unknown Card Suit")
        };
        AllegroBitmap suitBitmap = ImageManager.GetImage(bitmapAlias);

        stack.Push();
        stack.Multiply(Matrix4x4.CreateTranslation(leftCorner.X, leftCorner.Y, 0));
        stack.AsTransform(ref transform);
        Al.UseTransform(ref transform);
        // draw text
        Al.DrawText(font, accent, (int)offset.X, (int)offset.Y, FontAlignFlags.Left, cornerText);
        // draw suit
        float bitmapSide = Al.GetFontAscent(font);
        Al.DrawScaledBitmap(suitBitmap,
            0, 0, Al.GetBitmapWidth(suitBitmap), Al.GetBitmapWidth(suitBitmap),
            -bitmapSide * 0.5f, bitmapSide * 0.5f, bitmapSide, bitmapSide,
            FlipFlags.None
        );
        stack.Pop();

        Vector3 transBefore = stack.Peek().Translation;
        stack.Push();
        stack.Multiply(Matrix4x4.CreateTranslation(-transBefore));
        stack.Multiply(Matrix4x4.CreateRotationZ(MathF.PI));
        stack.Multiply(Matrix4x4.CreateTranslation(transBefore));
        stack.Multiply(Matrix4x4.CreateTranslation(rightCorner.X, rightCorner.Y, 0));
        stack.AsTransform(ref transform);
        Al.UseTransform(ref transform);
        Al.DrawText(font, accent, (int)offset.X, (int)offset.Y, FontAlignFlags.Left, cornerText);
        Al.DrawScaledBitmap(suitBitmap,
            0, 0, Al.GetBitmapWidth(suitBitmap), Al.GetBitmapWidth(suitBitmap),
            -bitmapSide * 0.5f, bitmapSide * 0.5f, bitmapSide, bitmapSide,
            FlipFlags.None
        );
        stack.Pop();
        
        // render icons
        Vector2 innerSize = new(CardSize.X - 2 * margin, CardSize.Y - 2 * margin);
        stack.Push();
        stack.Multiply(Matrix4x4.CreateTranslation(new Vector3(margin, margin,0)));
        stack.AsTransform(ref transform);
        Al.UseTransform(ref transform);
        if (card.Value is >= 2 and <= 10) { // only draw icons for 2 until 10
            float iconSize = Math.Min(innerSize.X*0.2f, innerSize.Y*0.125f);
            AllegroBitmap suitIcon = ImageManager.GetImage(bitmapAlias, (int)iconSize, (int)iconSize);
            List<int> icons = CardIcons[card.Value];
            foreach (int icon in icons) {
                Vector2 pos = IconPositions[icon - 1]; // for some reason, starts with 1
                // pos is 0..1
                Vector2 actual = pos * innerSize;
                Al.DrawBitmap(suitIcon, actual.X-iconSize*0.5f, actual.Y-iconSize*0.5f, icon >= 10 ? FlipFlags.Vertical : FlipFlags.None);
            }
        }
        else {
            // draw pretty bitmap person
        }
        stack.Pop();
        
        stack.AsTransform(ref transform);
        Al.UseTransform(ref transform);
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