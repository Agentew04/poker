using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Client.Network;
using InstaPoker.Core;
using InstaPoker.Core.Messages.Notifications;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game;

/// <summary>
/// Screen that renders a lobby with additional controls for admins, like lobby settings edition and the ability
/// to kick other players from the room. 
/// </summary>
public class AdminLobbyScreen : IRenderObject, IMouseInteractable, IKeyboardInteractable
{
    private string code;
    private string title;
    private readonly List<LobbyUser> users = [];
    private readonly List<LobbyUser> usersToDelete = [];
    private readonly LoadingLabel loading = new();
    private readonly Button startGameButton = new();
    private RoomSettings roomSettings = new();
    private readonly TextBox smallblindTextbox = new();
    private readonly TextBox maxBetTextbox = new();
    private readonly TextBox maxPlayersTextbox = new();
    private readonly Checkbox allinEnabledCheckbox = new();

    public void Initialize()
    {
        loading.Initialize();
        loading.Text = "Creating room";
        loading.FontSize = 28;
        startGameButton.Initialize();
        startGameButton.Label = "Start Game";
        startGameButton.Style = ButtonStyle.Default with
        {
            FontSize = 30
        };
        startGameButton.Pressed += () => GameStarted?.Invoke();
        maxBetTextbox.Initialize();
        maxPlayersTextbox.Initialize();
        smallblindTextbox.Initialize();
        smallblindTextbox.Style = maxPlayersTextbox.Style = maxBetTextbox.Style = TextBoxStyle.Default with {
            FontSize = 20
        };
        smallblindTextbox.Keyboard = maxPlayersTextbox.Keyboard = maxBetTextbox.Keyboard = TextboxKeyboard.Numeric;
        smallblindTextbox.HorizontalFontAlignment = maxPlayersTextbox.HorizontalFontAlignment = maxBetTextbox.HorizontalFontAlignment = HorizontalAlign.Left;
        smallblindTextbox.VerticalFontAlignment = maxPlayersTextbox.VerticalFontAlignment = maxBetTextbox.VerticalFontAlignment = VerticalAlign.Center;
        maxBetTextbox.MaxCharacters = 12;
        maxPlayersTextbox.MaxCharacters = 2;
        smallblindTextbox.MaxCharacters = 6;
        allinEnabledCheckbox.Initialize();
        allinEnabledCheckbox.Style = CheckboxStyle.Default;
        
        allinEnabledCheckbox.OnValueChanged += _ => SendConfiguration();
        smallblindTextbox.TextChanged += _ => SendConfiguration();
        maxPlayersTextbox.TextChanged += _ => SendConfiguration();
        maxBetTextbox.TextChanged += _ => SendConfiguration();
    }

