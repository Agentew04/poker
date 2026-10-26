using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Layout;
using InstaPoker.Client.Graphics.Objects;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Client.Subsystems;
using InstaPoker.Client.Subsystems.Account;
using InstaPoker.Client.Subsystems.Network;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game.Screens;

/// <summary>
/// Represents the Main menu screen of the game. Is a general hub for other screens. 
/// </summary>
public class MainMenuScreen() : SceneObject("Main Menu Screen"), IScreen<Screen> {

    public Screen Key => Screen.MainMenu;
    
    public override bool UseMouse => true;
    public override bool UseKeyboard => true;

    [ChildObject] private readonly Label titleLabel = new(nameof(titleLabel));
    [ChildObject] private readonly Image titleImage = new(nameof(titleImage));
    [ChildObject] private readonly Button createRoomButton = new(nameof(createRoomButton));
    [ChildObject] private readonly Button joinRoomButton = new(nameof(joinRoomButton));
    [ChildObject] private readonly TextBox codeTextBox = new(nameof(codeTextBox));
    [ChildObject] private readonly Fader emptyCodeFader = new(nameof(emptyCodeFader));
    [ChildObject] private readonly Button logoutButton = new(nameof(logoutButton));
    [ChildObject] private readonly Label usernameLabel = new(nameof(usernameLabel));
    private readonly Toast emptyCodeBoard = new(nameof(emptyCodeBoard));

    [ChildObject] private readonly StackPanel panel = new(nameof(panel));
    private readonly Label l1 = new(nameof(l1));
    private readonly Button but = new(nameof(but));
    private readonly Label l2 = new(nameof(l2));

    [Service] private AccountStore accountStore = null!;
    [Service] private NetworkManager networkManager = null!;

    public override void Initialize() {
        createRoomButton.Style = ButtonStyle.Default with {
            FontSize = 28
        };
        createRoomButton.Text = "Create Room";
        createRoomButton.Pressed += () => CreateRoomClicked?.Invoke();
        
        joinRoomButton.Style = ButtonStyle.Default with {
            FontSize = 28
        };
        joinRoomButton.Text = "Join Room";
        joinRoomButton.Pressed += OnJoinClick;
        
        codeTextBox.Style = TextBoxStyle.Default with {
            FontSize = 24
        };
        codeTextBox.Placeholder = "Insert room code";
        codeTextBox.Keyboard = TextboxKeyboard.Numeric;
        codeTextBox.HorizontalTextAlignment = HorizontalAlign.Center;
        codeTextBox.VerticalTextAlignment = VerticalAlign.Center;
        codeTextBox.EnterButton = joinRoomButton;
        
        emptyCodeFader.Content = emptyCodeBoard;
        emptyCodeBoard.Type = TextBoardType.Error;
        emptyCodeBoard.FontSize = 24;
        
        titleLabel.FontSize = 60;
        titleLabel.Text = "Insta Poker";
        
        titleImage.ImageName = "logo";
        titleImage.HorizontalAlign = HorizontalAlign.Right;
        titleImage.VerticalAlign = VerticalAlign.Center;

        logoutButton.Style = ButtonStyle.RedButton with {
            FontSize = 22
        };
        logoutButton.Text = "Logout";
        logoutButton.Pressed += Logout;

        usernameLabel.VerticalAlign = VerticalAlign.Top;
        usernameLabel.HorizontalAlign = HorizontalAlign.Left;
        
        l1.Text = "Teste1";
        panel.Add(l1);
        but.Text = "AA";
        but.Style = ButtonStyle.Default;
        panel.Add(but);
        l2.Text = "Teste2";
        panel.Add(l2);
        
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

    public void OnShow() {
        usernameLabel.Text = "Logged in as " + LocalSettings.Username;
    }

    private void Logout() {
        accountStore.ClearLogin();
        networkManager.Disconnect();
        LoggedOut?.Invoke();
    }

    private void TryJoinRoom() {
        JoinRoomClicked?.Invoke(codeTextBox.GetString());
    }

    public override void PositionElements() {
        Vector2 mid = new(Size.X * 0.5f, Size.Y * 0.5f);
        const float buttonMargin = 15;

        createRoomButton.Place().WithSize(300, 60)
            .At(0, mid.Y - buttonMargin - 60)
            .CenterHorizontallyIn(this);
        joinRoomButton.Place().WithSize(300, 60)
            .At(0, mid.Y + buttonMargin)
            .CenterHorizontallyIn(this);
        codeTextBox.Place().WithSize(200, 40)
            .Below(joinRoomButton, -1)
            .CenterHorizontallyIn(this);
        emptyCodeFader.Place().WithSize(300, 50)
            .Below(codeTextBox, buttonMargin)
            .CenterHorizontallyIn(this);

        titleLabel.Place().AtAnchor(Size.X * 0.5f, Size.Y * 0.25f);
        titleLabel.Measure();

        titleImage.Place().WithSize(128, 128)
            .AtAnchor(titleLabel.Bounds.Left - 15, Size.Y * 0.25f);

        logoutButton.Place().WithSize(80, 40).At(5, 5);
        usernameLabel.Place().AtAnchor(
            logoutButton.Bounds.Right + 5,
            5 + logoutButton.Bounds.Size.Y * 0.5f - usernameLabel.FontSize * 0.5f);

        l1.HorizontalAlign = HorizontalAlign.Center;
        l1.VerticalAlign = VerticalAlign.Top;
        but.VerticalAlign = VerticalAlign.Top;
        l2.VerticalAlign = VerticalAlign.Top;
        panel.Position = new Vector2(100,100);
        panel.Padding = 10;
        panel.Spacing = 5;
        panel.Size = new Vector2(300, Size.Y-200);
        panel.CrossAxisAlignment = CrossAxisAlignment.Start;
        panel.ChildAlignment = ChildAlignment.Pivot;
        panel.Direction = StackDirection.Horizontal;
        but.HorizontalAlign = HorizontalAlign.Center;
        
        but.Size = new Vector2(50, 100);

        base.PositionElements();
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

    public event Action? LoggedOut;
}