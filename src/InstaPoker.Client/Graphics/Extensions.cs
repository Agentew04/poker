using System.Drawing;
using System.Numerics;
using System.Reflection;
using SubC.AllegroDotNet;
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

    /// <summary>
    /// Clears a bitmap.
    /// </summary>
    /// <param name="bitmap">The bitmap to be cleared</param>
    public static void Clear(this AllegroBitmap bitmap) {
        AllegroBitmap prev = Al.GetTargetBitmap()!;
        Al.SetTargetBitmap(bitmap);
        Al.DrawFilledRectangle(0,0,Al.GetBitmapWidth(bitmap), Al.GetBitmapHeight(bitmap), 
            new AllegroColor {
                A = 0
            });
        Al.SetTargetBitmap(prev);
    }

    /// <summary>
    /// Maps a value from one range to another
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <param name="sourceMin">The minimum value in the incoming range. Input equals to this returns
    /// <c>destMin</c></param>
    /// <param name="sourceMax">The maximum value in the incoming range. Input equalts to this returns
    /// <c>destMax</c></param>
    /// <param name="destMin">The minimum value in the outgoing range. Input equals to <c>sourceMin</c>
    /// returns this value</param>
    /// <param name="destMax">The maximum value in the outgoing range. Input equals to <c>sourceMax</c>
    /// returns this value</param>
    /// <returns></returns>
    public static float Map(this float value, float sourceMin, float sourceMax, float destMin, float destMax) {
        if (MathF.Abs(sourceMax - sourceMin) < 1e-6f) return destMin;
        float t = (value - sourceMin) / (sourceMax - sourceMin);
        return destMin + t * (destMax - destMin);
    }

    private static FieldInfo? _nativePointerFieldInfo;
    
    /// <summary>
    /// Returns the internal pointer from Allegro objects.
    /// </summary>
    /// <param name="nativePointer"></param>
    /// <returns></returns>
    public static IntPtr GetPointer(this NativePointer nativePointer) {
        _nativePointerFieldInfo ??= typeof(NativePointer).GetField("Pointer", BindingFlags.NonPublic | BindingFlags.Instance);
        return (IntPtr)_nativePointerFieldInfo!.GetValue(nativePointer)!;
    }

    /// <summary>
    /// Discards the Z component of a <see cref="Vector3"/> and creates a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="vec3">The vector to be converted</param>
    /// <returns>The new vector with 2 components</returns>
    public static Vector2 ToVec2(this Vector3 vec3) {
        return new Vector2(vec3.X, vec3.Y);
    }

    /// <summary>
    /// Gets the width of a string using a font. Does not fail on strings containing non-ASCII characters
    /// like <see cref="Al.GetTextWidth"/> does.
    /// </summary>
    /// <param name="text">The text to measure</param>
    /// <param name="font">The font to use</param>
    /// <returns>The width of the text in pixels</returns>
    public static int GetUtfStringWidth(this string text, AllegroFont font) {
        int w = 0;
        for (int i = 0; i < text.Length; i++) {
            if (i != text.Length - 1) {
                w += Al.GetGlyphAdvance(font, text[i], text[i + 1]);
            }
            else {
                w += Al.GetGlyphAdvance(font, text[i], ' ');
            }
        }
        return w;
    }

    /// <summary>
    /// Draws a UTF string to the screen at the given coordinates.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="font"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    public static void DrawUtfString(this string text, AllegroFont font, float x, float y, AllegroColor color) {
        float xx = x;
        for (int i = 0; i < text.Length;i++) {
            if (text[i] == '\n') {
                xx = x;
                y += Al.GetFontLineHeight(font);
            }
            Al.DrawGlyph(font, color, xx, y, text[i]);
            if (i < text.Length - 1) {
                xx += Al.GetGlyphAdvance(font, text[i], text[i + 1]);
            }
        }
    }
}