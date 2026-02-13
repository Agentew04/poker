using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Objects;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Client.Network;
using InstaPoker.Core;
using InstaPoker.Core.Messages.Notifications;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game.Screens;

/// <summary>
/// Screen that renders a lobby with additional controls for admins, like lobby settings edition and the ability
/// to kick other players from the room. 
/// </summary>
public class AdminLobbyScreen() : SceneObject("Admin Lobby Screen") {
    
    public override bool UseMouse => true;
    public override bool UseKeyboard => true;

    private string code = string.Empty;
    private string title = string.Empty;
    private readonly List<LobbyUser> users = [];
    private readonly List<LobbyUser> usersToDelete = [];
    private readonly LoadingLabel loading = new(nameof(loading));
    private readonly Button startGameButton = new(nameof(startGameButton));
    private RoomSettings roomSettings = new();
    private readonly TextBox smallblindTextbox = new(nameof(smallblindTextbox));
    private readonly TextBox maxBetTextbox = new(nameof(maxBetTextbox));
    private readonly TextBox maxPlayersTextbox = new(nameof(maxBetTextbox));
    private readonly Checkbox allinEnabledCheckbox = new(nameof(allinEnabledCheckbox));


    public override void Initialize() {
        AddChild(loading);
        loading.Text = "Creating room";
        loading.FontSize = 28;
        
        AddChild(startGameButton);
        startGameButton.Text = "Start Game";
        startGameButton.Style = ButtonStyle.Default with {
            FontSize = 30
        };
        startGameButton.Pressed += () => GameStarted?.Invoke();
        
        AddChild(maxBetTextbox);
        AddChild(maxPlayersTextbox);
        AddChild(smallblindTextbox);
        AddChild(allinEnabledCheckbox);
        
        smallblindTextbox.Style = maxPlayersTextbox.Style = maxBetTextbox.Style = TextBoxStyle.Default with {
            FontSize = 20
        };
        smallblindTextbox.Keyboard = maxPlayersTextbox.Keyboard = maxBetTextbox.Keyboard = TextboxKeyboard.Numeric;
        smallblindTextbox.HorizontalTextAlignment = maxPlayersTextbox.HorizontalTextAlignment =
            maxBetTextbox.HorizontalTextAlignment = HorizontalAlign.Left;
        smallblindTextbox.VerticalTextAlignment = maxPlayersTextbox.VerticalTextAlignment =
            maxBetTextbox.VerticalTextAlignment = VerticalAlign.Center;
        
        smallblindTextbox.AutoTransform = false;
        maxBetTextbox.AutoTransform = false;
        maxPlayersTextbox.AutoTransform = false;
        allinEnabledCheckbox.AutoTransform = false;
        
        maxBetTextbox.MaxCharacters = 12;
        maxPlayersTextbox.MaxCharacters = 2;
        smallblindTextbox.MaxCharacters = 6;
        allinEnabledCheckbox.Style = CheckboxStyle.Default;
        

        allinEnabledCheckbox.OnValueChanged += _ => SendConfiguration();
        smallblindTextbox.TextChanged += _ => SendConfiguration();
        maxPlayersTextbox.TextChanged += _ => SendConfiguration();
        maxBetTextbox.TextChanged += _ => SendConfiguration();
        
        base.Initialize();
    }

    /// <summary>
    /// Function that is called when the screen is first shown to the user after clicking the button to
    /// create a room.
    /// </summary>
    public void OnShow() {
        loading.Show();
        Task<(string, RoomSettings)> createRoomTask = NetworkManager.CreateRoom();
        createRoomTask.ContinueWith(task => {
            users.Clear();
            users.Add(new LobbyUser() {
                Name = LocalSettings.Username,
                IsLocal = true,
                IsOwner = true
            });
            code = task.Result.Item1;
            title = "Room " + code;
            roomSettings = task.Result.Item2;
            allinEnabledCheckbox.Value = roomSettings.IsAllInEnabled;
            maxPlayersTextbox.SetString(roomSettings.MaxPlayers.ToString());
            smallblindTextbox.SetString(roomSettings.SmallBlind.ToString());
            maxBetTextbox.SetString(roomSettings.MaxBet.ToString());

            loading.Hide();
        });
    }

    /// <summary>
    /// Function called when the user was a normal player using <see cref="PlayerLobbyScreen"/> and became the
    /// new owner/admin.
    /// </summary>
    /// <param name="users">A list with the connected users</param>
    /// <param name="settings">The settings of the room</param>
    /// <param name="code">The code of the room</param>
    public void OnUpgrade(List<LobbyUser> users, RoomSettings settings, string code) {
        this.code = code;
        title = "Room " + code;
        this.users.Clear();
        this.users.AddRange(users);
        roomSettings.IsAllInEnabled = settings.IsAllInEnabled;
        roomSettings.MaxPlayers = settings.MaxPlayers;
        roomSettings.MaxBet = settings.MaxBet;
        roomSettings.SmallBlind = settings.SmallBlind;

        loading.ShowDots = false;
        loading.Text = "Becoming owner";
        loading.Show();
        allinEnabledCheckbox.Value = roomSettings.IsAllInEnabled;
        maxPlayersTextbox.SetString(roomSettings.MaxPlayers.ToString());
        smallblindTextbox.SetString(roomSettings.SmallBlind.ToString());
        maxBetTextbox.SetString(roomSettings.MaxBet.ToString());
        loading.Hide();
    }

