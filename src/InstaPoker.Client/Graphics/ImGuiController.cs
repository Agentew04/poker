using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

/// <summary>
/// Backend controller to draw ImGui interfaces using Allegro5 drawing routines
/// </summary>
/// <remarks>Partially adapted from <see href="https://github.com/ocornut/imgui/blob/master/backends/imgui_impl_allegro5.cpp"/></remarks>
public sealed partial class ImGuiController : IDisposable {
    /// <summary>
    /// The width of the Allegro Window.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// The height of the Allegro Window
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// If the entire ImGui should receive updates and be renderized
    /// to the screen.
    /// </summary>
    public bool IsEnabled { get; set; }

    private AllegroVertexDecl vertexDecl;
    private AllegroVertex2D[] vertexBuffer;
    private int[] indexBuffer;
    private Dictionary<IntPtr, AllegroBitmap> bitmaps = [];

    /// <summary>
    /// Initializes the ImGui library and setups required buffers.
    /// </summary>
    public ImGuiController() {
        // create custom Vertex Declaration
        // AllegroDotNet only has overloads using NULL
        // NULL draws only solid color triangles
        vertexDecl = Al.CreateVertexDecl(
        [
            new AllegroVertexElement() {
                Attribute = 1, // prim position
                Storage = 0,
                Offset = 0
            },
            new AllegroVertexElement() {
                Attribute = 3, // tex coord
                Storage = 0,
                Offset = 8
            },
            new AllegroVertexElement() {
                Attribute = 2, // color
                Storage = 0,
                Offset = 16
            },
            new AllegroVertexElement() {
                Attribute = 0,
                Storage = 0,
                Offset = 0
            }
        ], 32)!;

        ImGui.CreateContext();
        ImGui.GetIO().Fonts.AddFontDefault();
        if (!ImGui.GetIO().Fonts.IsBuilt()) {
            ImGui.GetIO().Fonts.Build();
            CreateFontTexture();
        }

        vertexBuffer = new AllegroVertex2D[256];
        indexBuffer = new int[256];
    }

    /// <summary>
    /// Sends a time update to the ImGui library. Also marks the start of a frame for
    /// ImGui.
    /// </summary>
    /// <param name="deltaTime">How much time passed since last frame.</param>
    public void Update(float deltaTime) {
        SetPerFrameData(deltaTime);
        ImGui.NewFrame();
    }

    /// <summary>
    /// Renders one frame of the ImGui interface.
    /// </summary>
    public void Render() {
        ImGui.EndFrame();
        if (IsEnabled) {
            ImGui.Render();
            RenderDrawData(ImGui.GetDrawData());
        }
    }

    /// <summary>
    /// Passes events to the ImGui library.
    /// </summary>
    /// <param name="e">The allegro event passed</param>
    /// <returns>If the event should be repassed to the main
    /// application or ImGui just consumed it</returns>
    public bool ProcessEvent(ref AllegroEvent e) {
        ImGuiIOPtr io = ImGui.GetIO();
        switch (e.Type) {
            case EventType.MouseAxes: {
                io.AddMousePosEvent(e.Mouse.X, e.Mouse.Y);
                io.AddMouseWheelEvent(-e.Mouse.DW, e.Mouse.DZ);
                return io.WantCaptureMouse;
            }
            case EventType.MouseButtonUp:
            case EventType.MouseButtonDown:
                if (e.Mouse.Button is > 0 and <= 5) {
                    io.AddMouseButtonEvent((int)(e.Mouse.Button - 1), e.Type == EventType.MouseButtonDown);
                }

                return io.WantCaptureMouse;
            case EventType.MouseLeaveDisplay:
                io.AddMousePosEvent(-float.MaxValue, -float.MaxValue);
                return io.WantCaptureMouse;
            case EventType.KeyChar: {
                if (e.Keyboard.Unichar != 0) {
                    io.AddInputCharacter((uint)e.Keyboard.Unichar);
                }

                return io.WantCaptureKeyboard;
            }
            case EventType.KeyDown:
            case EventType.KeyUp:
                UpdateKeyModifiers();
                ImGuiKey key = ToImGuiKey(e.Keyboard.KeyCode);
                io.AddKeyEvent(key, e.Type == EventType.KeyDown);
                io.SetKeyEventNativeData(key, (int)e.Keyboard.KeyCode, -1);
                return io.WantCaptureKeyboard;
            case EventType.DisplaySwitchOut:
                io.AddFocusEvent(false);
                break;
            case EventType.DisplaySwitchIn:
                io.AddFocusEvent(true);
                break;
        }

        return false;
    }

