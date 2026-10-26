using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Cards;

/// <summary>
/// Visual parameters for a playing card. A single instance can be shared by every card on the table.
/// </summary>
public struct CardStyle {

    public AllegroColor BacksideBackground { get; set; }
    public AllegroColor BacksideForeground { get; set; }
    public AllegroColor FrontsideBackground { get; set; }
    public AllegroColor RedSuitColor { get; set; }
    public AllegroColor BlackSuitColor { get; set; }
    public AllegroColor BorderColor { get; set; }

    /// <summary>The font used for the corner values and face-card letters.</summary>
    public string FontAlias { get; set; }

    /// <summary>Prefix of the suit icon aliases registered in the image manager.</summary>
    public string SuitIconPrefix { get; set; }

    public int StripCount { get; set; }
    public float BorderSize { get; set; }

    /// <summary>Scale applied while the card is hovered.</summary>
    public float HoverScale { get; set; }

    /// <summary>Duration of a 180° flip, in seconds.</summary>
    public float FlipDuration { get; set; }

    /// <summary>Time to reach the hovered scale, in seconds.</summary>
    public float HoverDuration { get; set; }

    public static readonly CardStyle Default = new() {
        BacksideForeground = new() {
            R = 0.8784313725490196f, G = 0.19215686274509805f, B = 0.19215686274509805f, A = 1
        },
        BacksideBackground = new() {
            R = 1, G = 0.788235294117647f, B = 0.788235294117647f, A = 1
        },
        FrontsideBackground = Colors.White,
        RedSuitColor = new() {
            R = 0.8784313725490196f, G = 0.19215686274509805f, B = 0.19215686274509805f, A = 1
        },
        BlackSuitColor = Colors.Black,
        BorderColor = Colors.Black,
        FontAlias = "ShareTech-Regular",
        SuitIconPrefix = "suit-",
        StripCount = 30,
        BorderSize = 2f,
        HoverScale = 1.1f,
        FlipDuration = 0.5f,
        HoverDuration = 0.15f
    };
}
