using System.Drawing;
using System.Numerics;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public static class Extensions {
    /// <summary>
    /// Transforms a <see cref="System.Drawing.Color"/>, that uses bytes, into a <see cref="AllegroColor"/>
    /// that uses floats.
    /// </summary>
    /// <param name="color">The color to be converted</param>
    /// <returns>Same color using floating point values</returns>
    public static AllegroColor ToAllegroColor(this Color color) {
        return new AllegroColor {
            R = color.R / 255.0f, G = color.G / 255.0f, B = color.B / 255.0f, A = color.A / 255.0f
        };
    }

    /// <summary>
    /// Projects the <see cref="Vector2"/> <paramref name="a"/> into <paramref name="b"/>. 
    /// </summary>
    /// <param name="a">The vector that will be projected</param>
    /// <param name="b">The base vector that receives the projection</param>
    /// <returns>The new vector</returns>
    public static Vector2 ProjectOnto(this Vector2 a, Vector2 b) {
        return Vector2.Dot(a, b) / Vector2.Dot(b, b) * b;
    }

    public static Vector2 OrthogonalProjectionOnto(this Vector2 a, Vector2 b) {
        return a - a.ProjectOnto(b);
    }

    /// <summary>
    /// Gets the point of intersection between two lines. Parameters are passed as two point on each line.
    /// </summary>
    /// <param name="line0p0">First point on the first line</param>
    /// <param name="line0p1">Second point on the first line</param>
    /// <param name="line1p0">First point on the second line</param>
    /// <param name="line1p1">Second point on the second line</param>
    /// <returns></returns>
    public static Vector2 LineLineIntersection(Vector2 line0p0, Vector2 line0p1, Vector2 line1p0, Vector2 line1p1) {
        float x1 = line0p0.X;
        float x2 = line0p1.X;
        float x3 = line1p0.X;
        float x4 = line1p1.X;
        float y1 = line0p0.Y;
        float y2 = line0p1.Y;
        float y3 = line1p0.Y;
        float y4 = line1p1.Y;
        float denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        float leftComponent = x1 * y2 - y1 * x2;
        float rightComponent = x3 * y4 - y3 * x4;
        return new Vector2(
            x: (leftComponent * (x3 - x4) - (x1 - x2) * rightComponent) / denominator,
            y: (leftComponent * (y3 - y4) - (y1 - y2) * rightComponent) / denominator
        );
    }
}