using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Styles;

public static class Colors {
    // extension(AllegroColor) {
        public static AllegroColor Black => new() {
            R = 0, G = 0, B = 0, A = 1
        };

        public static AllegroColor Red => new() {
            R = 1, G = 0, B = 0, A = 1
        };

        public static AllegroColor Green => new() {
            R = 0, G = 1, B = 0, A = 1
        };
        
        public static AllegroColor Blue => new() {
            R = 0, G = 0, B = 1, A = 1
        };
        
        public static AllegroColor White => new() {
            R = 1, G = 1, B = 1, A = 1
        };

        public static AllegroColor WhiteSmoke => new() {
            R = 0.9607843137254902f,
            G = 0.9607843137254902f,
            B = 0.9607843137254902f,
            A = 1
        };

        public static AllegroColor BackgroundWhite => new() {
            R = 0.9058823529411765f,
            G = 0.9607843137254902f,
            B = 1.0f,
            A = 1.0f
        };
    // }
}