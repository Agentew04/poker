using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Objects;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Client.Network;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Game.Screens;

public class GameScreen() : SceneObject("GameScreen") {
    public override bool UseMouse => true;
    public override bool UseKeyboard => true;

    private readonly Button leaveButton = new(nameof(leaveButton));
    
    // pot
    private readonly Label potTitleLabel = new(nameof(potTitleLabel));
    private readonly Label potAmountLabel = new(nameof(potAmountLabel));

    // bottom player
    private readonly Label yourTurnLabel = new(nameof(yourTurnLabel));
    private readonly Button checkButton = new(nameof(checkButton));
    private readonly Button callButton = new(nameof(callButton));
    private readonly Button raiseButton = new(nameof(raiseButton));
    private readonly TextBox raiseTextbox = new(nameof(raiseTextbox));
    private readonly Label balanceTitle = new(nameof(balanceTitle));
    private readonly Label balanceAmount = new(nameof(balanceAmount));

    public override void Initialize() {
        AddChild(leaveButton);
        leaveButton.Style = ButtonStyle.RedButton with {
            FontSize = 20
        };
        leaveButton.Text = "Leave";
        leaveButton.Pressed += UserLeave;

        AddChild(potTitleLabel);
        potTitleLabel.Text = "Pot";
        potTitleLabel.FontSize = 30;
        potTitleLabel.VerticalAlign = VerticalAlign.Bottom;
        
        AddChild(potAmountLabel);
        potAmountLabel.Text = "14.000";
        potAmountLabel.FontSize = 30;
        potAmountLabel.VerticalAlign = VerticalAlign.Top;
        

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
        leaveButton.Position = new Vector2(margin, margin);
        leaveButton.Size = new Vector2(75, 30);

        const float playerSpace = 150;
        const float buttonSpacing = 20;
        const float headerSpace = 50;
        
        checkButton.Size = new Vector2(100, 50);
        callButton.Size = new Vector2(100, 50);
        raiseButton.Size = new Vector2(100, 50);
        raiseTextbox.Size = new Vector2(125, 50);

        yourTurnLabel.Position = new Vector2(Size.X * 0.5f, Size.Y - playerSpace);

        // center button
        callButton.Position = new Vector2(Size.X * 0.5f, Size.Y - playerSpace * 0.5f);
        // left button
        checkButton.Position = new Vector2(
            callButton.Position.X - callButton.Size.X * 0.5f - buttonSpacing,
            Size.Y - playerSpace * 0.5f);
        // right button
        raiseButton.Position = new Vector2(
            callButton.Position.X + callButton.Size.X * 0.5f + buttonSpacing,
            Size.Y - playerSpace * 0.5f);
        // txt box
        raiseTextbox.Position = new Vector2(
            raiseButton.Position.X + raiseButton.Size.X - 1,
            Size.Y - playerSpace * 0.5f);

        // balance
        balanceAmount.Text = LocalSettings.Amount.ToString();
        AllegroFont balTitleFont = FontManager.GetFont("ShareTech-Regular", balanceTitle.FontSize);
        balanceTitle.Position = new Vector2(
            Size.X - 2*margin,
            Size.Y - playerSpace * 0.5f - margin*0.5f);
        balanceAmount.Position = new Vector2(
            Size.X - 2*margin - Al.GetTextWidth(balTitleFont, balanceTitle.Text)*0.5f, 
            Size.Y - playerSpace*0.5f + margin*0.5f);
            
        // pot
        float potWidthHalf = Size.X * 0.1f;
        float potHeightHalf = potWidthHalf * 0.5f;
        potTitleLabel.Position = new Vector2(Size.X * 0.5f, headerSpace + potHeightHalf - 5);
        potAmountLabel.Position = new Vector2(Size.X * 0.5f, headerSpace + potHeightHalf + 5);


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
        
        // Pot background
        float potWidthHalf = Size.X * 0.1f;
        float potHeightHalf = potWidthHalf * 0.5f;
        Al.DrawFilledEllipse(Size.X * 0.5f, headerSpace + potHeightHalf, potWidthHalf, potHeightHalf, darkGreen);
        Al.DrawEllipse(Size.X * 0.5f, headerSpace + potHeightHalf, potWidthHalf, potHeightHalf, Colors.Black, 2);
        
        // debug
        Al.DrawRectangle(0, Size.Y - playerSpace, Size.X, Size.Y, Colors.Red, 1);
        Al.DrawRectangle(0, 0, Size.X, headerSpace, Colors.Blue, 1);
        base.Render(ctx);
    }

    private void UserLeave() {
        // notify server that we left the table.
        _ = NetworkManager.LeaveRoom();
        UserLeft?.Invoke();
    }

    public event Action? UserLeft;
}