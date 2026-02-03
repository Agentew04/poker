using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Objects;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Client.Network;
using InstaPoker.Core;
using InstaPoker.Core.Messages.Notifications;
using InstaPoker.Core.Messages.Responses;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game.Screens;

/// <summary>
/// Screen that shows a lobby where the current user is <b>not</b> the owner/admin.
/// </summary>
public class PlayerLobbyScreen() : SceneObject("Player Lobby Screen") {

    public override bool UseMouse => true;
    public override bool UseKeyboard => false;
    
    private string code = string.Empty;
    private string title = string.Empty;
    private readonly LoadingLabel loading = new(nameof(loading));
    private RoomSettings roomSettings = new();
    private List<LobbyUser> users = [];

    private Task<JoinRoomResponse>? joinTask;

    public override void Initialize() {
        AddChild(loading);
        loading.FontSize = 28;
    }

    /// <summary>
    /// Code executed when the users click on the button to join a room using a code.
    /// </summary>
    /// <param name="code">The code the user entered</param>
    public void OnShow(string code) {
        this.code = code;
        loading.ShowDots = true;
        loading.Text = "Joining room";
        loading.Show();
        joinTask = NetworkManager.JoinRoom(code);
        joinTask.ContinueWith(task => {
            if (task.Result.Result != JoinRoomResult.Success) {
                loading.Text = task.Result.Result switch {
                    JoinRoomResult.RoomDoesNotExist => "Room does not exist!",
                    JoinRoomResult.RoomFull => "Room is full!",
                    JoinRoomResult.UsernameAlreadyExist => "Room already has a player with your name!",
                    JoinRoomResult.AlreadyInOtherRoom => "You are in another room!",
                    _ => "Error joining: " + task.Result.Result
                };
                loading.Text += " Returning in 5 seconds";
                loading.ShowDots = false;
                loading.Show();
                Task.Delay(5000).ContinueWith(_ => {
                    UserLeft?.Invoke();
                });
                return;
            }
            
            title = "Room " + code;
            loading.Hide();
            users.Clear();
            users.AddRange(task.Result.ConnectedUsers.Select(x => new LobbyUser()
            {
                Name = x,
                IsLocal = false,
                IsOwner = x == task.Result.OwnerName
            }));
            users.Add(new LobbyUser() {
                IsLocal = true,
                IsOwner = false,
                Name = LocalSettings.Username
            });
            roomSettings = task.Result.Settings;
        });
    }

    public override void PositionElements() {
        loading.Size = Size;
        loading.Position = Vector2.Zero;
        base.PositionElements();
    }

    public override void Render(RenderContext ctx) {
        loading.Render(ctx);
        if (loading.IsEnabled)
        {
            return;
        }
        
        ctx.UpdateTransform();
        // title
        AllegroFont titleFont = FontManager.GetFont("ShareTech-Regular", 36);
        Al.DrawText(titleFont, Colors.Black,
        (int)(Size.X * 0.5f), (int)(Size.Y * 0.0625 - Al.GetFontLineHeight(titleFont) * 0.5f),
        FontAlignFlags.Center, title);
        
        // draw waiting for host
        AllegroFont subtitlefont = FontManager.GetFont("ShareTech-Regular", 34);
        Al.DrawText(subtitlefont, Colors.Black, 
            (int)(Size.X * 0.5f),
            (int)(Size.Y - Size.Y * 0.0625f - Al.GetFontLineHeight(subtitlefont)*0.5f),
            FontAlignFlags.Center,
            "Waiting for host");
        
        RenderPlayerList(ctx);
        RenderConfiguration(ctx);
        
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
                    user.Button = new Button("leaveButton");
                    user.Button.Initialize();
                    user.Button.Text = "Leave";
                    user.Button.Pressed += OnUserLeave;
                    user.Button.Style = ButtonStyle.RedButton with
                    {
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
    
    private void RenderConfiguration(RenderContext ctx) {
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
        
        Al.DrawText(font, Colors.Black, 0, GetHeight(0), FontAlignFlags.Left, "Small Blind: " + roomSettings.SmallBlind);
        Al.DrawText(font, Colors.Black, 0, GetHeight(1), FontAlignFlags.Left, "Max Bet: " + roomSettings.MaxBet);
        Al.DrawText(font, Colors.Black, 0, GetHeight(2), FontAlignFlags.Left, "Max Players: " + roomSettings.MaxPlayers);
        Al.DrawText(font, Colors.Black, 0, GetHeight(3), FontAlignFlags.Left, "All-In Enabled: " + (roomSettings.IsAllInEnabled ? "Yes" : "No" ));
        
        ctx.Stack.Pop();
        Al.ResetClippingRectangle();
        
        return;
        float GetHeight(int i)
        {
            return itemHeight * i + spacing * (i - 1);// + Al.GetFontLineHeight(font) * 0.5f;
        }
    }

    private void OnUserLeave() {
        NetworkManager.LeaveRoom();
        UserLeft?.Invoke();
    }

    /// <inheritdoc/>
    public override void Update(double delta) {
        loading.Update(delta);
        if (loading.IsEnabled) {
            return;
        }
        
        if (NetworkManager.Handler!.TryGetPendingMessage(out RoomListUpdatedNotification? listUpdate)) {
            if (listUpdate.UpdateType is LobbyListUpdateType.UserLeft or LobbyListUpdateType.UserKicked) {
                LobbyUser? removed = users.Find(x => x.Name == listUpdate.Username);
                if (removed is not null) {
                    users.Remove(removed);
                    if (removed.Button is not null) {
                        RemoveChild(removed.Button);
                    }
                }
                
                if (listUpdate.UpdateType == LobbyListUpdateType.UserKicked && listUpdate.Username == LocalSettings.Username) {
                    // user was kicked. show kick screen and return to main menu
                    loading.ShowDots = false;
                    loading.Text = "You were kicked from the room. Returning to Main Menu in 5 seconds";
                    loading.Show();
                    Task.Delay(5000).ContinueWith(_ => UserLeft?.Invoke());
                }
            }else if (listUpdate.UpdateType == LobbyListUpdateType.UserJoined) {
                users.Add(new LobbyUser() {
                    Name = listUpdate.Username,
                    IsLocal = false,
                    IsOwner = false,
                });
            }
        }

        if (NetworkManager.Handler!.TryGetPendingMessage(
                out RoomSettingsChangeNotification? settingsChangeNotification)) {
            roomSettings = settingsChangeNotification.NewSettings;
        }
        
        if(NetworkManager.Handler!.TryGetPendingMessage(out NewRoomOwnerNotification? newRoomOwnerNotification)) {
            LobbyUser? owner = users.FirstOrDefault(x => x.Name == newRoomOwnerNotification.Owner);
            if (owner is null) {
                throw new Exception("Unknown user became owner!!");
            }
            users.ForEach(x => x.IsOwner = false);
            owner.IsOwner = true;
            if (owner.IsLocal) {
                UpgradedToAdmin?.Invoke(users, roomSettings, code);
            }
        }   
    }
    
    /// <summary>
    /// Event fired when the user leaves or is kicked from the room.
    /// </summary>
    public event Action? UserLeft;

    /// <summary>
    /// Event fired when the user is upgraded to an admin role. The current room state is
    /// passed as parameters to <see cref="AdminLobbyScreen"/> consume in the <see cref="AdminLobbyScreen.OnUpgrade"/>
    /// method.
    /// </summary>
    public event Action<List<LobbyUser>, RoomSettings,string>? UpgradedToAdmin;

    public event Action? GameStarted;
}