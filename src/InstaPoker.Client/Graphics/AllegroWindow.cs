using System.Reflection;
using System.Runtime.InteropServices;
using InstaPoker.Client.Network;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Extensions;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public abstract class AllegroWindow {

    private bool running = false;
    protected AllegroDisplay? Display = null;
    private IntPtr displayPtr;
    private string? windowTitle = null;
    private readonly List<KeyGesture> gestures = [];
    private readonly FontManager fontManager = new();
    private readonly AudioManager audioManager = new();
    
    private bool isFullscreen;
    private bool fromFullscreen = false;
    private int windowedPositionX;
    private int windowedPositionY;
    private int windowedWidth;
    private int windowedHeight;
    
    public int Width { get; private set; }
    public int Height { get; private set; }

    public string Title {
        get => windowTitle!;
        set => SetWindowTitle(value);
    }

    private bool InitializeAllegro() {
        if (!Al.InstallSystem(LibraryVersion.V526)) {
            Console.WriteLine("Cant install system;");
            return false;
        }

        Al.InitTtfAddon();
        Al.InitPrimitivesAddon();
        Al.InstallKeyboard();
        Al.InstallMouse();
        Al.InstallAudio();
        Al.InitACodecAddon();
        Al.InitImageAddon();

        return true;
    }

    private void DeinitializeAllegro() {
        Al.ShutdownImageAddon();
        
        audioManager.Dispose(); // destroy audio samples
        Al.UninstallAudio();
        
        Al.UninstallMouse();
        Al.UninstallKeyboard();
        Al.ShutdownPrimitivesAddon();
        
        fontManager.Dispose(); // destroy fonts
        Al.ShutdownTtfAddon();
        
        Al.UninstallSystem();
    }

    public void Run() {
        if (!InitializeAllegro()) {
            return;
        }
        Initialize();

        RegisterGesture(new KeyGesture([KeyCode.KeyEnter], KeyModifiers.LeftAlt, ToggleFullscreen));
        RegisterGesture(new KeyGesture([KeyCode.KeyF11], 0, ToggleFullscreen));
        
        CreateDisplay();
        AllegroEventQueue queue = Al.CreateEventQueue() ?? throw new Exception("Could not create event queue");
        RegisterEventSources(queue);

        // audio samples
        Al.ReserveSamples(4);

        running = true;
        double lastTime = Al.GetTime();
        
        double targetFps = Al.GetDisplayRefreshRate(Display);
        double targetFpsDelta = 1.0 / targetFps;
        while (running) {
            double time = Al.GetTime();
            double delta = time - lastTime;
            lastTime = time;

            ProcessEvents(queue);
            NetworkManager.Handler?.CheckForNewMessages();
            Update(delta);
            Render();
            
            Al.FlipDisplay();
            
            // wait for new frame
            double endTime = Al.GetTime();
            double frameDelta = endTime - lastTime;
            double sleepDelta = targetFpsDelta - frameDelta;
            if (sleepDelta > 0) {
                Thread.Sleep(TimeSpan.FromSeconds(sleepDelta));
            }
        }

        WindowClosing?.Invoke();
        UnregisterEventSources(queue);
        Al.DestroyEventQueue(queue);
        Al.DestroyDisplay(Display);
        Display = null;
        DeinitializeAllegro();
    }

    private void CreateDisplay() {
        if (windowTitle is null) {
            throw new Exception("Window title cant be null");
        }
        // Al.SetNewDisplayOption(DisplayOption.SampleBuffers, 1, DisplayImportance.Suggest);
        // Al.SetNewDisplayOption(DisplayOption.Samples, 4, DisplayImportance.Suggest);
        Al.SetNewDisplayFlags(DisplayFlags.Windowed|DisplayFlags.Resizable);
        windowedWidth = 1280;
        windowedHeight = 720;
        Display = Al.CreateDisplay(windowedWidth, windowedHeight) ?? throw new Exception("Could not create display");
        Width = Al.GetDisplayWidth(Display);
        Height = Al.GetDisplayHeight(Display);
        displayPtr = GetPointer(Display);
        Al.SetWindowTitle(Display, windowTitle);
    }

    private void RegisterEventSources(AllegroEventQueue queue) {
        Al.RegisterEventSource(queue, Al.GetKeyboardEventSource());
        Al.RegisterEventSource(queue, Al.GetMouseEventSource());
        Al.RegisterEventSource(queue, Al.GetDisplayEventSource(Display));
        
    }

    private void UnregisterEventSources(AllegroEventQueue queue) {
        Al.UnregisterEventSource(queue, Al.GetKeyboardEventSource());
        Al.UnregisterEventSource(queue, Al.GetMouseEventSource());
        Al.UnregisterEventSource(queue, Al.GetDisplayEventSource(Display));
    }

    private void ProcessEvents(AllegroEventQueue queue) {
        AllegroEvent e = new();
        while (!queue.IsEventQueueEmpty()) {
            if (!queue.GetNextEvent(ref e)) {
                continue;
            }

            switch (e.Type) {
                case EventType.KeyDown: {
                    KeyModifiers modif = (KeyModifiers)e.Keyboard.Modifiers;
                    if (e.Keyboard.Modifiers == 66) {
                        modif = KeyModifiers.RightAlt;
                    }
                    gestures.ForEach(x => x.OnKeyDown(e.Keyboard.KeyCode, modif));
                    OnKeyDown(e.Keyboard.KeyCode, modif);
                    break;
                }
                case EventType.KeyUp: {
                    KeyModifiers modif = (KeyModifiers)e.Keyboard.Modifiers;
                    if (e.Keyboard.Modifiers == 66) {
                        modif = KeyModifiers.RightAlt;
                    }
                    gestures.ForEach(x => x.OnKeyUp(e.Keyboard.KeyCode, modif));
                    OnKeyUp(e.Keyboard.KeyCode, modif);
                    break;
                }
                case EventType.MouseAxes: {
                    OnMouseMove(e.Mouse.X, e.Mouse.Y, e.Mouse.DX, e.Mouse.DY);
                    break;
                }
                case EventType.MouseButtonDown: {
                    OnMouseDown((MouseButton)e.Mouse.Button);
                    break;
                }
                case EventType.MouseButtonUp: {
                    OnMouseUp((MouseButton)e.Mouse.Button);
                    break;
                }
                case EventType.DisplayClose: {
                    running = false;
                    break;
                }
                case EventType.DisplayResize: {
                    if (fromFullscreen) {
                        Al.ResizeDisplay(Display, windowedWidth, windowedHeight);
                        fromFullscreen = false;
                        Width = windowedWidth;
                        Height = windowedHeight;
                    }
                    else {
                        Al.AcknowledgeResize(Display);
                        Width = e.Display.Width;
                        Height = e.Display.Height;
                    }
                    break;
                }
                case EventType.KeyChar: {
                    if (e.Keyboard.Unichar > 0) {
                        OnCharDown((char)e.Keyboard.Unichar);
                    }
                    break;
                }
            }
        }
    }

    private void SetWindowTitle(string title) {
        if (title == windowTitle) {
            return;
        }
        windowTitle = title;
        if (Display is not null) {
            Al.SetWindowTitle(Display, windowTitle);
        }
    }

    protected void RegisterGesture(KeyGesture gesture) {
        gestures.Add(gesture);
    }
    
    private void ToggleFullscreen() {
        if (!isFullscreen) {
            // save old size
            windowedWidth = Al.GetDisplayWidth(Display);
            windowedHeight = Al.GetDisplayHeight(Display);
            Al.GetWindowPosition(Display, out windowedPositionX, out windowedPositionY); 
        
            // resize
            int adapter = al_get_display_adapter(displayPtr);
            Al.GetMonitorInfo(adapter, out AllegroMonitorInfo monitorInfo);
            int w = Math.Abs(monitorInfo.X1 - monitorInfo.X2);
            int h = Math.Abs(monitorInfo.Y1 - monitorInfo.Y2);
            Al.ResizeDisplay(Display, w, h);
            Width = w;
            Height = h;

            // set borderless
            Al.SetDisplayFlag(Display, DisplayFlags.FullscreenWindow, true);
            isFullscreen = true;
        }
        else {
            Al.ResizeDisplay(Display, windowedWidth, windowedHeight);
            Al.SetDisplayFlag(Display, DisplayFlags.FullscreenWindow, false);
            Al.SetWindowPosition(Display, windowedPositionX, windowedPositionY);
            Width = windowedWidth;
            Height = windowedHeight;
            isFullscreen = false;
            fromFullscreen = true;
        }
    }

    private IntPtr GetPointer(NativePointer ptr) {
        FieldInfo? field = typeof(NativePointer).GetField("Pointer", BindingFlags.NonPublic | BindingFlags.Instance);
        return (IntPtr)field!.GetValue(ptr)!;
    }

    [DllImport("allegro-5.2.dll")]
    private static extern int al_get_display_adapter(IntPtr display);

    /// <inheritdoc cref="IRenderObject.Initialize"/>
    protected abstract void Initialize();

    /// <inheritdoc cref="IRenderObject.Update"/>
    protected abstract void Update(double delta);

    /// <inheritdoc cref="IRenderObject.Render"/>
    protected abstract void Render();
    
    /// <inheritdoc cref="IKeyboardInteractable.OnKeyDown"/>
    protected abstract void OnKeyDown(KeyCode key, KeyModifiers modifiers);

    /// <inheritdoc cref="IKeyboardInteractable.OnCharDown"/>
    protected abstract void OnCharDown(char character);

    /// <inheritdoc cref="IKeyboardInteractable.OnKeyUp"/>
    protected abstract void OnKeyUp(KeyCode key, KeyModifiers modifiers);

    /// <inheritdoc cref="IMouseInteractable.OnMouseMove"/>
    protected abstract void OnMouseMove(int x, int y, int dx, int dy);

    /// <inheritdoc cref="IMouseInteractable.OnMouseDown"/>
    protected abstract void OnMouseDown(MouseButton button);
    
    /// <inheritdoc cref="IMouseInteractable.OnMouseUp"/>
    protected abstract void OnMouseUp(MouseButton button);

    /// <summary>
    /// Event fired when the window starts to close. This is executed before any deallocation or stopping of
    /// graphics systems.
    /// </summary>
    public event Action? WindowClosing = null;

    /// <summary>
    /// Closes the window.
    /// </summary>
    public void Close() {
        running = false;
    }
}