    public override void PositionElements() {
        loading.Size = Size;
        loading.Position = Vector2.Zero;
        
        startGameButton.Size = new Vector2(300, 60);
        startGameButton.Position = new Vector2(
            Size.X * 0.5f - startGameButton.Size.X * 0.5f,
            Size.Y - Size.Y * 0.0625f - startGameButton.Size.Y * 0.5f
        );
        
        base.PositionElements();
    }

    public override void Render(RenderContext ctx) {
        loading.Render(ctx);
        if (loading.IsEnabled) {
            return;
        }

        ctx.UpdateTransform();

        // draw title
        AllegroFont titleFont = FontManager.GetFont("ShareTech-Regular", 36);
        Al.DrawText(titleFont, Colors.Black,
            (int)(Size.X * 0.5f), (int)(Size.Y * 0.0625 - Al.GetFontLineHeight(titleFont) * 0.5f),
            FontAlignFlags.Center, title);

        // draw player list
        RenderPlayerList(ctx);

        // draw options
        RenderRoomSettings(ctx);

        // draw start game
        base.Render(ctx);
    }

    private void RenderPlayerList(RenderContext ctx) {
        const float width = 500;
        // draw background
        Vector2 p1 = new(Size.X * 0.5f - width * 0.5f, Size.Y * 0.125f);
        Vector2 p2 = new(Size.X * 0.5f + width * 0.5f, Size.Y - Size.Y * 0.125f);
        ctx.UpdateTransform();
        Al.DrawFilledRectangle(p1.X, p1.Y, p2.X, p2.Y, Colors.BackgroundWhite);
        Al.DrawRectangle(p1.X, p1.Y, p2.X, p2.Y, Colors.Black, 1);
        Al.SetClippingRectangle((int)p1.X, (int)p1.Y, (int)(p2.X - p1.X), (int)(p2.Y - p1.Y));
        const float margin = 15;
        const float spacing = 5;

        AllegroFont font = FontManager.GetFont("ShareTech-Regular", 26);

        ctx.Stack.Push();
        ctx.Stack.Multiply(Matrix4x4.CreateTranslation(p1.X, p1.Y, 0));
        ctx.UpdateTransform();
        float y = margin + Al.GetFontLineHeight(font) * 0.5f;
        foreach (LobbyUser user in users) {
            float x = margin;
            Al.DrawText(font, Colors.Black, (int)x, (int)(y - Al.GetFontLineHeight(font) * 0.5f),
                FontAlignFlags.Left, user.Name);
            x += Al.GetTextWidth(font, user.Name);
            if (user.IsOwner) {
                Al.DrawText(font, Colors.Black, (int)x, (int)(y - Al.GetFontLineHeight(font) * 0.5f),
                    FontAlignFlags.Left, " (Owner)");
                x += Al.GetTextWidth(font, " (Owner)");
            }

            if (user.IsLocal) {
                Al.DrawText(font, Colors.Black, (int)x, (int)(y - Al.GetFontLineHeight(font) * 0.5f),
                    FontAlignFlags.Left, " (You)");
                x += Al.GetTextWidth(font, " (You)");
                // leave button
                if (user.Button is null) {
                    user.Button = new Button("leave button");
                    user.Button.Initialize(); // late initialize
                    user.Button.Text = "Leave";
                    user.Button.Pressed += OnUserLeave;
                    user.Button.Style = ButtonStyle.RedButton with {
                        FontSize = 24
                    };
                    AddChild(user.Button);
                }
                user.Button.Size = new Vector2(100, Al.GetFontLineHeight(font));
                user.Button.Position = new Vector2(p1.X + x + margin, p1.Y + y - Al.GetFontLineHeight(font) * 0.5f);
            }
            else {
                // kick button
                if (user.Button is null) {
                    user.Button = new Button("kick button");
                    user.Button.Initialize(); // late initialize
                    user.Button.Text = "Kick";
                    user.Button.Pressed += () => OnUserKick(user);
                    user.Button.Style = ButtonStyle.RedButton with {
                        FontSize = 24
                    };
                    AddChild(user.Button);
                }
                user.Button.Size = new Vector2(100, Al.GetFontLineHeight(font));
                user.Button.Position = new Vector2(p1.X + x + margin, p1.Y + y - Al.GetFontLineHeight(font) * 0.5f);
            }

            y += Al.GetFontLineHeight(font) + spacing;
        }

        ctx.Stack.Pop();

        Al.ResetClippingRectangle();
    }