    private void UpdateKeyModifiers() {
        ImGuiIOPtr io = ImGui.GetIO();
        AllegroKeyboardState keys = new();
        Al.GetKeyboardState(ref keys);
        io.AddKeyEvent(ImGuiKey.ModCtrl, Al.KeyDown(ref keys, KeyCode.KeyLeftControl));
        io.AddKeyEvent(ImGuiKey.ModShift, Al.KeyDown(ref keys, KeyCode.KeyLeftShift)
                                          || Al.KeyDown(ref keys, KeyCode.KeyRightShift));
        io.AddKeyEvent(ImGuiKey.ModAlt, Al.KeyDown(ref keys, KeyCode.KeyAlt)
                                        || Al.KeyDown(ref keys, KeyCode.KeyAltGr));
        io.AddKeyEvent(ImGuiKey.ModSuper, Al.KeyDown(ref keys, KeyCode.KeyLeftWindows)
                                          || Al.KeyDown(ref keys, KeyCode.KeyRightWindows));
    }

    private ImGuiKey ToImGuiKey(KeyCode code) {
        switch (code) {
            case KeyCode.KeyTab: return ImGuiKey.Tab;
            case KeyCode.KeyLeft: return ImGuiKey.LeftArrow;
            case KeyCode.KeyRight: return ImGuiKey.RightArrow;
            case KeyCode.KeyUp: return ImGuiKey.UpArrow;
            case KeyCode.KeyDown: return ImGuiKey.DownArrow;
            case KeyCode.KeyPageUp: return ImGuiKey.PageUp;
            case KeyCode.KeyPageDown: return ImGuiKey.PageDown;
            case KeyCode.KeyHome: return ImGuiKey.Home;
            case KeyCode.KeyEnd: return ImGuiKey.End;
            case KeyCode.KeyInsert: return ImGuiKey.Insert;
            case KeyCode.KeyDelete: return ImGuiKey.Delete;
            case KeyCode.KeyBackspace: return ImGuiKey.Backspace;
            case KeyCode.KeySpace: return ImGuiKey.Space;
            case KeyCode.KeyEnter: return ImGuiKey.Enter;
            case KeyCode.KeyEscape: return ImGuiKey.Escape;
            case KeyCode.KeyQuote: return ImGuiKey.Apostrophe;
            case KeyCode.KeyComma: return ImGuiKey.Comma;
            case KeyCode.KeyMinus: return ImGuiKey.Minus;
            case KeyCode.KeyFullStop: return ImGuiKey.Period;
            case KeyCode.KeySlash: return ImGuiKey.Slash;
            case KeyCode.KeySemiColon: return ImGuiKey.Semicolon;
            case KeyCode.KeyEquals: return ImGuiKey.Equal;
            case KeyCode.KeyOpenBrace: return ImGuiKey.LeftBracket;
            case KeyCode.KeyBackslash: return ImGuiKey.Backslash;
            case KeyCode.KeyCloseBrace: return ImGuiKey.RightBracket;
            case KeyCode.KeyTilde: return ImGuiKey.GraveAccent;
            case KeyCode.KeyCapsLock: return ImGuiKey.CapsLock;
            case KeyCode.KeyScrollLock: return ImGuiKey.ScrollLock;
            case KeyCode.KeyNumberLock: return ImGuiKey.NumLock;
            case KeyCode.KeyPrintScreen: return ImGuiKey.PrintScreen;
            case KeyCode.KeyPause: return ImGuiKey.Pause;
            case KeyCode.KeyPad0: return ImGuiKey.Keypad0;
            case KeyCode.KeyPad1: return ImGuiKey.Keypad1;
            case KeyCode.KeyPad2: return ImGuiKey.Keypad2;
            case KeyCode.KeyPad3: return ImGuiKey.Keypad3;
            case KeyCode.KeyPad4: return ImGuiKey.Keypad4;
            case KeyCode.KeyPad5: return ImGuiKey.Keypad5;
            case KeyCode.KeyPad6: return ImGuiKey.Keypad6;
            case KeyCode.KeyPad7: return ImGuiKey.Keypad7;
            case KeyCode.KeyPad8: return ImGuiKey.Keypad8;
            case KeyCode.KeyPad9: return ImGuiKey.Keypad9;
            case KeyCode.KeyPadDelete: return ImGuiKey.KeypadDecimal;
            case KeyCode.KeyPadSlash: return ImGuiKey.KeypadDivide;
            case KeyCode.KeyPadAsterisk: return ImGuiKey.KeypadMultiply;
            case KeyCode.KeyPadMinus: return ImGuiKey.KeypadSubtract;
            case KeyCode.KeyPadPlus: return ImGuiKey.KeypadAdd;
            case KeyCode.KeyPadEnter: return ImGuiKey.KeypadEnter;
            case KeyCode.KeyPadEquals: return ImGuiKey.KeypadEqual;
            case KeyCode.KeyLeftControl: return ImGuiKey.LeftCtrl;
            case KeyCode.KeyLeftShift: return ImGuiKey.LeftShift;
            case KeyCode.KeyAlt: return ImGuiKey.LeftAlt;
            case KeyCode.KeyLeftWindows: return ImGuiKey.LeftSuper;
            case KeyCode.KeyRightControl: return ImGuiKey.RightCtrl;
            case KeyCode.KeyRightShift: return ImGuiKey.RightShift;
            case KeyCode.KeyAltGr: return ImGuiKey.RightAlt;
            case KeyCode.KeyRightWindows: return ImGuiKey.RightSuper;
            case KeyCode.KeyMenu: return ImGuiKey.Menu;
            case KeyCode.Key0: return ImGuiKey._0;
            case KeyCode.Key1: return ImGuiKey._1;
            case KeyCode.Key2: return ImGuiKey._2;
            case KeyCode.Key3: return ImGuiKey._3;
            case KeyCode.Key4: return ImGuiKey._4;
            case KeyCode.Key5: return ImGuiKey._5;
            case KeyCode.Key6: return ImGuiKey._6;
            case KeyCode.Key7: return ImGuiKey._7;
            case KeyCode.Key8: return ImGuiKey._8;
            case KeyCode.Key9: return ImGuiKey._9;
            case KeyCode.KeyA: return ImGuiKey.A;
            case KeyCode.KeyB: return ImGuiKey.B;
            case KeyCode.KeyC: return ImGuiKey.C;
            case KeyCode.KeyD: return ImGuiKey.D;
            case KeyCode.KeyE: return ImGuiKey.E;
            case KeyCode.KeyF: return ImGuiKey.F;
            case KeyCode.KeyG: return ImGuiKey.G;
            case KeyCode.KeyH: return ImGuiKey.H;
            case KeyCode.KeyI: return ImGuiKey.I;
            case KeyCode.KeyJ: return ImGuiKey.J;
            case KeyCode.KeyK: return ImGuiKey.K;
            case KeyCode.KeyL: return ImGuiKey.L;
            case KeyCode.KeyM: return ImGuiKey.M;
            case KeyCode.KeyN: return ImGuiKey.N;
            case KeyCode.KeyO: return ImGuiKey.O;
            case KeyCode.KeyP: return ImGuiKey.P;
            case KeyCode.KeyQ: return ImGuiKey.Q;
            case KeyCode.KeyR: return ImGuiKey.R;
            case KeyCode.KeyS: return ImGuiKey.S;
            case KeyCode.KeyT: return ImGuiKey.T;
            case KeyCode.KeyU: return ImGuiKey.U;
            case KeyCode.KeyV: return ImGuiKey.V;
            case KeyCode.KeyW: return ImGuiKey.W;
            case KeyCode.KeyX: return ImGuiKey.X;
            case KeyCode.KeyY: return ImGuiKey.Y;
            case KeyCode.KeyZ: return ImGuiKey.Z;
            case KeyCode.KeyF1: return ImGuiKey.F1;
            case KeyCode.KeyF2: return ImGuiKey.F2;
            case KeyCode.KeyF3: return ImGuiKey.F3;
            case KeyCode.KeyF4: return ImGuiKey.F4;
            case KeyCode.KeyF5: return ImGuiKey.F5;
            case KeyCode.KeyF6: return ImGuiKey.F6;
            case KeyCode.KeyF7: return ImGuiKey.F7;
            case KeyCode.KeyF8: return ImGuiKey.F8;
            case KeyCode.KeyF9: return ImGuiKey.F9;
            case KeyCode.KeyF10: return ImGuiKey.F10;
            case KeyCode.KeyF11: return ImGuiKey.F11;
            case KeyCode.KeyF12: return ImGuiKey.F12;
            default: return ImGuiKey.None;
        }
    }

