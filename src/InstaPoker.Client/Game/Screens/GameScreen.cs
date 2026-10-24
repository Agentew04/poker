using System.Numerics;
using InstaPoker.Client.Game.Screens.Components;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Objects;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Client.Subsystems;
using InstaPoker.Client.Subsystems.Network;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game.Screens;

public class GameScreen() : SceneObject("GameScreen"), IScreen<Screen> {
    
    public Screen Key => Screen.Game;
    
    public override bool UseMouse => true;
    public override bool UseKeyboard => true;

    private readonly Button leaveButton = new(nameof(leaveButton));
    
    // pot
    [ChildObject] private readonly Pot pot = new(nameof(pot));

    // bottom player
    [ChildObject] private readonly Label yourTurnLabel = new(nameof(yourTurnLabel));
    [ChildObject] private readonly Button checkButton = new(nameof(checkButton));
    [ChildObject] private readonly Button callButton = new(nameof(callButton));
    [ChildObject] private readonly Button raiseButton = new(nameof(raiseButton));
    [ChildObject] private readonly TextBox raiseTextbox = new(nameof(raiseTextbox));
    [ChildObject] private readonly Label balanceTitle = new(nameof(balanceTitle));
    [ChildObject] private readonly Label balanceAmount = new(nameof(balanceAmount));

    [Service] private NetworkManager networkManager = null!; 

    public override void Initialize() {
        AddChild(leaveButton);
        leaveButton.Style = ButtonStyle.RedButton with {
            FontSize = 20
        };
        leaveButton.Text = "Leave";
        leaveButton.Pressed += UserLeave;

        AddChild(pot);
        
        AddChild(yourTurnLabel);
        yourTurnLabel.Text = "Your Turn!";
        yourTurnLabel.FontSize = 30;
        yourTurnLabel.VerticalAlign = VerticalAlign.Top;

        AddChild(checkButton);
        checkButton.Text = "Check";
        checkButton.Style = ButtonStyle.Default with {
            FontSize = 30
        };
        checkButton.HorizontalAlign = HorizontalAlign.Right;
        checkButton.VerticalAlign = VerticalAlign.Center;
        AddChild(callButton);
        callButton.Text = "Call";
        callButton.Style = ButtonStyle.Default with {
            FontSize = 30
        };
        callButton.HorizontalAlign = HorizontalAlign.Center;
        callButton.VerticalAlign = VerticalAlign.Center;
        AddChild(raiseButton);
        raiseButton.Text = "Raise";
        raiseButton.Style = ButtonStyle.Default with {
            FontSize = 30
        };
        raiseButton.HorizontalAlign = HorizontalAlign.Left;
        raiseButton.VerticalAlign = VerticalAlign.Center;
        AddChild(raiseTextbox);
        raiseTextbox.Style = TextBoxStyle.Default with {
            FontSize = 30
        };
        raiseTextbox.VerticalTextAlignment = VerticalAlign.Center;
        raiseTextbox.HorizontalTextAlignment = HorizontalAlign.Left;
        raiseTextbox.HorizontalAlign = HorizontalAlign.Left;
        raiseTextbox.VerticalAlign = VerticalAlign.Center;
        raiseTextbox.Keyboard = TextboxKeyboard.Numeric;

        AddChild(balanceTitle);
        balanceTitle.FontSize = 30;
        balanceTitle.Text = "Balance:";
        balanceTitle.HorizontalAlign = HorizontalAlign.Right;
        balanceTitle.VerticalAlign = VerticalAlign.Bottom;

        AddChild(balanceAmount);
        balanceAmount.FontSize = 28;
        balanceAmount.HorizontalAlign = HorizontalAlign.Center;
        balanceAmount.VerticalAlign = VerticalAlign.Top;

        base.Initialize();
    }

    public void OnShow() {
        Console.WriteLine("Game Started");
    }

    public override void PositionElements() {
        const float margin = 10;
        leaveButton.Place().WithSize(75, 30).At(margin, margin);

        const float playerSpace = 150;
        const float buttonSpacing = 20;

        callButton.Place().WithSize(100, 50)
            .AtAnchor(Size.X * 0.5f, Size.Y - playerSpace * 0.5f);
        checkButton.Place().WithSize(100, 50)
            .LeftOf(callButton, buttonSpacing)
            .AlignCenterYWith(callButton);
        raiseButton.Place().WithSize(100, 50)
            .RightOf(callButton, buttonSpacing)
            .AlignCenterYWith(callButton);
        raiseTextbox.Place().WithSize(125, 50)
            .RightOf(raiseButton, -1)
            .AlignCenterYWith(raiseButton);

        yourTurnLabel.Place().AtAnchor(Size.X * 0.5f, Size.Y - playerSpace);

        // balance
        balanceAmount.Text = LocalSettings.Amount.ToString();
        AllegroFont balTitleFont = FontManager.GetFont("ShareTech-Regular", balanceTitle.FontSize);
        balanceTitle.Place().AtAnchor(
            Size.X - 2 * margin,
            Size.Y - playerSpace * 0.5f - margin * 0.5f);
        balanceAmount.Place().AtAnchor(
            Size.X - 2 * margin - Al.GetTextWidth(balTitleFont, balanceTitle.Text) * 0.5f,
            Size.Y - playerSpace * 0.5f + margin * 0.5f);

        base.PositionElements();
    }

    public override void Render(RenderContext ctx) {
        const float playerSpace = 150;
        const float headerSpace = 50;
        ctx.UpdateTransform();

        // balance background
        var darkGreen = new AllegroColor {
            R = 0.4117647058823529f, G = 0.8588235294117647f, B = 0.48627450980392156f, A = 1
        };
        Al.DrawFilledRoundedRectangle(balanceAmount.Position.X - 3, balanceAmount.Position.Y - 3,
            balanceAmount.Position.X + balanceAmount.Size.X + 5, balanceAmount.Position.Y + balanceAmount.Size.Y + 5,
            5, 5, darkGreen);
        Al.DrawRoundedRectangle(balanceAmount.Position.X - 3, balanceAmount.Position.Y - 3,
            balanceAmount.Position.X + balanceAmount.Size.X + 5, balanceAmount.Position.Y + balanceAmount.Size.Y + 5,
            5, 5, Colors.Black, 2);
        
        // debug
        Al.DrawRectangle(0, Size.Y - playerSpace, Size.X, Size.Y, Colors.Red, 1);
        Al.DrawRectangle(0, 0, Size.X, headerSpace, Colors.Blue, 1);
        base.Render(ctx);
    }

    private void UserLeave() {
        // notify server that we left the table.
        _ = networkManager.LeaveRoom();
        UserLeft?.Invoke();
    }

    public event Action? UserLeft;
}