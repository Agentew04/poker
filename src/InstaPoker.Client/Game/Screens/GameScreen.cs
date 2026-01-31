using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Styles;

namespace InstaPoker.Client.Game.Screens;

public class GameScreen : SceneObject {
    public override bool UseMouse => true;
    public override bool UseKeyboard => true;

    private readonly Button leaveButton = new();
    
    public override void Initialize() {
        AddChild(leaveButton);
        leaveButton.Style = ButtonStyle.RedButton with {
            FontSize = 20
        };
        leaveButton.Label = "Leave";
        leaveButton.Pressed += UserLeave;
        
        base.Initialize();
    }

    public void OnShow() {
        Console.WriteLine("Game Started");
    }

    public override void PositionElements() {
        const float margin = 10;
        leaveButton.Position = new Vector2(margin, margin);
        leaveButton.Size = new Vector2(75, 30);
        base.PositionElements();
    }

    private void UserLeave() {
        // notify server that we left the table.
        UserLeft?.Invoke();
    }

    public event Action? UserLeft;
}