using System.Numerics;
using InstaPoker.Client.Game.Screens;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Objects;
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

    private readonly WindowFrame frame = new("frame");

    public SceneObject RootObject => frame.ActiveScreen;

    /// <inheritdoc cref="AllegroWindow.Initialize"/>
    protected override void Initialize() {
        FontManager.RegisterFont("ShareTech-Regular");
        RegisterAudio();
        RegisterImages();
        
        frame.Initialize();
        frame.IdentifyScreen.OkClicked += () => frame.ActiveScreen = frame.MainMenuScreen;
        
        frame.MainMenuScreen.CreateRoomClicked += () => {
            frame.ActiveScreen = frame.AdminLobbyScreen;
            frame.AdminLobbyScreen.OnShow();
        };
        frame.MainMenuScreen.JoinRoomClicked += code => {
            frame.ActiveScreen = frame.PlayerLobbyScreen;
            frame.PlayerLobbyScreen.OnShow(code);
        };
        frame.AdminLobbyScreen.UserLeft += () => {
            frame.ActiveScreen = frame.MainMenuScreen;
        };
        frame.PlayerLobbyScreen.UserLeft += () => {
            frame.ActiveScreen = frame.MainMenuScreen;
        };
        frame.PlayerLobbyScreen.UpgradedToAdmin += (users, settings, code) => {
            frame.ActiveScreen = frame.AdminLobbyScreen;
            frame.AdminLobbyScreen.OnUpgrade(users, settings, code);
        };
        frame.AdminLobbyScreen.GameStarted += () => {
            frame.ActiveScreen = frame.GameScreen;
            frame.GameScreen.OnShow();
        };
        frame.PlayerLobbyScreen.GameStarted += () => {
            frame.ActiveScreen = frame.GameScreen;
            frame.GameScreen.OnShow();
        };
        frame.GameScreen.UserLeft += () => {
            frame.ActiveScreen = frame.MainMenuScreen;
        };

        debugWindow.Game = this;
    }

    /// <inheritdoc cref="AllegroWindow.Update"/>
    protected override void Update(double delta) {
        frame.Update(delta);
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
        frame.Position = Vector2.Zero;
        frame.Size = new Vector2(Width, Height);
        
        frame.PositionElements();

        frame.Render(renderContext);
        
        Highlighter.DrawHighlights();
        
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
        frame.OnKeyDown(key,modifiers);
    }

    /// <inheritdoc cref="AllegroWindow.OnCharDown"/>
    protected override void OnCharDown(char character) {
        frame.OnCharDown(character);
    }

    /// <inheritdoc cref="AllegroWindow.OnKeyUp"/>
    protected override void OnKeyUp(KeyCode key, KeyModifiers modifiers) {
        frame.OnKeyUp(key,modifiers);
    }

    /// <inheritdoc cref="AllegroWindow.OnMouseMove"/>
    protected override void OnMouseMove(Vector2 pos, Vector2 delta) {
        frame.OnMouseMove(pos,delta);
    }

    /// <inheritdoc cref="AllegroWindow.OnMouseDown"/>
    protected override void OnMouseDown(MouseButton button) {
        frame.OnMouseDown(button);
    }

    /// <inheritdoc cref="AllegroWindow.OnMouseUp"/>
    protected override void OnMouseUp(MouseButton button) {
        frame.OnMouseUp(button);
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
        ImageManager.RegisterImage("logo-128.png", "logo");
    }
}
