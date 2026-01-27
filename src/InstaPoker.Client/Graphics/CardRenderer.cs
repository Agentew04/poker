using System.Numerics;
using InstaPoker.Client.Game;
using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public class CardRenderer {


    public Vector2 CardSize { get; set; }
    
    public RenderContext RenderContext { get; set; }

    public CardStyle Style { get; set; }
    
    /// <summary>
    /// Draws a playing card centralized at <paramref name="pos"/> with size <see cref="CardSize"/>.
    /// </summary>
    /// <param name="card">The value of the card that will be rendered</param>
    /// <param name="pos">The centralized position of the card</param>
    /// <param name="faceDown">Whether the card is facing down or up</param>
    public void RenderAt(GameCard card, Vector2 pos, bool faceDown) {
        Vector2 topLeft = pos - CardSize * 0.5f;
        Vector2 bottomRight = pos + CardSize * 0.5f;
        RenderContext.Stack.Push();
        RenderContext.Stack.Multiply(Matrix4x4.CreateTranslation(topLeft.X, topLeft.Y, 0));
        RenderContext.UpdateTransform();
        // Al.SetClippingRectangle(0, 0, (int)CardSize.X, (int)CardSize.Y);
        
        if (faceDown) {
            // draw background
            Al.DrawFilledRectangle(0, 0, CardSize.X, CardSize.Y, Style.BacksideBackground);
            
            Vector2 P3 = CardSize with { X = -CardSize.X };
            const float angle = MathF.PI / 4; // 45 degrees
            Vector2 r = new(-MathF.Cos(-angle), -MathF.Sin(-angle)); // (-1,0) rotated by 'angle' degrees
            Vector2 P4 = Vector2.Dot(P3, r) / Vector2.Dot(r, r) * r;
            float distance = P4.Length(); // distance between P4 and (0,0)
            int bandCount = 2 * Style.StripCount + 1;
            float stripThickness = distance / bandCount;
            float halfThick = stripThickness * 0.5f;
            Vector2 r90 = new(-r.Y, r.X);
            for (int i = 0; i < bandCount; i++) {
                if (i % 2 != 1) {
                    continue;
                }
                Vector2 P5 = r * (i*stripThickness) + r * halfThick;
                Vector2 rightSideIntersection =
                    Extensions.LineLineIntersection(P5, P5 + r90, new Vector2(halfThick,0),  new Vector2(halfThick,1));
                Vector2 bottomSideIntersection =
                    Extensions.LineLineIntersection(P5, P5 + r90, new Vector2(0,-halfThick), new Vector2(-CardSize.X,-halfThick));
                Vector2 topSideIntersection =
                    Extensions.LineLineIntersection(P5, P5 + r90, new Vector2(0, CardSize.Y+halfThick), new Vector2(-CardSize.X,CardSize.Y+halfThick));
                Vector2 leftSideIntersection =
                    Extensions.LineLineIntersection(P5, P5 + r90, new Vector2(-CardSize.X-halfThick, 0), new Vector2(-CardSize.X-halfThick, CardSize.Y));

                Vector2 left = Vector2.Distance(P5, leftSideIntersection) < Vector2.Distance(P5, bottomSideIntersection)
                    ? leftSideIntersection
                    : bottomSideIntersection;
                Vector2 right = Vector2.Distance(P5, rightSideIntersection) < Vector2.Distance(P5, topSideIntersection)
                    ? rightSideIntersection
                    : topSideIntersection;
                
                Al.DrawLine(CardSize.X + left.X, CardSize.Y - left.Y, 
                    CardSize.X + right.X, CardSize.Y - right.Y, 
                    Style.BacksideForeground, stripThickness);
            }
        }else
        {
            // background
            Al.DrawFilledRectangle(0, 0, CardSize.X, CardSize.Y, Style.FrontsideBackground);
            
            const float marginRate = 0.191780821917808f;
            float margin = CardSize.X * marginRate;
            Al.DrawRectangle(margin, margin, 
                CardSize.X - margin, CardSize.Y - margin,
                Colors.Black, 1);

            AllegroColor accent = card.Suit is Suit.Clubs or Suit.Spades ? Style.BlackSuitColor : Style.RedSuitColor;
            
            // draw corner indicators
            AllegroFont font = FontManager.GetFont("ShareTech-Regular", 28);
            Al.DrawText(font, accent, (int)(CardSize.X*0.5f), (int)(CardSize.Y*0.5f - Al.GetFontLineHeight(font)*0.5f), FontAlignFlags.Center,
                "A");
            
        }
            
        // draw border
        Al.DrawRectangle((int)1, (int)1, (int)CardSize.X-1, (int)CardSize.Y, Style.BorderColor, 2);
        
        // Al.ResetClippingRectangle();
        RenderContext.Stack.Pop();
    }

    private static Dictionary<int, List<int>> iconPlaces = new(){
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
}