using System.Drawing;
using System.Numerics;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public static class Extensions {

    public static AllegroColor ToAllegroColor(this Color color) {
        return new AllegroColor {
            R = color.R/255.0f, G = color.G/255.0f, B = color.B/255.0f, A = color.A/255.0f
        };
    }
}

public static class Utils {
    public static Vector2 TransformToLocal(Vector2 elementPosition, Vector2 mouseCoord) {
        return elementPosition - mouseCoord;
    }
}