    private void RenderRoomSettings(RenderContext ctx) {
        ctx.UpdateTransform();
        Vector2 p1 = new(Size.X * 0.5f - 500 * 0.5f, Size.Y * 0.125f);
        Al.SetClippingRectangle(0, 0, (int)p1.X, (int)Size.Y);

        const float spacing = 10;
        const float margin = 15;
        var font = FontManager.GetFont("ShareTech-Regular", 28);
        float itemHeight = Al.GetFontLineHeight(font);

        const int itemCount = 4; // TODO: trocar para 4 e add allowAllIn
        float totalHeight = itemHeight * itemCount + spacing * (itemCount - 1);
        ctx.Stack.Push();
        ctx.Stack.Multiply(Matrix4x4.CreateTranslation(margin, Size.Y * 0.5f - totalHeight * 0.5f, 0));
        ctx.UpdateTransform();

        float targetWidth = p1.X - 2 * margin;


        smallblindTextbox.Position = new Vector2(Al.GetTextWidth(font, "Small Blind: "), GetHeight(0));
        maxBetTextbox.Position = new Vector2(Al.GetTextWidth(font, "Max Bet: "), GetHeight(1));
        maxPlayersTextbox.Position = new Vector2(Al.GetTextWidth(font, "Max Players: "), GetHeight(2));
        allinEnabledCheckbox.Position = new Vector2(
            Al.GetTextWidth(font, "All-In Enabled:") + margin * 0.5f,
            GetHeight(3) + itemHeight * 0.125f);

        smallblindTextbox.Size = new Vector2(targetWidth - smallblindTextbox.Position.X, itemHeight);
        maxBetTextbox.Size = new Vector2(targetWidth - maxBetTextbox.Position.X, itemHeight);
        maxPlayersTextbox.Size = new Vector2(targetWidth - maxPlayersTextbox.Position.X, itemHeight);
        allinEnabledCheckbox.Size = new Vector2(itemHeight * 0.75f, itemHeight * 0.75f);

        Al.DrawText(font, Colors.Black, 0, GetHeight(0), FontAlignFlags.Left, "Small Blind: ");
        Al.DrawText(font, Colors.Black, 0, GetHeight(1), FontAlignFlags.Left, "Max Bet: ");
        Al.DrawText(font, Colors.Black, 0, GetHeight(2), FontAlignFlags.Left, "Max Players: ");
        Al.DrawText(font, Colors.Black, 0, GetHeight(3), FontAlignFlags.Left, "All-In Enabled: ");

        Matrix4x4 transform = ctx.Stack.Peek();
        smallblindTextbox.Transform = transform;
        maxBetTextbox.Transform = transform;
        maxPlayersTextbox.Transform = transform;
        allinEnabledCheckbox.Transform = transform;

        ctx.Stack.Pop();
        Al.ResetClippingRectangle();

        return;

        float GetHeight(int i) {
            return itemHeight * i + spacing * (i - 1);
        }
    }

    private void OnUserKick(LobbyUser user) {
        usersToDelete.Add(user);
        _ = NetworkManager.KickUser(user.Name);
    }

    private void OnUserLeave() {
        NetworkManager.LeaveRoom();
        UserLeft?.Invoke();
    }

    private void SendConfiguration() {
        if (loading.IsEnabled) {
            return;
        }

        roomSettings.MaxPlayers = !string.IsNullOrEmpty(maxPlayersTextbox.GetString())
            ? int.Parse(maxPlayersTextbox.GetString())
            : 0;
        roomSettings.SmallBlind = !string.IsNullOrEmpty(smallblindTextbox.GetString())
            ? int.Parse(smallblindTextbox.GetString())
            : 0;
        roomSettings.MaxBet = !string.IsNullOrEmpty(maxBetTextbox.GetString())
            ? int.Parse(maxBetTextbox.GetString())
            : 0;
        roomSettings.IsAllInEnabled = allinEnabledCheckbox.Value;

        Console.WriteLine("Sending Configuration");
        _ = NetworkManager.SendSettings(roomSettings);
    }

    public override void Update(double delta) {
        loading.Update(delta);
        if (loading.IsEnabled) {
            return;
        }

        if (NetworkManager.Handler!.TryGetPendingMessage(out RoomListUpdatedNotification? listUpdate)) {
            if (listUpdate.UpdateType is LobbyListUpdateType.UserLeft or LobbyListUpdateType.UserKicked) {
                users.RemoveAll(x => x.Name == listUpdate.Username);
            }
            else if (listUpdate.UpdateType == LobbyListUpdateType.UserJoined) {
                users.Add(new LobbyUser() {
                    Name = listUpdate.Username,
                    IsLocal = false,
                    IsOwner = false,
                });
            }
        }

        // ignore updates from the server. we are admin and we are the source of truth (hopefully)
        NetworkManager.Handler!.TryGetPendingMessage(out RoomSettingsChangeNotification? _);
        
        base.Update(delta);
    }

    public override void OnMouseUp(MouseButton button) {
        // remove users
        foreach (LobbyUser user in usersToDelete) {
            users.Remove(user);
            if (user.Button is not null) {
                RemoveChild(user.Button);
            }
        }

        usersToDelete.Clear();
        base.OnMouseUp(button);
    }
    
    /// <summary>
    /// Event called when the user signals the game to start.
    /// </summary>
    public event Action? GameStarted;

    /// <summary>
    /// Event called when the user leaves the room and returns to the main menu.
    /// </summary>
    public event Action? UserLeft;
}