    /// <summary>
    /// Function that is called when the screen is first shown to the user after clicking the button to
    /// create a room.
    /// </summary>
    public void OnShow()
    {
        loading.Show();
        Task<(string,RoomSettings)> createRoomTask = NetworkManager.CreateRoom();
        createRoomTask.ContinueWith(task =>
        {
            users.Clear();
            users.Add(new LobbyUser()
            {
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
    
    public void Render(RenderContext ctx)
    {
        loading.Size = Size;
        loading.Position = Vector2.Zero;
        loading.Render(ctx);
        if (loading.IsEnabled)
        {
            return;
        }

        Translation = Matrix4x4.Identity;
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
        startGameButton.Size = new Vector2(300, 60);
        startGameButton.Position = new Vector2(
            Size.X * 0.5f - startGameButton.Size.X * 0.5f,
            Size.Y - Size.Y * 0.0625f - startGameButton.Size.Y * 0.5f
        );
        startGameButton.Render(ctx);
    }

    private void RenderPlayerList(RenderContext ctx)
    {
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
        foreach (LobbyUser user in users)
        {
            float x = margin;
            Al.DrawText(font, Colors.Black, (int)x, (int)(y - Al.GetFontLineHeight(font) * 0.5f),
                FontAlignFlags.Left, user.Name);
            x += Al.GetTextWidth(font, user.Name);
            if (user.IsOwner)
            {
                Al.DrawText(font, Colors.Black, (int)x, (int)(y - Al.GetFontLineHeight(font) * 0.5f),
                    FontAlignFlags.Left, " (Owner)");
                x += Al.GetTextWidth(font, " (Owner)");
            }

            if (user.IsLocal)
            {
                Al.DrawText(font, Colors.Black, (int)x, (int)(y - Al.GetFontLineHeight(font) * 0.5f),
                    FontAlignFlags.Left, " (You)");
                x += Al.GetTextWidth(font, " (You)");
                // leave button
                if (user.Button is null)
                {
                    user.Button = new Button();
                    user.Button.Initialize();
                    user.Button.Label = "Leave";
                    user.Button.Pressed += OnUserLeave;
                    user.Button.Style = ButtonStyle.RedButton with
                    {
                        FontSize = 24
                    };
                }

                user.Button.Size = new Vector2(100, Al.GetFontLineHeight(font));
                user.Button.Position = new Vector2(p1.X + x + margin, p1.Y + y - Al.GetFontLineHeight(font) * 0.5f);
            }
            else
            {
                // kick button
                if (user.Button is null)
                {
                    user.Button = new Button();
                    user.Button.Initialize();
                    user.Button.Label = "Kick";
                    user.Button.Pressed += () => OnUserKick(user);
                    user.Button.Style = ButtonStyle.RedButton with
                    {
                        FontSize = 24
                    };
                }

                user.Button.Size = new Vector2(100, Al.GetFontLineHeight(font));
                user.Button.Position = new Vector2(p1.X + x + margin, p1.Y + y - Al.GetFontLineHeight(font) * 0.5f);
            }

            y += Al.GetFontLineHeight(font) + spacing;
        }

        ctx.Stack.Pop();

        foreach (LobbyUser user in users)
        {
            user.Button?.Render(ctx);
        }

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
        float totalHeight = itemHeight * itemCount + spacing*(itemCount-1);
        ctx.Stack.Push();
        ctx.Stack.Multiply(Matrix4x4.CreateTranslation(margin, Size.Y*0.5f - totalHeight*0.5f, 0));
        ctx.UpdateTransform();

        float targetWidth = p1.X - 2*margin;

        
        smallblindTextbox.Position = new Vector2(Al.GetTextWidth(font, "Small Blind: "), GetHeight(0));
        maxBetTextbox.Position = new Vector2(Al.GetTextWidth(font, "Max Bet: "), GetHeight(1));
        maxPlayersTextbox.Position = new Vector2(Al.GetTextWidth(font, "Max Players: "), GetHeight(2));
        allinEnabledCheckbox.Position = new Vector2(
            Al.GetTextWidth(font, "All-In Enabled:")+margin*0.5f, 
            GetHeight(3) + itemHeight * 0.125f);
        
        smallblindTextbox.Size = new Vector2(targetWidth-smallblindTextbox.Position.X , itemHeight);
        maxBetTextbox.Size = new Vector2(targetWidth-maxBetTextbox.Position.X, itemHeight);
        maxPlayersTextbox.Size  = new Vector2(targetWidth-maxPlayersTextbox.Position.X, itemHeight);
        allinEnabledCheckbox.Size = new Vector2(itemHeight*0.75f, itemHeight*0.75f);

        Al.DrawText(font, Colors.Black, 0, GetHeight(0), FontAlignFlags.Left, "Small Blind: ");
        Al.DrawText(font, Colors.Black, 0, GetHeight(1), FontAlignFlags.Left, "Max Bet: ");
        Al.DrawText(font, Colors.Black, 0, GetHeight(2), FontAlignFlags.Left, "Max Players: ");
        Al.DrawText(font, Colors.Black, 0, GetHeight(3), FontAlignFlags.Left, "All-In Enabled: ");

        smallblindTextbox.Render(ctx);
        maxBetTextbox.Render(ctx);
        maxPlayersTextbox.Render(ctx);
        allinEnabledCheckbox.Render(ctx);
        
        ctx.Stack.Pop();
        Al.ResetClippingRectangle();
        
        return;

        float GetHeight(int i)
        {
            return itemHeight * i + spacing * (i - 1);
        }
    }

    private void OnUserKick(LobbyUser user)
    {
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
         ? int.Parse(maxPlayersTextbox.GetString()) : 0;
        roomSettings.SmallBlind = !string.IsNullOrEmpty(smallblindTextbox.GetString())
            ? int.Parse(smallblindTextbox.GetString()) : 0;
        roomSettings.MaxBet = !string.IsNullOrEmpty(maxBetTextbox.GetString())
            ? int.Parse(maxBetTextbox.GetString()) : 0;
        roomSettings.IsAllInEnabled = allinEnabledCheckbox.Value;

        Console.WriteLine("Sending Configuration");
        _ = NetworkManager.SendSettings(roomSettings);
    }
    
    public void Update(double delta)
    {
        loading.Update(delta);
        if (loading.IsEnabled)
        {
            return;
        }

        if (NetworkManager.Handler!.TryGetPendingMessage(out RoomListUpdatedNotification? listUpdate)) {
            if (listUpdate.UpdateType is LobbyListUpdateType.UserLeft or LobbyListUpdateType.UserKicked) {
                users.RemoveAll(x => x.Name == listUpdate.Username);
            }else if (listUpdate.UpdateType == LobbyListUpdateType.UserJoined) {
                users.Add(new LobbyUser() {
                    Name = listUpdate.Username,
                    IsLocal = false,
                    IsOwner = false,
                });
            }
        }
        // ignore updates from the server. we are admin and we are the source of truth (hopefully)
        NetworkManager.Handler!.TryGetPendingMessage(out RoomSettingsChangeNotification _);
    }

    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Matrix4x4 Translation { get; set; }

    public void OnMouseMove(Vector2 pos, Vector2 delta)
    {
        pos = pos - new Vector2(Translation.Translation.X, Translation.Translation.Y);
        startGameButton.OnMouseMove(pos, delta);
        foreach (LobbyUser user in users)
        {
            user.Button?.OnMouseMove(pos, delta);
        }
        maxBetTextbox.OnMouseMove(pos,delta);
        maxPlayersTextbox.OnMouseMove(pos,delta);
        smallblindTextbox.OnMouseMove(pos,delta);
        allinEnabledCheckbox.OnMouseMove(pos,delta);
    }

    public void OnMouseDown(MouseButton button)
    {
        startGameButton.OnMouseDown(button);
        foreach (LobbyUser user in users)
        {
            user.Button?.OnMouseDown(button);
        }
        maxBetTextbox.OnMouseDown(button);
        maxPlayersTextbox.OnMouseDown(button);
        smallblindTextbox.OnMouseDown(button);
        allinEnabledCheckbox.OnMouseDown(button);
    }

    public void OnMouseUp(MouseButton button)
    {
        startGameButton.OnMouseUp(button);
        foreach (LobbyUser user in users)
        {
            user.Button?.OnMouseUp(button);
        }

        // remove users
        foreach (LobbyUser user in usersToDelete)
        {
            users.Remove(user);
        }

        usersToDelete.Clear();
        maxBetTextbox.OnMouseUp(button);
        maxPlayersTextbox.OnMouseUp(button);
        smallblindTextbox.OnMouseUp(button);
        allinEnabledCheckbox.OnMouseUp(button);
    }

    public void OnKeyDown(KeyCode key, KeyModifiers modifiers)
    {
        maxBetTextbox.OnKeyDown(key,modifiers);
        maxPlayersTextbox.OnKeyDown(key,modifiers);
        smallblindTextbox.OnKeyDown(key,modifiers);
    }

    public void OnKeyUp(KeyCode key, KeyModifiers modifiers)
    {
        maxBetTextbox.OnKeyUp(key,modifiers);
        maxPlayersTextbox.OnKeyUp(key,modifiers);
        smallblindTextbox.OnKeyUp(key,modifiers);
    }

    public void OnCharDown(char character)
    {
        maxBetTextbox.OnCharDown(character);
        maxPlayersTextbox.OnCharDown(character);
        smallblindTextbox.OnCharDown(character);
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