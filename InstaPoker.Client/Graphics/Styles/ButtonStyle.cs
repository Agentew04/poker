using System.Drawing;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Styles;

public struct ButtonStyle {
    public int FontSize { get; set; }
    public AllegroColor Foreground { get; set; }
    public AllegroColor Background { get; set; }
    public AllegroColor BorderColor { get; set; }
    public int BorderSize { get; set; }
    public AllegroColor BackgroundHover { get; set; }
    public AllegroColor BackgroundPressed { get; set; }

    public static readonly ButtonStyle Default = new ButtonStyle() {
        FontSize = 16,
        Foreground = AllegroColor.WhiteSmoke,
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
        BorderColor = AllegroColor.Black
    };
}