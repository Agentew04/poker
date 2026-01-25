using System.Drawing;
using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game;

public class MainMenuScreen : IRenderObject, IKeyboardInteractable, IMouseInteractable {

    private readonly Button createRoomButton = new();
    private readonly Button joinRoomButton = new();
    private readonly TextBox codeTextBox = new();
    private readonly Fader emptyCodeFader = new();
    private readonly TextBoard emptyCodeBoard = new();
    
    public void Initialize() {
        createRoomButton.Initialize();
        createRoomButton.Style = ButtonStyle.Default with {
            FontSize = 28
        };
        createRoomButton.Label = "Create Room";
        createRoomButton.Pressed += () => CreateRoomClicked?.Invoke();
        
        joinRoomButton.Initialize();
        joinRoomButton.Style = ButtonStyle.Default with {
            FontSize = 28
        };
        joinRoomButton.Label = "Join Room";
        joinRoomButton.Pressed += OnJoinClick;
        
        codeTextBox.Initialize();
        codeTextBox.Style = TextBoxStyle.Default with {
            FontSize = 24
        };
        codeTextBox.Placeholder = "Insert room code";
        codeTextBox.Keyboard = TextboxKeyboard.Numeric;
        codeTextBox.HorizontalFontAlignment = HorizontalAlign.Center;
        codeTextBox.VerticalFontAlignment = VerticalAlign.Center;
        
        emptyCodeFader.Initialize();
        emptyCodeFader.Content = emptyCodeBoard;
        emptyCodeBoard.Initialize();
        emptyCodeBoard.Type = TextBoardType.Error;
        emptyCodeBoard.FontSize = 24;
    }

    private void OnJoinClick() {
        if (!string.IsNullOrWhiteSpace(codeTextBox.GetString())) {
            TryJoinRoom();
            return;
        }
        emptyCodeBoard.Text = "Type room code first!";
        emptyCodeFader.ShowFor(3);
    }

    private void TryJoinRoom() {
        JoinRoomClicked?.Invoke(codeTextBox.GetString());
    }

    public void Render(RenderContext ctx) {
        Translation = Matrix4x4.Identity;
        createRoomButton.Size = new Vector2(300, 60);
        joinRoomButton.Size = new Vector2(300, 60);
        codeTextBox.Size = new Vector2(200, 40);
        emptyCodeFader.Size = new Vector2(300, 50);

        Vector2 mid = new(Size.X * 0.5f, Size.Y * 0.5f);
        const float buttonMargin = 15;
        createRoomButton.Position = new Vector2(
            mid.X - createRoomButton.Size.X*0.5f,
            mid.Y - buttonMargin - createRoomButton.Size.Y
        );
        joinRoomButton.Position = new Vector2(
            mid.X - joinRoomButton.Size.X*0.5f,
            mid.Y + buttonMargin
        );
        codeTextBox.Position = new Vector2(
            mid.X - codeTextBox.Size.X*0.5f,
            mid.Y + buttonMargin + joinRoomButton.Size.Y - 1
        );
        emptyCodeFader.Position = new Vector2(
            mid.X - emptyCodeFader.Size.X * 0.5f,
            codeTextBox.Position.Y + codeTextBox.Size.Y + buttonMargin
        );
        
        createRoomButton.Render(ctx);
        joinRoomButton.Render(ctx);
        codeTextBox.Render(ctx);
        emptyCodeFader.Render(ctx);
        
        // title
        AllegroFont font = FontManager.GetFont("ShareTech-Regular", 60);
        AllegroColor black = new() {
            R = 0.0f, G = 0.0f, B = 0.0f, A = 1.0f
        };
        ctx.UpdateTransform();
        Al.DrawText(font, black, (int)(Size.X*0.5f), (int)(Size.Y*0.25f - Al.GetFontLineHeight(font)*0.5f),
            FontAlignFlags.Center, "Insta Poker");
    }

    public void Update(double delta) {
        emptyCodeFader.Update(delta);
    }

    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Matrix4x4 Translation { get; set; }

    public void OnKeyDown(KeyCode key, uint modifiers) {
        codeTextBox.OnKeyDown(key,modifiers);
    }

    public void OnKeyUp(KeyCode key, uint modifiers) {
        codeTextBox.OnKeyUp(key,modifiers);
    }

    public void OnCharDown(char character) {
        codeTextBox.OnCharDown(character);
    }

    public void OnMouseMove(Vector2 pos, Vector2 delta)
    {
        pos = pos - new Vector2(Translation.Translation.X, Translation.Translation.Y);
        createRoomButton.OnMouseMove(pos, delta);
        joinRoomButton.OnMouseMove(pos, delta);
        codeTextBox.OnMouseMove(pos, delta);
    }

    public void OnMouseDown(uint button) {
        createRoomButton.OnMouseDown(button);
        joinRoomButton.OnMouseDown(button);
        codeTextBox.OnMouseDown(button);
    }

    public void OnMouseUp(uint button) {
        createRoomButton.OnMouseUp(button);
        joinRoomButton.OnMouseUp(button);
        codeTextBox.OnMouseUp(button);
    }

    public event Action? CreateRoomClicked;
    public event Action<string>? JoinRoomClicked;
}