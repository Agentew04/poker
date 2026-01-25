using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Styles;

public struct TextBoxStyle {
    
    public int FontSize { get; set; }
    public AllegroColor Foreground { get; set; }
    public AllegroColor PlaceholderForeground { get; set; }
    public AllegroColor Background { get; set; }
    public AllegroColor BorderColor { get; set; }
    public int BorderSize { get; set; }
    public AllegroColor BackgroundHover { get; set; }
    public AllegroColor BackgroundPressed { get; set; }

    public static readonly TextBoxStyle Default = new() {
        Foreground = new AllegroColor() {
            R = 0.2f,
            G = 0.2f,
            B = 0.2f,
            A = 1.0f
        },
        Background = Colors.BackgroundWhite,
        BorderSize = 1,
        BorderColor = Colors.Black,
        PlaceholderForeground = new AllegroColor() {
            R = 0.5f,
            G = 0.5f,
            B = 0.5f,
            A = 1.0f
        },
        BackgroundHover = new AllegroColor() {
            R = 0.9411764705882353f,
            G = 0.9686274509803922f,
            B = 0.9882352941176471f,
            A = 1.0f
        },
        BackgroundPressed = new AllegroColor() {
            R = 0.9803921568627451f,
            G = 0.984313725490196f,
            B = 0.9882352941176471f,
            A = 1.0f
        }
    };
}