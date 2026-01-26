using System.Numerics;
using SubC.AllegroDotNet.Enums;

namespace InstaPoker.Client.Graphics;

/// <summary>
/// Interface that defines common method and properties that hierarchy objects may use. 
/// </summary>
public interface IRenderObject {
    
    /// <summary>
    /// Method called after the Allegro Engine is initialized, before the first frame is rendered.
    /// Constructors may be called before the engine is initialized. 
    /// </summary>
    void Initialize();
    
    /// <summary>
    /// Render the current object to the screen.
    /// </summary>
    /// <param name="ctx">The current render context</param>
    void Render(RenderContext ctx);
    
    /// <summary>
    /// Updates all internal components of the object that are related with time and networking.
    /// </summary>
    /// <param name="delta">How much time has passed since the last frame in seconds</param>
    void Update(double delta);
    
    /// <summary>
    /// Relative position of this object in relation of the parent object.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The absolute size of the object in the screen.
    /// </summary>
    public Vector2 Size { get; set; }
    
    /// <summary>
    /// Holds a <see cref="Matrix4x4"/> that represents all the cumulative transforms of this object.
    /// </summary>
    /// <remarks>This object must be updated every frame to match the top of the stack.</remarks>
    public Matrix4x4 Translation { get; set; }
}

/// <summary>
/// Interface that defines common methods that objects that use and interact with the user's keyboard must
/// implement.
/// </summary>
public interface IKeyboardInteractable {
    
    /// <summary>
    /// Called when a key in the keyboard is pressed. For text typing, use <see cref="OnCharDown"/>.
    /// </summary>
    /// <param name="key">The code of the key that was pressed</param>
    /// <param name="modifiers">The modifiers</param>
    void OnKeyDown(KeyCode key, KeyModifiers modifiers);
    
    /// <summary>
    /// Called when a key in the keyboard is released.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="modifiers"></param>
    void OnKeyUp(KeyCode key, KeyModifiers modifiers);
    
    /// <summary>
    /// Version of <see cref="OnKeyDown"/> modified for text typing. Receives correct
    /// <see cref="char"/> that was typed and key repetition.
    /// </summary>
    /// <param name="character">The unicode character that was typed</param>
    void OnCharDown(char character);
}

/// <summary>
/// Enumeration that lists possible key modifiers from the keyboard. 
/// </summary>
[Flags]
public enum KeyModifiers {
    Shift = 1,
    Control = 2,
    LeftAlt = 4,
    Context = 32,
    RightAlt = 64,
    ScrollLock = 256,
    NumLock = 512,
    CapsLock = 1024
}

/// <summary>
/// Interface that defines methods that objects that use the mouse must implement.
/// </summary>
public interface IMouseInteractable {
    
    /// <summary>
    /// Called when the mouse is moved.
    /// </summary>
    /// <example>
    /// The implementer must transform mouse coordinates to local position with the following code:
    /// <code>
    ///     pos -= new Vector2(Translation.Translation.X, Translation.Translation.Y);
    /// </code>
    /// The <see cref="IRenderObject.Translation"/> must be updated in the <see cref="IRenderObject.Render"/> method
    /// using <c>Translation = ctx.Stack.Peek()</c>
    /// </example>
    /// <param name="pos">The position of the mouse, relative to the parent object</param>
    /// <param name="delta">Mouse movement speed, relative to last call</param>
    void OnMouseMove(Vector2 pos, Vector2 delta);
    
    /// <summary>
    /// Called when the user presses a mouse button.
    /// </summary>
    /// <param name="button">The button that was pressed</param>
    void OnMouseDown(MouseButton button);
    
    /// <summary>
    /// Called when the user releases a button that was pressed. 
    /// </summary>
    /// <param name="button">The button that was released</param>
    void OnMouseUp(MouseButton button);
}

/// <summary>
/// Enumeration with the mouse buttons that can be passed to <see cref="IMouseInteractable.OnMouseUp"/> and
/// <see cref="IMouseInteractable.OnMouseDown"/>.
/// </summary>
public enum MouseButton {
    Left = 1,
    Right = 2,
    Middle = 3
}
