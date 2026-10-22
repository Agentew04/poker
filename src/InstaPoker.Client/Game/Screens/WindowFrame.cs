using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Objects;

namespace InstaPoker.Client.Game.Screens;

public class WindowFrame : SceneObject {
    public WindowFrame(string name) : base(name) {
    }

    public override bool UseMouse => true;
    public override bool UseKeyboard => true;
    
    [ChildObject] public readonly IdentifyScreen IdentifyScreen = new();
    [ChildObject] public readonly MainMenuScreen MainMenuScreen = new();
    [ChildObject] public readonly AdminLobbyScreen AdminLobbyScreen = new();
    [ChildObject] public readonly PlayerLobbyScreen PlayerLobbyScreen = new();
    [ChildObject] public readonly GameScreen GameScreen = new();

    public SceneObject ActiveScreen { get; set; } = null!;

    public override void Initialize() {
        ActiveScreen = IdentifyScreen;
        
        // aqui resolve os servicos das diferentes views
        base.Initialize();
    }

    public override void Update(double delta) {
        IdentifyScreen.IsEnabled = ActiveScreen == IdentifyScreen;
        MainMenuScreen.IsEnabled = ActiveScreen == MainMenuScreen;
        AdminLobbyScreen.IsEnabled = ActiveScreen == AdminLobbyScreen;
        PlayerLobbyScreen.IsEnabled = ActiveScreen == PlayerLobbyScreen;
        GameScreen.IsEnabled = ActiveScreen == GameScreen;

        foreach (SceneObject child in GetChildren()) {
            child.Size = Size;
            child.Position = Vector2.Zero;
        }

        base.Update(delta);
    }

    public override void Render(RenderContext ctx) {
        base.Render(ctx);
    }
}