    void SetPerFrameData(float deltaTime) {
        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(Width, Height);
        io.DeltaTime = deltaTime > 0 ? deltaTime : 1f / 60f;
    }

    void RenderDrawData(ImDrawDataPtr drawData) {
        // avoid rendering when minimized
        if (drawData.DisplaySize.X <= 0 || drawData.DisplaySize.Y <= 0) {
            return;
        }

        AllegroTransform lastTransform = Al.GetCurrentTransform()!.Value;
        AllegroTransform lastProjectionTransform = Al.GetCurrentProjectionTransform();

        int lastClipX, lastClipY, lastClipW, lastClipH;
        Al.GetClippingRectangle(out lastClipX, out lastClipY, out lastClipW, out lastClipH);
        BlendOperation lastBlenderOp;
        BlendMode lastBlenderSrc, lastBlenderDst;
        Al.GetBlender(out lastBlenderOp, out lastBlenderSrc, out lastBlenderDst);

        SetupRenderState(drawData);

        for (int n = 0; n < drawData.CmdListsCount; n++) {
            ImDrawListPtr drawList = drawData.CmdLists[n];
            // ensure vertex buffer capacity
            if (vertexBuffer.Length < drawList.VtxBuffer.Size) {
                Console.WriteLine("resize allegro imgui vertex buffer");
                vertexBuffer = new AllegroVertex2D[drawList.VtxBuffer.Size];
            }

            // convert imgui vertices to allegro vertices
            for (int i = 0; i < drawList.VtxBuffer.Size; i++) {
                ToAllegroVertex(ref vertexBuffer[i], drawList.VtxBuffer[i]);
            }

            // ensure index buffer capacity
            if (indexBuffer.Length < drawList.IdxBuffer.Size) {
                Console.WriteLine("resize allegro imgui index buffer");
                indexBuffer = new int[drawList.IdxBuffer.Size];
            }

            // convert ushort indices to uint
            for (int i = 0; i < drawList.IdxBuffer.Size; i++) {
                indexBuffer[i] = drawList.IdxBuffer[i];
            }

            // render command lists
            Vector2 clipOff = drawData.DisplayPos;
            for (int i = 0; i < drawList.CmdBuffer.Size; i++) {
                ImDrawCmdPtr pcmd = drawList.CmdBuffer[i];
                if (pcmd.UserCallback != IntPtr.Zero) {
                    Console.WriteLine("Unsupported user callback. may be reset state");
                }
                else {
                    Vector2 clipMin = new(pcmd.ClipRect.X - clipOff.X, pcmd.ClipRect.Y - clipOff.Y);
                    Vector2 clipMax = new(pcmd.ClipRect.Z - clipOff.X, pcmd.ClipRect.W - clipOff.Y);
                    if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y) {
                        continue;
                    }

                    Al.SetClippingRectangle((int)clipMin.X, (int)clipMin.Y,
                        (int)(clipMax.X - clipMin.X), (int)(clipMax.Y - clipMin.Y));

                    if (!bitmaps.TryGetValue(pcmd.GetTexID(), out AllegroBitmap? bitmap)) {
                        // Al.DrawIndexedPrim(vertexBuffer, null, indexBuffer, (int)(pcmd.IdxOffset + pcmd.ElemCount),
                        //     PrimType.TriangleList);
                        Console.WriteLine($"Couldn't find texture bitmap: {pcmd.GetTexID()}");
                        continue;
                    }

                    AllegroBitmap texture = bitmaps[pcmd.GetTexID()];
                    al_draw_indexed_prim(vertexBuffer, vertexDecl.GetPointer(), texture.GetPointer(), indexBuffer,
                        (int)(pcmd.IdxOffset + pcmd.ElemCount), (int)PrimType.TriangleList);
                    // Al.DrawIndexedPrim(vertexBuffer, null, indexBuffer, (int)(pcmd.IdxOffset + pcmd.ElemCount),
                    //     PrimType.TriangleList);
                }
            }
        }

