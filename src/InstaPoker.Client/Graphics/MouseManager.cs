using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public static class MouseManager {

    private static AllegroDisplay display;
    private static SystemMouseCursor cursor;
    private static bool requested;

    public static void Initialize(AllegroDisplay display) {
        MouseManager.display = display;
    }

    public static void RequestMouse(SystemMouseCursor cursor) {
        MouseManager.cursor = cursor;
        requested = true;
    }

    public static void CommitChanges() {
        if (!requested) {
            Al.SetSystemMouseCursor(display, SystemMouseCursor.Default);
            cursor = SystemMouseCursor.Default;
            return;
        }

        Console.WriteLine("cursor: " + cursor);
        Al.SetSystemMouseCursor(display, cursor);
    }

    public static void NewFrame() {
        requested = false;
    }
}