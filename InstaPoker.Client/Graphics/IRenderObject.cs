using System.Numerics;
using SubC.AllegroDotNet.Enums;

namespace InstaPoker.Client.Graphics;

public interface IRenderObject {
    void Initialize();
    void Render(RenderContext ctx);
    void Update(double delta);
    
    public Vector2 Position { get; set; }

    public Vector2 Size { get; set; }
}

public interface IKeyboardInteractable {
    void OnKeyDown(KeyCode key, uint modifiers);
    void OnKeyUp(KeyCode key, uint modifiers);
    void OnCharDown(char character);
}

public interface IMouseInteractable {
    void OnMouseMove(Vector2 pos, Vector2 delta);
    void OnMouseDown(uint button);
    void OnMouseUp(uint button);
}
