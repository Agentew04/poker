using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Network;

namespace InstaPoker.Client.Game;

public class PlayerLobbyScreen : IRenderObject, IMouseInteractable {

    private string code;
    private readonly LoadingLabel loading = new();
    
    public void Initialize() {
        loading.Initialize();
        loading.Size = Size;
        loading.Position = Vector2.Zero;
        loading.Text = "Joining room";
        loading.FontSize = 28;
    }

    public void OnShow(string code) {
        this.code = code;
        Task joinTask = NetworkManager.JoinRoom(code);
        joinTask.ContinueWith(x => {
            loading.Hide();
        });
    }

    public void Render(RenderContext ctx) {
        loading.Render(ctx);
    }

    public void Update(double delta) {
        loading.Update(delta);
    }

    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }

    public void OnMouseMove(int x, int y, int dx, int dy) {
    }

    public void OnMouseDown(uint button) {
    }

    public void OnMouseUp(uint button) {
    }
}