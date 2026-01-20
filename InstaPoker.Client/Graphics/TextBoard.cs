using System.Numerics;
using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public class TextBoard : IRenderObject {

    public string Text { get; set; } = string.Empty;
    public int FontSize = 16;
    
    public TextBoardType Type { get; set; }
    
    public void Initialize() {
        // empty
    }

    public void Render(RenderContext ctx) {
        ctx.Stack.Push();
        ctx.Stack.Multiply(Matrix4x4.CreateTranslation(Position.X, Position.Y, 0));
        ctx.UpdateTransform();

        AllegroColor backgroundColor = Type switch {
            TextBoardType.Information => new AllegroColor {
                R = 0.396078431372549f,
                G = 0.6039215686274509f,
                B = 0.6784313725490196f,
            },
            TextBoardType.Warning => new AllegroColor {
                R = 0.9882352941176471f,
                G = 0.7019607843137254f,
                B = 0.0784313725490196f,
            },
            TextBoardType.Error => new AllegroColor {
                R = 0.7411764705882353f,
                G = 0.13333333333333333f,
                B = 0.15294117647058825f,
            },
            _ => throw new ArgumentOutOfRangeException()
        };
        backgroundColor.A = ctx.Alpha;

        AllegroColor foregroundColor = Type switch {
            TextBoardType.Information => AllegroColor.WhiteSmoke,
            TextBoardType.Warning => AllegroColor.Black,
            TextBoardType.Error => AllegroColor.White,
            _ => throw new ArgumentOutOfRangeException()
        };
        foregroundColor.A = ctx.Alpha;

        Al.DrawFilledRoundedRectangle(0,0, Size.X, Size.Y, 5,5, backgroundColor);
        AllegroFont font = FontManager.GetFont("ShareTech-Regular", FontSize);
        Al.DrawMultilineText(font, foregroundColor,
            (int)(Size.X*0.5f),
            (int)(Size.Y*0.5f - Al.GetFontLineHeight(font)*0.5f), Size.X,
            Al.GetFontLineHeight(font), FontAlignFlags.Center, Text);
        
        ctx.Stack.Pop();
    }

    public void Update(double delta) {
        // empty
    }

    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
}

public enum TextBoardType {
    Information,
    Warning,
    Error
}