using System.Runtime.InteropServices;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

/// <summary>
/// Adaptation of <see cref="AllegroVertex"/>, but with 2D position
/// coordinates. Used primarily by <see cref="ImGuiController"/>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct AllegroVertex2D {
    private float x;
    private float y;
    private float u;
    private float v;
    private float r;
    private float g;
    private float b;
    private float a;

    public AllegroColor Color {
        readonly get => new() {
            R = r, G = g, B = b, A = a
        };
        set {
            r = value.R;
            g = value.G;
            b = value.B;
            a = value.A;
        }
    }

    public float U
    {
        readonly get => this.u;
        set => this.u = value;
    }

    public float V
    {
        readonly get => this.v;
        set => this.v = value;
    }

    public float X
    {
        readonly get => this.x;
        set => this.x = value;
    }

    public float Y
    {
        readonly get => this.y;
        set => this.y = value;
    }
}