        // restore modified allegro state
        Al.SetBlender(lastBlenderOp, lastBlenderSrc, lastBlenderDst);
        Al.SetClippingRectangle(lastClipX, lastClipY, lastClipW, lastClipH);
        Al.UseTransform(ref lastTransform);
        Al.UseProjectionTransform(ref lastProjectionTransform);
    }

    void SetupRenderState(ImDrawDataPtr drawData) {
        // setup blending
        Al.SetSeparateBLender(BlendOperation.Add, BlendMode.Alpha, BlendMode.InverseAlpha, BlendOperation.Add,
            BlendMode.One, BlendMode.InverseAlpha);

        // setup orthograpic projection matrix
        // NOTE: desnecessario? jogo 2D jah usa ortho
        float L = drawData.DisplayPos.X;
        float R = drawData.DisplayPos.X + drawData.DisplaySize.X;
        float T = drawData.DisplayPos.Y;
        float B = drawData.DisplayPos.Y + drawData.DisplaySize.Y;
        AllegroTransform transform;
        Al.IdentityTransform(ref transform);
        Al.UseTransform(ref transform);
        Al.OrthographicTransform(ref transform, L, T, 1.0f, R, B, -1.0f);
        Al.UseProjectionTransform(ref transform);
    }

    private void ToAllegroVertex(ref AllegroVertex2D allegroVertex, ImDrawVertPtr vertPtr) {
        allegroVertex.X = vertPtr.pos.X;
        allegroVertex.Y = vertPtr.pos.Y;
        allegroVertex.U = vertPtr.uv.X;
        allegroVertex.V = vertPtr.uv.Y;
        // var bmp = bitmaps.First().Value;
        // var col = Al.GetPixel(bmp, (int)(vertPtr.uv.X*Al.GetBitmapWidth(bmp)), (int)(vertPtr.uv.Y*Al.GetBitmapHeight(bmp))); 
        allegroVertex.Color = ToAllegroColor(vertPtr.col);
    }

    private static AllegroColor ToAllegroColor(uint col) {
        float r = (col & 0xFF) / 255f;
        float g = ((col >> 8) & 0xFF) / 255f;
        float b = ((col >> 16) & 0xFF) / 255f;
        float a = ((col >> 24) & 0xFF) / 255f;

        return new AllegroColor {
            R = r, G = g, B = b, A = a
        };
    }

    private void CreateFontTexture() {
        ImGuiIOPtr io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);
        AllegroBitmap? fontBitmap = Al.CreateBitmap(width, height);
        if (fontBitmap is null) {
            throw new Exception("Couldnt create bitmap for ImGui font atlas");
        }

        AllegroBitmap? prev = Al.GetTargetBitmap();
        Al.SetTargetBitmap(fontBitmap);
        unsafe {
            byte* src = (byte*)pixels;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int i = (y * width + x) * 4;
                    byte r = src[i + 0];
                    byte g = src[i + 1];
                    byte b = src[i + 2];
                    byte a = src[i + 3];

                    Al.PutPixel(x, y, new AllegroColor() {
                        R = r / 255f,
                        G = g / 255f,
                        B = b / 255f,
                        A = a / 255f
                    });
                }
            }
        }

        Al.SetTargetBitmap(prev);

        bitmaps[fontBitmap.GetPointer()] = fontBitmap;
        io.Fonts.SetTexID(fontBitmap.GetPointer());
        io.Fonts.ClearTexData(); // libera RAM
    }


    [LibraryImport("allegro_primitives-5.2.dll")]
    private static partial void al_draw_indexed_prim([In] AllegroVertex2D[] vtxs, IntPtr decl, IntPtr texture,
        [In] int[] indices, int num_vtx, int type);

    /// <summary>
    /// Releases all Allegro resources create by the <see cref="ImGuiController"/> 
    /// </summary>
    public void Dispose() {
        foreach (KeyValuePair<IntPtr, AllegroBitmap> kvp in bitmaps) {
            Al.DestroyBitmap(kvp.Value);
        }

        Al.DestroyVertexDecl(vertexDecl);
    }
}