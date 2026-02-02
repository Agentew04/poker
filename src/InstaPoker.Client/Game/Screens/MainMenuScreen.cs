using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game.Screens;

/// <summary>
/// Represents the Main menu screen of the game. Is a general hub for other screens. 
/// </summary>
public class MainMenuScreen() : SceneObject("Main Menu Screen") {

    public override bool UseMouse => true;
    public override bool UseKeyboard => true;

    private readonly Label titleLabel = new(nameof(titleLabel));
    private readonly Button createRoomButton = new(nameof(createRoomButton));
    private readonly Button joinRoomButton = new(nameof(joinRoomButton));
    private readonly TextBox codeTextBox = new(nameof(codeTextBox));
    private readonly Fader emptyCodeFader = new(nameof(emptyCodeFader));
    private readonly Toast emptyCodeBoard = new(nameof(emptyCodeBoard));

    public override void Initialize() {
        AddChild(createRoomButton);
        createRoomButton.Style = ButtonStyle.Default with {
            FontSize = 28
        };
        createRoomButton.Label = "Create Room";
        createRoomButton.Pressed += () => CreateRoomClicked?.Invoke();
        
        AddChild(joinRoomButton);
        joinRoomButton.Style = ButtonStyle.Default with {
            FontSize = 28
        };
        joinRoomButton.Label = "Join Room";
        joinRoomButton.Pressed += OnJoinClick;
        
        AddChild(codeTextBox);
        codeTextBox.Style = TextBoxStyle.Default with {
            FontSize = 24
        };
        codeTextBox.Placeholder = "Insert room code";
        codeTextBox.Keyboard = TextboxKeyboard.Numeric;
        codeTextBox.HorizontalFontAlignment = HorizontalAlign.Center;
        codeTextBox.VerticalFontAlignment = VerticalAlign.Center;
        
        AddChild(emptyCodeFader);
        emptyCodeFader.Content = emptyCodeBoard;
        emptyCodeBoard.Type = TextBoardType.Error;
        emptyCodeBoard.FontSize = 24;
        
        AddChild(titleLabel);
        titleLabel.FontSize = 60;
        titleLabel.Text = "Insta Poker";
        
        base.Initialize();
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

    public override void PositionElements() {
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

        titleLabel.Position = new Vector2(Size.X * 0.5f, Size.Y * 0.25f);
        
        base.PositionElements();
    }

    public override void Render(RenderContext ctx) {
        base.Render(ctx);
    }

    /// <summary>
    /// Event fired when the user clicks on the 'Create Room' button.
    /// </summary>
    public event Action? CreateRoomClicked;
    
    /// <summary>
    /// Event fired when the user clicks to join an existing room. The code that the user typed
    /// is passed as the sole parameter.
    /// </summary>
    public event Action<string>? JoinRoomClicked;
}