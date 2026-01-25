using System.Net.NetworkInformation;
using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Client.Network;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game;

public class IdentifyScreen : IRenderObject, IMouseInteractable, IKeyboardInteractable {

    private readonly TextBox nameTextBox = new();
    private readonly Button okButton = new();
    private readonly Fader emptyNameFader = new();
    private readonly TextBoard emptyNameBoard = new();
    private readonly LoadingLabel loading = new();
    
    public void Initialize() {
        loading.Initialize();
        loading.Text = "Connecting to server";
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

            loading.Show();
            Task connectTask = NetworkManager.ConnectToServer(LocalSettings.Username);
            connectTask.ContinueWith((t) => {
                loading.Hide();
                OkClicked?.Invoke();
            });
            return;
        }
        emptyNameFader.ShowFor(3);
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
        Al.DrawText(font, AllegroColor.Black, (int)(Size.X*0.5f), (int)(Size.Y*0.25f),
            FontAlignFlags.Center, "Hello! What's your name?");
    }

    public void Update(double delta) {
        emptyNameFader.Update(delta);
    }

    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Matrix4x4 Translation { get; set; }

    public void OnMouseMove(Vector2 pos, Vector2 delta) {
        pos = pos - new Vector2(Translation.Translation.X, Translation.Translation.Y);
        nameTextBox.OnMouseMove(pos,delta);
        okButton.OnMouseMove(pos,delta);
    }

    public void OnMouseDown(uint button) {
        nameTextBox.OnMouseDown(button);
        okButton.OnMouseDown(button);
    }

    public void OnMouseUp(uint button) {
        nameTextBox.OnMouseUp(button);
        okButton.OnMouseUp(button);
    }

    public void OnKeyDown(KeyCode key, uint modifiers) {
        nameTextBox.OnKeyDown(key,modifiers);
    }

    public void OnKeyUp(KeyCode key, uint modifiers) {
        nameTextBox.OnKeyUp(key,modifiers);
    }

    public void OnCharDown(char character) {
        nameTextBox.OnCharDown(character);
    }

    public event Action? OkClicked;
}