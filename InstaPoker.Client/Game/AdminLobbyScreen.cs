using System.Numerics;
using System.Security.Claims;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Client.Network;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game;

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
        startGameButton.Pressed += () => OnGameStart?.Invoke();
        maxBetTextbox.Initialize();
        maxPlayersTextbox.Initialize();
        smallblindTextbox.Initialize();
        maxBetTextbox.Style = TextBoxStyle.Default;
        maxPlayersTextbox.Style =  TextBoxStyle.Default;
        smallblindTextbox.Style = TextBoxStyle.Default;
        maxBetTextbox.Keyboard = TextboxKeyboard.Numeric;
        maxPlayersTextbox.Keyboard = TextboxKeyboard.Numeric;
        smallblindTextbox.Keyboard = TextboxKeyboard.Numeric;
        maxBetTextbox.HorizontalFontAlignment = HorizontalAlign.Left;
        maxPlayersTextbox.HorizontalFontAlignment = HorizontalAlign.Left;
        smallblindTextbox.HorizontalFontAlignment = HorizontalAlign.Left;
        maxBetTextbox.VerticalFontAlignment = VerticalAlign.Center;
        maxPlayersTextbox.VerticalFontAlignment = VerticalAlign.Center;
        smallblindTextbox.VerticalFontAlignment = VerticalAlign.Center;
        maxBetTextbox.MaxCharacters = 12;
        maxPlayersTextbox.MaxCharacters = 2;
        smallblindTextbox.MaxCharacters = 6;
    }

    public void OnShow()
    {
        loading.Show();
        Task<string> createRoomTask = NetworkManager.CreateRoom();
        createRoomTask.ContinueWith(x =>
        {
            users.Clear();
            users.Add(new LobbyUser()
            {
                Name = LocalSettings.Username,
                IsLocal = true,
                IsOwner = true
            });
            users.Add(new LobbyUser()
            {
                Name = "Carlos", IsLocal = false, IsOwner = false
            });
            users.Add(new LobbyUser()
            {
                Name = "Eduardo", IsLocal = false, IsOwner = false
            });
            code = x.Result;
            title = "Room " + code;
            roomSettings = new RoomSettings()
            {
                SmallBlind = 50,
                IsAllInEnabled = true,
                MaxPlayers = 8,
                MaxBet = 1000
            };
            loading.Hide();
        });
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
        Al.DrawText(titleFont, AllegroColor.Black,
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
        Al.DrawFilledRectangle(p1.X, p1.Y, p2.X, p2.Y, AllegroColor.BackgroundWhite);
        Al.DrawRectangle(p1.X, p1.Y, p2.X, p2.Y, AllegroColor.Black, 1);
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
            Al.DrawText(font, AllegroColor.Black, (int)x, (int)(y - Al.GetFontLineHeight(font) * 0.5f),
                FontAlignFlags.Left, user.Name);
            x += Al.GetTextWidth(font, user.Name);
            if (user.IsOwner)
            {
                Al.DrawText(font, AllegroColor.Black, (int)x, (int)(y - Al.GetFontLineHeight(font) * 0.5f),
                    FontAlignFlags.Left, " (Owner)");
                x += Al.GetTextWidth(font, " (Owner)");
            }

            if (user.IsLocal)
            {
                Al.DrawText(font, AllegroColor.Black, (int)x, (int)(y - Al.GetFontLineHeight(font) * 0.5f),
                    FontAlignFlags.Left, " (You)");
                x += Al.GetTextWidth(font, " (You)");
                // leave button
                if (user.Button is null)
                {
                    user.Button = new Button();
                    user.Button.Initialize();
                    user.Button.Label = "Leave";
                    user.Button.Pressed += () => { OnLeave?.Invoke(); };
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
                    user.Button.Pressed += () => { OnUserKick(user); };
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

        foreach (var user in users)
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

        const int itemCount = 3; // TODO: trocar para 4 e add allowAllIn
        float totalHeight = itemHeight * itemCount + spacing*(itemCount-1);
        ctx.Stack.Push();
        ctx.Stack.Multiply(Matrix4x4.CreateTranslation(margin, Size.Y*0.5f - totalHeight*0.5f, 0));
        ctx.UpdateTransform();
        
        Al.DrawRectangle(0,0, 200, totalHeight, AllegroColor.Black, 1);
        smallblindTextbox.Size = new Vector2(100, itemHeight);
        maxBetTextbox.Size = new Vector2(100, itemHeight);
        maxPlayersTextbox.Size  = new Vector2(100, itemHeight);
        
        Al.DrawText(font, AllegroColor.Black, 0, GetHeight(0), FontAlignFlags.Left, "Small Blind: ");
        Al.DrawText(font, AllegroColor.Black, 0, GetHeight(1), FontAlignFlags.Left, "Max Bet: ");
        Al.DrawText(font, AllegroColor.Black, 0, GetHeight(2), FontAlignFlags.Left, "Max Players: ");

        smallblindTextbox.Position = new Vector2(Al.GetTextWidth(font, "Small Blind: "), GetHeight(0));
        maxBetTextbox.Position = new Vector2(Al.GetTextWidth(font, "Max Bet: "), GetHeight(1));
        maxPlayersTextbox.Position = new Vector2(Al.GetTextWidth(font, "Max Players: "), GetHeight(2));
        
        smallblindTextbox.Render(ctx);
        maxBetTextbox.Render(ctx);
        maxPlayersTextbox.Render(ctx);
        
        ctx.Stack.Pop();
        Al.ResetClippingRectangle();
        
        return;

        float GetHeight(int i)
        {
            return itemHeight * i + spacing * (i - 1) + Al.GetFontLineHeight(font) * 0.5f;
        }
    }

    private void OnUserKick(LobbyUser user)
    {
        usersToDelete.Add(user);
        _ = NetworkManager.KickUser(user.Name);
    }

    public void Update(double delta)
    {
        loading.Update(delta);
        if (loading.IsEnabled)
        {
            return;
        }
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
    }

    public void OnMouseDown(uint button)
    {
        startGameButton.OnMouseDown(button);
        foreach (LobbyUser user in users)
        {
            user.Button?.OnMouseDown(button);
        }
        maxBetTextbox.OnMouseDown(button);
        maxPlayersTextbox.OnMouseDown(button);
        smallblindTextbox.OnMouseDown(button);
    }

    public void OnMouseUp(uint button)
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
    }

    public void OnKeyDown(KeyCode key, uint modifiers)
    {
        maxBetTextbox.OnKeyDown(key,modifiers);
        maxPlayersTextbox.OnKeyDown(key,modifiers);
        smallblindTextbox.OnKeyDown(key,modifiers);
    }

    public void OnKeyUp(KeyCode key, uint modifiers)
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

    public event Action? OnGameStart;

    public event Action? OnLeave;
}