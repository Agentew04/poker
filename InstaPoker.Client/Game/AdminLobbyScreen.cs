using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Client.Network;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game;

public class AdminLobbyScreen : IRenderObject, IMouseInteractable, IKeyboardInteractable {

    private string code;
    private string title;
    private readonly List<LobbyUser> users = [];
    private readonly List<LobbyUser> usersToDelete = [];
    private readonly LoadingLabel loading = new();
    private readonly Button startGameButton = new();
    
    public void Initialize() {
        loading.Initialize();
        loading.Text = "Creating room";
        loading.FontSize = 28;
        startGameButton.Initialize();
        startGameButton.Label = "Start Game";
        startGameButton.Style = ButtonStyle.Default with {
            FontSize = 30
        };
        startGameButton.Pressed += () => OnGameStart?.Invoke();
    }

    public void OnShow() {
        loading.Show();
        Task<string> createRoomTask = NetworkManager.CreateRoom();
        createRoomTask.ContinueWith(x => {
            users.Clear();
            users.Add(new LobbyUser() {
                Name = LocalSettings.Username,
                IsLocal = true,
                IsOwner = true
            });
            users.Add(new LobbyUser() {
                Name = "Carlos", IsLocal = false, IsOwner = false
            });
            users.Add(new LobbyUser() {
                Name = "Eduardo", IsLocal = false, IsOwner = false
            });
            code = x.Result;
            title = "Room " + code;
            loading.Hide();
        });
        
    }

    public void Render(RenderContext ctx) {
        loading.Size = Size;
        loading.Position = Vector2.Zero;
        loading.Render(ctx);
        if (loading.IsEnabled) {
            return;
        }
        
        ctx.UpdateTransform();
        
        // draw title
        AllegroFont titleFont = FontManager.GetFont("ShareTech-Regular", 36);
        Al.DrawText(titleFont, AllegroColor.Black, 
            (int)(Size.X*0.5f), (int)(Size.Y * 0.0625 - Al.GetFontLineHeight(titleFont)*0.5f),
            FontAlignFlags.Center, title);
        
        // draw player list
        RenderPlayerList(ctx);
        
        // draw options
        
        // draw start game
        startGameButton.Size = new Vector2(300, 60);
        startGameButton.Position = new Vector2(
            Size.X * 0.5f - startGameButton.Size.X * 0.5f,
            Size.Y - Size.Y * 0.0625f - startGameButton.Size.Y*0.5f
        );
        startGameButton.Render(ctx);
    }

    private void RenderPlayerList(RenderContext ctx) {
        const float width = 500;
        // draw background
        Vector2 p1 = new(Size.X * 0.5f - width * 0.5f, Size.Y * 0.125f);
        Vector2 p2 = new(Size.X * 0.5f + width*0.5f, Size.Y - Size.Y * 0.125f); 
        ctx.UpdateTransform();
        Al.DrawFilledRectangle(p1.X, p1.Y, p2.X, p2.Y, AllegroColor.BackgroundWhite);
        Al.DrawRectangle(p1.X, p1.Y, p2.X, p2.Y, AllegroColor.Black, 1);
        Al.SetClippingRectangle((int)p1.X, (int)p1.Y, (int)(p2.X - p1.X), (int)(p2.Y- p1.Y));
        const float margin = 15;
        const float spacing = 5;

        AllegroFont font = FontManager.GetFont("ShareTech-Regular", 26);

        ctx.Stack.Push();
        ctx.Stack.Multiply(Matrix4x4.CreateTranslation(p1.X, p1.Y, 0));
        ctx.UpdateTransform();
        float y = margin + Al.GetFontLineHeight(font) * 0.5f;
        foreach (LobbyUser user in users) {
            float x = margin;
            Al.DrawText(font, AllegroColor.Black, (int)x, (int)(y - Al.GetFontLineHeight(font)*0.5f), 
                FontAlignFlags.Left, user.Name);
            x += Al.GetTextWidth(font, user.Name);
            if (user.IsOwner) {
                Al.DrawText(font, AllegroColor.Black, (int)x, (int)(y - Al.GetFontLineHeight(font)*0.5f), 
                    FontAlignFlags.Left, " (Owner)");
                x += Al.GetTextWidth(font, " (Owner)");
            }

            if (user.IsLocal) {
                Al.DrawText(font, AllegroColor.Black, (int)x, (int)(y - Al.GetFontLineHeight(font)*0.5f), 
                    FontAlignFlags.Left, " (You)");
                x += Al.GetTextWidth(font, " (You)");
                // leave button
                if (user.Button is null) {
                    user.Button = new Button();
                    user.Button.Initialize();
                    user.Button.Label = "Leave";
                    user.Button.Pressed += () => {
                        OnLeave?.Invoke();
                    };
                    user.Button.Style = ButtonStyle.RedButton with {
                        FontSize = 24
                    };
                }
                user.Button.Size = new Vector2(100, Al.GetFontLineHeight(font));
                user.Button.Position = new Vector2(p1.X + x+margin, p1.Y + y-Al.GetFontLineHeight(font)*0.5f);
            }
            else {
                // kick button
                if (user.Button is null) {
                    user.Button = new Button();
                    user.Button.Initialize();
                    user.Button.Label = "Kick";
                    user.Button.Pressed += () => {
                        OnUserKick(user);
                    };
                    user.Button.Style = ButtonStyle.RedButton with {
                        FontSize = 24
                    };
                }
                user.Button.Size = new Vector2(100, Al.GetFontLineHeight(font));
                user.Button.Position = new Vector2(p1.X + x+margin, p1.Y + y-Al.GetFontLineHeight(font)*0.5f);
            }

            y += Al.GetFontLineHeight(font) + spacing;
        }
        ctx.Stack.Pop();

        users.ForEach(x => x.Button?.Render(ctx));

        Al.ResetClippingRectangle();
    }

    private void OnUserKick(LobbyUser user) {
        usersToDelete.Add(user);
        _ = NetworkManager.KickUser(user.Name);
    }

    public void Update(double delta) {
        loading.Update(delta);
        if (loading.IsEnabled) {
            return;
        }
    }

    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }

    public void OnMouseMove(Vector2 pos, Vector2 delta) {
        pos = Utils.TransformToLocal(Position, pos);
        
        startGameButton.OnMouseMove(pos,delta);
        foreach (LobbyUser user in users) {
            user.Button?.OnMouseMove(pos,delta);
        }
    }

    public void OnMouseDown(uint button) {
        startGameButton.OnMouseDown(button);
        foreach (LobbyUser user in users) {
            user.Button?.OnMouseDown(button);
        }
    }

    public void OnMouseUp(uint button) {
        startGameButton.OnMouseUp(button);
        foreach (LobbyUser user in users) {
            user.Button?.OnMouseUp(button);
        }

        // remove users
        foreach (LobbyUser user in usersToDelete) {
            users.Remove(user);
        }
        usersToDelete.Clear();
    }
    
    public void OnKeyDown(KeyCode key, uint modifiers) {
    }

    public void OnKeyUp(KeyCode key, uint modifiers) {
    }

    public void OnCharDown(char character) {
    }

    public event Action? OnGameStart;

    public event Action? OnLeave;

}