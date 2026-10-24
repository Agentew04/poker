using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Objects;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Client.Subsystems;
using InstaPoker.Client.Subsystems.Account;
using InstaPoker.Client.Subsystems.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InstaPoker.Client.Game.Screens;

/// <summary>
/// First screen the user sees when the game launches. Prompts the user to enter their username
/// and after that, establishes a connection to the remote game server.
/// </summary>
/// <remarks>
/// If for some reason the connection to the remote server fails, the client automatically tries again.
/// There is no limit of retries.
/// </remarks>
public class IdentifyScreen() : SceneObject("Identify Screen"), IScreen<Screen> {
    
    public Screen Key => Screen.Identify;
    
    public override bool UseMouse => true;
    public override bool UseKeyboard => true;

    [ChildObject] private readonly Label whatsyournameLabel = new(nameof(whatsyournameLabel));
    [ChildObject] private readonly TextBox nameTextBox = new(nameof(nameTextBox));
    [ChildObject] private readonly Button okButton = new(nameof(okButton));
    [ChildObject] private readonly Fader emptyNameFader = new(nameof(emptyNameFader));
    private readonly Toast emptyNameBoard = new(nameof(emptyNameBoard));
    [ChildObject] private readonly LoadingLabel loading = new(nameof(loading));

    [Service] private AccountStore accountStore = null!;
    [Service] private NetworkManager networkManager = null!;

    private bool connectionStarted;

    public override void Initialize() {
        accountStore = Service.Provider.GetRequiredService<AccountStore>();
        networkManager = Service.Provider.GetRequiredService<NetworkManager>();
        
        loading.FontSize = 28;
        loading.Hide();
        
        nameTextBox.Style = TextBoxStyle.Default with {
            FontSize = 28
        };
        nameTextBox.Placeholder = "Insert your nickname";
        nameTextBox.HorizontalTextAlignment = HorizontalAlign.Center;
        nameTextBox.VerticalTextAlignment = VerticalAlign.Center;
        nameTextBox.MaxCharacters = 20;
        nameTextBox.Keyboard = TextboxKeyboard.AlphaNumeric;
        
        okButton.Style = ButtonStyle.Default with {
            FontSize = 28
        };
        nameTextBox.EnterButton = okButton;
        okButton.Text = "OK";
        okButton.Pressed += OnOkClick;
        
        emptyNameFader.Content = emptyNameBoard;
        emptyNameBoard.Text = "Nickname cannot be empty!";
        emptyNameBoard.FontSize = 24;
        emptyNameBoard.Type = TextBoardType.Error;
        
        whatsyournameLabel.FontSize = 32;
        whatsyournameLabel.Text = "Hello! What's your name?";
        
        base.Initialize();
    }

    private void OnOkClick() {
        if (!string.IsNullOrWhiteSpace(nameTextBox.GetString())) {
            LocalSettings.Username = nameTextBox.GetString();
            accountStore.SaveLogin(new Login(nameTextBox.GetString(), string.Empty));
            connectionStarted = true;
            _ = ConnectAsync();
            return;
        }
        emptyNameFader.ShowFor(3);
    }

    private async Task ConnectAsync() {
        while (true) {
            ConnectionResult result = await TryConnection();
            switch (result) {
                case ConnectionResult.Success:
                    loading.Hide();
                    OkClicked?.Invoke();
                    return;

                case ConnectionResult.CertificateMismatch:
                    // retrying cannot help: this client is pinned to a different server key
                    loading.ShowDots = false;
                    loading.Text = "This server's certificate has changed. Please update your client to connect.";
                    loading.Show();
                    Logger.LogError("Connection aborted: the server certificate does not match this client. Update the client.");
                    return;

                default:
                    loading.Text = "Could not connect to remote server. Retrying in 5 seconds";
                    loading.ShowDots = false;
                    loading.Show();
                    await Task.Delay(5000);
                    break;
            }
        }
    }

    private Task<ConnectionResult> TryConnection() {
        loading.ShowDots = true;
        loading.Text = "Connecting to server";
        loading.Show();
        Logger.LogInformation("Trying to connect to remote server");
        return networkManager.ConnectToServer(LocalSettings.Username);
    }

    public override void PositionElements() {
        loading.Size = Size;
        loading.Position = Vector2.Zero;

        const float margin = 15;

        nameTextBox.Place().WithSize(500, 40)
            .CenterIn(this);
        okButton.Place().WithSize(300, 60)
            .CenterHorizontallyIn(this)
            .Below(nameTextBox, margin);
        emptyNameFader.Place()
            .At(0, Size.Y * 0.5f + okButton.Size.Y + 3 * margin)
            .WithSize(300, 50)
            .CenterHorizontallyIn(this);
            
            
        // nameTextBox.Size = new Vector2(500, 40);
        // okButton.Size = new Vector2(300, 60);
        // emptyNameFader.Size = new Vector2(300, 50);
        
        // nameTextBox.Position = new Vector2(
        //     Size.X*0.5f - nameTextBox.Size.X * 0.5f,
        //     Size.Y*0.5f - nameTextBox.Size.Y * 0.5f);
        // okButton.Position = new Vector2(
        //     Size.X*0.5f - okButton.Size.X * 0.5f,
        //     Size.Y*0.5f + nameTextBox.Size.Y * 0.5f + margin
        // );
        // emptyNameFader.Position = new Vector2(
        //     Size.X*0.5f - emptyNameFader.Size.X * 0.5f,
        //     Size.Y*0.5f + okButton.Size.Y + 3*margin
        // );

        whatsyournameLabel.Position = new Vector2(Size.X*0.5f, Size.Y*0.25f);
        
        base.PositionElements();
    }

    public override void Update(double delta) {
        Login? login = accountStore.GetSavedLogin();
        if (login != null && !connectionStarted) {
            // there is a saved account. we can skip
            Logger.LogInformation("Using saved login data");
            nameTextBox.SetString(login.Username);
            OnOkClick();
        }

        foreach (SceneObject child in GetChildren()) {
            if (child == loading) {
                continue;
            }
            child.IsEnabled = !loading.IsEnabled;
        }
        base.Update(delta);
    }

    public override void Render(RenderContext ctx) {
        loading.Render(ctx);
        if (loading.IsEnabled) {
            return;
        }
        
        ctx.UpdateTransform();
        
        base.Render(ctx);
    }

    /// <summary>
    /// Event fired when the user clicks on the Ok button. <see cref="IdentifyScreen"/> performs internal validation
    /// and only fires this event when the username is valid.
    /// </summary>
    public event Action? OkClicked;

    public void OnShow() {
        // empty
    }
}