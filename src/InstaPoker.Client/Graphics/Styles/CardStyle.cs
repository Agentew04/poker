using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Styles;

public struct CardStyle {
    public AllegroColor BacksideBackground { get; set; }
    
    public AllegroColor BacksideForeground { get; set; }

    public AllegroColor FrontsideBackground { get; set; }

    public AllegroColor RedSuitColor { get; set; }

    public AllegroColor BlackSuitColor { get; set; }

    public AllegroColor BorderColor { get; set; }
    
    public int StripCount { get; set; }

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
        StripCount = 30
    };
}