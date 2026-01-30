using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Client.Network;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game;

/// <summary>
/// First screen the user sees when the game launches. Prompts the user to enter their username
/// and after that, establishes a connection to the remote game server.
/// </summary>
/// <remarks>
/// If for some reason the connection to the remote server fails, the client automatically tries again.
/// There is no limit of retries.
/// </remarks>
public class IdentifyScreen : IRenderObject, IMouseInteractable, IKeyboardInteractable {

    private readonly TextBox nameTextBox = new();
    private readonly Button okButton = new();
    private readonly Fader emptyNameFader = new();
    private readonly TextBoard emptyNameBoard = new();
    private readonly LoadingLabel loading = new();

    private Task<bool>? connectionTask;
    
    public void Initialize() {
        loading.Initialize();
        loading.FontSize = 28;
        loading.Hide();
        
        nameTextBox.Initialize();
        nameTextBox.Style = TextBoxStyle.Default with {
            FontSize = 28
        };
        nameTextBox.Placeholder = "Insert your nickname";
        nameTextBox.HorizontalFontAlignment = HorizontalAlign.Center;
        nameTextBox.VerticalFontAlignment = VerticalAlign.Center;
        nameTextBox.MaxCharacters = 20;
        nameTextBox.Keyboard = TextboxKeyboard.AlphaNumeric;
        
        okButton.Initialize();
        okButton.Style = ButtonStyle.Default with {
            FontSize = 28
        };
        okButton.Label = "OK";
        okButton.Pressed += OnOkClick;
        
        emptyNameFader.Initialize();
        emptyNameFader.Content = emptyNameBoard;
        emptyNameBoard.Initialize();
        emptyNameBoard.Text = "Nickname cannot be empty!";
        emptyNameBoard.FontSize = 24;
        emptyNameBoard.Type = TextBoardType.Error;
    }

    private void OnOkClick() {
        if (!string.IsNullOrWhiteSpace(nameTextBox.GetString())) {
            LocalSettings.Username = nameTextBox.GetString();

            
            connectionTask = TryConnection();
            connectionTask.ContinueWith(OnConnectEnd);
            return;
        }
        emptyNameFader.ShowFor(3);
    }

    private Task<bool> TryConnection() {
        loading.ShowDots = true;
        loading.Text = "Connecting to server";
        loading.Show();
        return NetworkManager.ConnectToServer(LocalSettings.Username);
    }
    
    private void OnConnectEnd(Task<bool> task) {
        if (task.Result) {
            loading.Hide();
            OkClicked?.Invoke();
        }
        else {
            loading.Text = "Could not connect to remote server. Retrying in 5 seconds";
            loading.ShowDots = false;
            loading.Show();
            Task.Delay(5000).ContinueWith(_ => {
                connectionTask = TryConnection();
                connectionTask.ContinueWith(OnConnectEnd);
            });
        }
    }

    public void Render(RenderContext ctx) {
        loading.Size = Size;
        loading.Position = Vector2.Zero;
        loading.Render(ctx);
        if (loading.IsEnabled) {
            return;
        }
        
        Translation = Matrix4x4.Identity;
        nameTextBox.Size = new Vector2(500, 40);
        okButton.Size = new Vector2(300, 60);
        emptyNameFader.Size = new Vector2(300, 50);

        const float margin = 15;
        nameTextBox.Position = new Vector2(
            Size.X*0.5f - nameTextBox.Size.X * 0.5f,
            Size.Y*0.5f - nameTextBox.Size.Y * 0.5f);
        okButton.Position = new Vector2(
            Size.X*0.5f - okButton.Size.X * 0.5f,
            Size.Y*0.5f + nameTextBox.Size.Y * 0.5f + margin
        );
        emptyNameFader.Position = new Vector2(
            Size.X*0.5f - emptyNameFader.Size.X * 0.5f,
            Size.Y*0.5f + okButton.Size.Y + 3*margin
        );
        
        nameTextBox.Render(ctx);
        okButton.Render(ctx);
        emptyNameFader.Render(ctx);

        ctx.UpdateTransform();
        AllegroFont font = FontManager.GetFont("ShareTech-Regular", 32);
        Al.DrawText(font, Colors.Black, (int)(Size.X*0.5f), (int)(Size.Y*0.25f),
            FontAlignFlags.Center, "Hello! What's your name?");
        
        renderer.Style = CardStyle.Default;
        renderer.CardSize = new Vector2(63.5f,88.9f)*4;
        renderer.RenderContext = ctx;
        if (Al.GetTime() > turnStart + turnTime && isTurning) {
            Console.WriteLine("End anim");
            isTurning = false;
            isDown = !isDown;
        }

        // }else if(isTurning && Al.GetTime() <= turnStart + turnTime) Console.WriteLine((float)((Al.GetTime() - turnStart)/turnTime));
        renderer.RenderAt(new GameCard(7, Suit.Clubs), new Vector2(250,250),isDown, !isTurning 
                ? 0
                : (float)((Al.GetTime() - turnStart)/turnTime)
            , isHover ? MathF.PI*0.25f : 0);
    }

    private CardRenderer renderer = new();
    
    private bool isTurning = false;
    private double turnTime = 0.5f;
    private double turnStart = 0;
    private bool isDown = true;
    private bool isHover;
    
    public void Update(double delta) {
        emptyNameFader.Update(delta);
    }

    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Matrix4x4 Translation { get; set; }

    public void OnMouseMove(Vector2 pos, Vector2 delta) {
        pos = pos - new Vector2(Translation.Translation.X, Translation.Translation.Y);
        isHover = pos.X >= 250 - renderer.CardSize.X * 0.5f
                  && pos.X <= 250 + renderer.CardSize.X * 0.5f
                  && pos.Y >= 250 - renderer.CardSize.Y * 0.5f
                  && pos.Y <= 250 + renderer.CardSize.Y * 0.5f;
            
        nameTextBox.OnMouseMove(pos,delta);
        okButton.OnMouseMove(pos,delta);
    }

    public void OnMouseDown(MouseButton button) {
        if (button == MouseButton.Left) {
            // isTurning = true;
            // turnStart = Al.GetTime();
        }
        nameTextBox.OnMouseDown(button);
        okButton.OnMouseDown(button);
    }

    public void OnMouseUp(MouseButton button) {
        nameTextBox.OnMouseUp(button);
        okButton.OnMouseUp(button);
    }

    public void OnKeyDown(KeyCode key, KeyModifiers modifiers) {
        nameTextBox.OnKeyDown(key,modifiers);
    }

    public void OnKeyUp(KeyCode key, KeyModifiers modifiers) {
        nameTextBox.OnKeyUp(key,modifiers);
    }

    public void OnCharDown(char character) {
        nameTextBox.OnCharDown(character);
    }

    /// <summary>
    /// Event fired when the user clicks on the Ok button. <see cref="IdentifyScreen"/> performs internal validation
    /// and only fires this event when the username is valid.
    /// </summary>
    public event Action? OkClicked;
}