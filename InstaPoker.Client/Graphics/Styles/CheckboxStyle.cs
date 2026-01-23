using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Styles;

public struct CheckboxStyle {
    public AllegroColor BackgroundCheck { get; set; }
    public AllegroColor BackgroundCheckHover { get; set; }
    public AllegroColor BackgroundCheckPressed { get; set; }
    public AllegroColor Background { get; set; }
    public AllegroColor BackgroundHover { get; set; }
    public AllegroColor BackgroundPressed { get; set; }
    public AllegroColor Foreground { get; set; }
    public AllegroColor BorderColor { get; set; }
    public float BorderSize { get; set; }

    public static readonly CheckboxStyle Default = new() {
        Foreground = AllegroColor.White,
        BackgroundCheck = new AllegroColor {
            R = 0.13333333333333333f,
            G = 0.5450980392156862f,
            B = 0.9019607843137255f,
            A = 1
        },
        BackgroundCheckHover = new AllegroColor {
            R = 0.30196078431372547f,
            G = 0.6705882352941176f,
            B = 0.9686274509803922f,
            A = 1
        },
        BackgroundCheckPressed = new AllegroColor {
            R = 0.48627450980392156f,
            G = 0.7411764705882353f,
            B = 0.9490196078431372f,
            A = 1
        },
        Background = new AllegroColor {
            R = 0.9058823529411765f,
            G = 0.9607843137254902f,
            B = 1f,
            A = 1
        },
        BackgroundHover = new AllegroColor {
            R = 0.7607843137254902f,
            G = 0.8941176470588236f,
            B = 0.9882352941176471f,
            A = 1
        },
        BackgroundPressed = new AllegroColor {
            R = 0.6431372549019608f,
            G = 0.8470588235294118f,
            B = 0.9882352941176471f,
            A = 1
        },
        BorderColor = AllegroColor.Black,
        BorderSize = 1
    };
}