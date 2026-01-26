using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Styles;

/// <summary>
/// Structure that holds colors and visual configuration for different aspects of a <see cref="Button"/>. 
/// </summary>
public struct ButtonStyle {
    
    /// <summary>
    /// The size of the font of the <see cref="Button.Label"/>.
    /// </summary>
    public int FontSize { get; set; }
    
    /// <summary>
    /// Color of the <see cref="Button.Label"/>.
    /// </summary>
    public AllegroColor Foreground { get; set; }
    public AllegroColor Background { get; set; }
    public AllegroColor BorderColor { get; set; }
    public int BorderSize { get; set; }
    public AllegroColor BackgroundHover { get; set; }
    public AllegroColor BackgroundPressed { get; set; }

    /// <summary>
    /// Default style for buttons.
    /// </summary>
    public static readonly ButtonStyle Default = new() {
        FontSize = 16,
        Foreground = Colors.WhiteSmoke,
        Background = new AllegroColor() {
            R = 0.30196078431372547f,
            G = 0.6705882352941176f,
            B = 0.9686274509803922f,
            A = 1.0f
        },
        BackgroundHover = new AllegroColor() {
            R = 0.3568627450980392f,
            G = 0.6901960784313725f,
            B = 0.9607843137254902f,
            A = 1.0f
        },
        BackgroundPressed = new AllegroColor() {
            R = 0.403921568627451f,
            G = 0.7098039215686275f,
            B = 0.9607843137254902f,
            A = 1.0f
        },
        BorderSize = 1,
        BorderColor = Colors.Black
    };

    /// <summary>
    /// Style for a negative button. Normally used to execute a 'dangerous', aggressive or negative action.
    /// </summary>
    public static readonly ButtonStyle RedButton = new() {
        FontSize = 16,
        Foreground = Colors.WhiteSmoke,
        Background = new AllegroColor() {
            R = 0.9607843137254902f,
            G = 0.3058823529411765f,
            B = 0.25882352941176473f,
            A = 1.0f
        },
        BackgroundHover = new AllegroColor() {
            R = 0.9607843137254902f,
            G = 0.3686274509803922f,
            B = 0.3254901960784314f,
            A = 1.0f
        },
        BackgroundPressed = new AllegroColor() {
            R = 0.9607843137254902f,
            G = 0.4392156862745098f,
            B = 0.403921568627451f,
            A = 1.0f
        },
        BorderColor = new AllegroColor() {
            R = 0.1411764705882353f,
            G = 0.050980392156862744f,
            B = 0.047058823529411764f,
            A = 1.0f
        },
        BorderSize = 1
    };
}