using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public sealed class MouseManager {

    private AllegroDisplay? display;
    private SystemMouseCursor cursor;
    private bool requested;

    public void Initialize(AllegroDisplay display) {
        this.display = display;
    }

    public void RequestMouse(SystemMouseCursor cursor) {
        this.cursor = cursor;
        requested = true;
    }

    public void CommitChanges() {
        if (!requested) {
            Al.SetSystemMouseCursor(display, SystemMouseCursor.Default);
            cursor = SystemMouseCursor.Default;
            return;
        }

        Al.SetSystemMouseCursor(display, cursor);
    }

    public void NewFrame() {
        requested = false;
    }
}