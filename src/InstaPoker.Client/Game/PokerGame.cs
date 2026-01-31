using System.Numerics;
using ImGuiNET;
using InstaPoker.Client.Game.Screens;
using InstaPoker.Client.Graphics;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game;

/// <summary>
/// Main class that shapes the application logic. Controls event forwarding and ties calls and events
/// between different screens.
/// </summary>
public class PokerGame : AllegroWindow {

    /// <summary>
    /// Creates a new instance of the game.
    /// </summary>
    public PokerGame() {
        Title = "Poker";
    }
    
    private readonly RenderContext renderContext = new();
    private readonly DebugWindow debugWindow = new();
    
    private readonly IdentifyScreen identifyScreen = new();
    private readonly MainMenuScreen mainMenuScreen = new();
    private readonly AdminLobbyScreen adminLobbyScreen = new();
    private readonly PlayerLobbyScreen playerLobbyScreen = new();
    private readonly GameScreen gameScreen = new();

    private SceneObject renderScreen = null!;
    
    /// <inheritdoc cref="AllegroWindow.Initialize"/>
    protected override void Initialize() {
        FontManager.RegisterFont("ShareTech-Regular");
        RegisterAudio();
        RegisterImages();
        identifyScreen.Initialize();
        renderScreen = identifyScreen;
        identifyScreen.OkClicked += () => renderScreen = mainMenuScreen;
        mainMenuScreen.Initialize();
        adminLobbyScreen.Initialize();
        playerLobbyScreen.Initialize();
        gameScreen.Initialize();
        
        mainMenuScreen.CreateRoomClicked += () => {
            renderScreen = adminLobbyScreen;
            adminLobbyScreen.OnShow();
        };
        mainMenuScreen.JoinRoomClicked += code => {
            renderScreen = playerLobbyScreen;
            playerLobbyScreen.OnShow(code);
        };
        adminLobbyScreen.UserLeft += () => {
            renderScreen = mainMenuScreen;
        };
        playerLobbyScreen.UserLeft += () => {
            renderScreen = mainMenuScreen;
        };
        playerLobbyScreen.UpgradedToAdmin += (users, settings, code) => {
            renderScreen = adminLobbyScreen;
            adminLobbyScreen.OnUpgrade(users, settings, code);
        };
        adminLobbyScreen.GameStarted += () => {
            renderScreen = gameScreen;
            gameScreen.OnShow();
        };
        playerLobbyScreen.GameStarted += () => {
            renderScreen = gameScreen;
            gameScreen.OnShow();
        };
        gameScreen.UserLeft += () => {
            renderScreen = mainMenuScreen;
        };

        debugWindow.Game = this;
    }

    /// <inheritdoc cref="AllegroWindow.Update"/>
    protected override void Update(double delta) {
        renderScreen.Update(delta);
    }

    /// <inheritdoc cref="AllegroWindow.Render"/>
    protected override void Render() {
        AllegroColor color = new() {
            R = 0.647058823f,
            G = 0.84705882f,
            B = 1.0f,
            A = 1.0f
        };
        Al.ClearToColor(color);
        renderContext.UpdateTransform();

        int matrixStackBefore = renderContext.Stack.Count;
        int alphaStackBefore = renderContext.AlphaStack.Count;
        renderScreen.Position = Vector2.Zero;
        renderScreen.Size = new Vector2(Width, Height);
        
        renderScreen.PositionElements();
        renderScreen.Render(renderContext);
        
        debugWindow.Render();
        if (matrixStackBefore != renderContext.Stack.Count) {
            throw new Exception("Unbalanced Matrix Stack at end of frame. Aborting.");
        }

        if (alphaStackBefore != renderContext.AlphaStack.Count) {
            throw new Exception("Unbalanced Alpha Stack at end of frame. Aborting.");
        }
    }

    /// <inheritdoc cref="AllegroWindow.OnKeyDown"/>
    protected override void OnKeyDown(KeyCode key, KeyModifiers modifiers) {
        renderScreen.OnKeyDown(key,modifiers);
    }

    /// <inheritdoc cref="AllegroWindow.OnCharDown"/>
    protected override void OnCharDown(char character) {
        renderScreen.OnCharDown(character);
    }

    /// <inheritdoc cref="AllegroWindow.OnKeyUp"/>
    protected override void OnKeyUp(KeyCode key, KeyModifiers modifiers) {
        renderScreen.OnKeyUp(key,modifiers);
    }

    /// <inheritdoc cref="AllegroWindow.OnMouseMove"/>
    protected override void OnMouseMove(Vector2 pos, Vector2 delta) {
        renderScreen.OnMouseMove(pos,delta);
    }

    /// <inheritdoc cref="AllegroWindow.OnMouseDown"/>
    protected override void OnMouseDown(MouseButton button) {
        renderScreen.OnMouseDown(button);
    }

    /// <inheritdoc cref="AllegroWindow.OnMouseUp"/>
    protected override void OnMouseUp(MouseButton button) {
        renderScreen.OnMouseUp(button);
    }

    private static void RegisterAudio() {
        // bap
        AudioManager.RegisterAudio("bap1.ogg", "bap1");
        AudioManager.RegisterAudio("bap2.ogg", "bap2");
        AudioManager.RegisterAudio("bap3.ogg", "bap3");
        AudioManager.RegisterAudio("bap4.ogg", "bap4");
        AudioManager.RegisterAudio("bap5.ogg", "bap5");
        AudioManager.RegisterAudio("bap6.ogg", "bap6");
        AudioManager.RegisterAudio("bap7.ogg", "bap7");
        AudioManager.RegisterAudio("bap8.ogg", "bap8");
        AudioManager.RegisterAudio("bap9.ogg", "bap9");
        AudioManager.CreateGroup("bap", "bap1", "bap2", "bap3", "bap4", "bap5", "bap6", "bap7", "bap8", "bap9");
        
        // bop
        AudioManager.RegisterAudio("bop1.ogg", "bop1");
        AudioManager.RegisterAudio("bop2.ogg", "bop2");
        AudioManager.RegisterAudio("bop3.ogg", "bop3");
        AudioManager.RegisterAudio("bop4.ogg", "bop4");
        AudioManager.RegisterAudio("bop5.ogg", "bop5");
        AudioManager.RegisterAudio("bop6.ogg", "bop6");
        AudioManager.RegisterAudio("bop7.ogg", "bop7");
        AudioManager.RegisterAudio("bop8.ogg", "bop8");
        AudioManager.CreateGroup("bop", "bop1", "bop2", "bop3", "bop4", "bop5", "bop6", "bop7", "bop8");
    }

    private static void RegisterImages() {
        ImageManager.RegisterImage("suit-clubs-512.png", "suit-clubs");
        ImageManager.RegisterImage("suit-diamonds-512.png", "suit-diamonds");
        ImageManager.RegisterImage("suit-hearts-512.png", "suit-hearts");
        ImageManager.RegisterImage("suit-spades-512.png", "suit-spades");
    }
}
