using System.Numerics;
using InstaPoker.Client.Graphics;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Extensions;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game;

internal class PokerGame : AllegroWindow {

    public PokerGame() {
        Title = "Poker";
    }
    
    private readonly RenderContext renderContext = new();
    
    private readonly IdentifyScreen identifyScreen = new();
    private readonly MainMenuScreen mainMenuScreen = new();
    private readonly AdminLobbyScreen adminLobbyScreen = new();
    private readonly PlayerLobbyScreen playerLobbyScreen = new();

    private IRenderObject renderScreen;

    public readonly string AppDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "instapoker"
    );

    protected override void Initialize() {
        FontManager.RegisterFont("ShareTech-Regular");
        AudioManager.RegisterAudio("bop.ogg", "bop");
        identifyScreen.Initialize();
        renderScreen = identifyScreen;
        identifyScreen.OkClicked += () => renderScreen = mainMenuScreen;
        mainMenuScreen.Initialize();
        adminLobbyScreen.Initialize();
        playerLobbyScreen.Initialize();
        mainMenuScreen.CreateRoomClicked += () => {
            renderScreen = adminLobbyScreen;
            adminLobbyScreen.OnShow();
        };
        mainMenuScreen.JoinRoomClicked += code => {
            renderScreen = playerLobbyScreen;
            playerLobbyScreen.OnShow(code);
        };
        adminLobbyScreen.OnLeave += () => {
            renderScreen = mainMenuScreen;
        };
        playerLobbyScreen.OnLeave += () => {
            renderScreen = mainMenuScreen;
        };
    }

    protected override void Update(double delta) {
        renderScreen.Update(delta);
    }

    protected override void Render() {
        AllegroColor color = new() {
            R = 0.647058823f,
            G = 0.84705882f,
            B = 1.0f,
            A = 1.0f
        };
        Al.ClearToColor(color);
        renderContext.UpdateTransform();
        
        renderScreen.Position = Vector2.Zero;
        renderScreen.Size = new Vector2(Width, Height);
        renderScreen.Render(renderContext);
    }

    protected override void OnKeyDown(KeyCode key, uint modifiers) {
        if (renderScreen is IKeyboardInteractable keyb) {
            keyb.OnKeyDown(key,modifiers);
        }
    }

    protected override void OnCharDown(char character) {
        if (renderScreen is IKeyboardInteractable keyb) {
            keyb.OnCharDown(character);
        }
    }

    protected override void OnKeyUp(KeyCode key, uint modifiers) {
        if (renderScreen is IKeyboardInteractable keyb) {
            keyb.OnKeyUp(key,modifiers);
        }
    }

    protected override void OnMouseMove(int x, int y, int dx, int dy) {
        if (renderScreen is IMouseInteractable mouse) {
            mouse.OnMouseMove(new Vector2(x,y), new Vector2(dx,dy));
        }
    }

    protected override void OnMouseDown(uint button) {
        if (renderScreen is IMouseInteractable mouse) {
            mouse.OnMouseDown(button);
        }
    }

    protected override void OnMouseUp(uint button) {
        if (renderScreen is IMouseInteractable mouse) {
            mouse.OnMouseUp(button);
        }
    }
}
