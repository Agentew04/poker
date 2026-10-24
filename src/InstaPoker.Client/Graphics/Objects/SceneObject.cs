using System.Numerics;
using System.Reflection;
using InstaPoker.Client.Game;
using InstaPoker.Client.Graphics.Layout;
using InstaPoker.Client.Graphics.Styles;
using InstaPoker.Client.Subsystems;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;

namespace InstaPoker.Client.Graphics.Objects;

public abstract class SceneObject {
    private readonly List<SceneObject> children = [];
    
    /// <summary>
    /// If the object must receive mouse events.
    /// </summary>
    public abstract bool UseMouse { get; }
    
    /// <summary>
    /// The name of the object on the scene. Does not need to be unique. Just for identification
    /// purposes in the <see cref="DebugWindow"/>
    /// </summary>
    public string Name {get;set;}

    /// <summary>
    /// If the object must receive keyboard events.
    /// </summary>
    public abstract bool UseKeyboard { get; }

    /// <summary>
    /// If this object is currently enabled and should receive updates, events and be rendered.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// If the transform of the object should be calculated by its render call hierarchy.
    /// </summary>
    public bool AutoTransform { get; set; } = true;

    /// <summary>
    /// If a clipping rectangle must be calculated for this object.
    /// </summary>
    public bool Clip { get; set; } = false;

    /// <summary>
    /// The horizontal alignment of the <see cref="Position"/> in relation to the <see cref="Size"/>. 
    /// </summary>
    public HorizontalAlign HorizontalAlign { get; set; } = HorizontalAlign.Left;

    /// <summary>
    /// The vertical alignment of the <see cref="Position"/> in relation to the <see cref="Size"/>.
    /// </summary>
    public VerticalAlign VerticalAlign { get; set; } = VerticalAlign.Top;

    private bool isInitialized = false;

    protected ILogger Logger {
        get {
            if (field == null) {
                Type loggerType = typeof(ILogger<>).MakeGenericType(GetType());
                field = (ILogger)Service.Provider.GetRequiredService(loggerType);
            }
            return field;
        }
    }

    public SceneObject(string name) {
        Name = name;
    }
    
    /// <summary>
    /// Injects the services declared with <see cref="ServiceAttribute"/> and then runs <see cref="Initialize"/>.
    /// This is the only supported entry point for initializing a scene object: whoever initializes an object
    /// injects its dependencies, so no caller can forget the injection step.
    /// </summary>
    public void InitializeWithServices() {
        if (isInitialized) {
            return;
        }

        InjectServices(this);
        Initialize();
    }

    /// <summary>
    /// Method called after the Allegro Engine is initialized, before the first frame is rendered.
    /// Constructors may be called before the engine is initialized.
    /// </summary>
    /// <remarks>
    /// Prefer calling <see cref="InitializeWithServices"/> instead of this method directly. Overrides must
    /// call <c>base.Initialize()</c> so the children declared with <see cref="ChildObjectAttribute"/> are
    /// added to the scene graph.
    /// </remarks>
    public virtual void Initialize() {
        if (isInitialized) {
            return;
        }

        isInitialized = true;

        IEnumerable<SceneObject> childFields = GetType()
            .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .OfType<FieldInfo>()
            .Where(x => x.GetCustomAttribute<ChildObjectAttribute>() is not null &&
                        typeof(SceneObject).IsAssignableFrom(x.FieldType))
            .Select(x => x.GetValue(this))
            .OfType<SceneObject>();

        foreach (SceneObject child in childFields) {
            AddChild(child);
        }
    }

    /// <summary>
    /// Updates <see cref="Size"/> for objects whose size is derived from their content, like a
    /// <see cref="Label"/>. Objects with an explicit size do not need to override this.
    /// </summary>
    public virtual void Measure() {
        // empty: the size was set explicitly
    }

    private static void InjectServices(SceneObject target) {
        IEnumerable<FieldInfo> services = target.GetType()
            .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .OfType<FieldInfo>()
            .Where(x => x.GetCustomAttribute<ServiceAttribute>() is not null);

        foreach (FieldInfo field in services) {
            object? service = Service.Provider.GetService(field.FieldType);
            if (service is null) {
                throw new InvalidOperationException(
                    $"Service {field.FieldType.FullName} is not registered " +
                    $"(required by {target.GetType().Name}.{field.Name})");
            }
            field.SetValue(target, service);
        }
    }
    
    /// <summary>
    /// Updates all internal components of the object that are related with time and networking.
    /// </summary>
    /// <param name="delta">How much time has passed since the last frame in seconds</param>
    public virtual void Update(double delta) {
        foreach (SceneObject child in children) {
            if (child.IsEnabled) {
                child.Update(delta);
            }
        }
    }
    
    /// <summary>
    /// Measures and positions all children inside itself.
    /// </summary>
    /// <remarks>
    /// <see cref="Position"/> is an anchor: this method must not mutate it. The resolved top-left corner is
    /// available through <see cref="Bounds"/> and is what <see cref="Render"/> uses.
    /// </remarks>
    public virtual void PositionElements() {
        foreach (SceneObject child in children) {
            child.PositionElements();
        }
    }
    
    /// <summary>
    /// Render the current object to the screen.
    /// </summary>
    /// <param name="ctx">The current render context</param>
    public virtual void Render(RenderContext ctx) {
        foreach (SceneObject child in children) {
            if (child.IsEnabled) {
                if (!child.isInitialized) {
                    throw new Exception("Tried rendering child element that was unitialized. Type: " + child.GetType().Name);
                }
                ctx.Stack.Push();
                if (!child.AutoTransform) {
                    ctx.Stack.Replace(child.Transform);
                }
                ctx.Stack.Multiply(Matrix4x4.CreateTranslation(
                    child.Bounds.Position.X, child.Bounds.Position.Y, 0));
                
                Matrix4x4 absolutePosition = ctx.Stack.Peek();
                if (Clip) {
                    Al.SetClippingRectangle((int)absolutePosition.Translation.X, (int)absolutePosition.Translation.Y,
                    (int)child.Size.X, (int)child.Size.Y);
                }
                // if (child.AutoTransform) {
                child.Transform = absolutePosition;
                // }
                child.PositionElements();
                child.Render(ctx);
                Al.ResetClippingRectangle();
                ctx.Stack.Pop();
            }
        }
    }
    
    /// <summary>
    /// Anchor position of this object in relation to the parent object. How the anchor maps to the
    /// rectangle depends on <see cref="HorizontalAlign"/> and <see cref="VerticalAlign"/>.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Offset from the top-left corner to <see cref="Position"/>, based on the current alignment.
    /// </summary>
    public Vector2 PivotOffset => new(
        HorizontalAlign switch {
            HorizontalAlign.Center => Size.X * 0.5f,
            HorizontalAlign.Right => Size.X,
            _ => 0
        },
        VerticalAlign switch {
            VerticalAlign.Center => Size.Y * 0.5f,
            VerticalAlign.Bottom => Size.Y,
            _ => 0
        });

    /// <summary>
    /// The resolved bounds of this object in its parent's coordinate space, always with a top-left position.
    /// </summary>
    public Bounds Bounds => new(Position - PivotOffset, Size);

    /// <summary>
    /// Places the object by its resolved top-left corner, regardless of its alignment.
    /// </summary>
    public void PlaceAt(Vector2 topLeft) {
        Position = topLeft + PivotOffset;
    }

    /// <summary>
    /// The absolute size of the object in the screen.
    /// </summary>
    public Vector2 Size { get; set; }

    /// <summary>
    /// Holds a <see cref="Matrix4x4"/> that represents the absolute transformation needed to draw the object.
    /// Automatically updated if <see cref="AutoTransform"/> is true. 
    /// </summary>
    public Matrix4x4 Transform { get; set; }

    /// <summary>
    /// Called when a key in the keyboard is pressed. For text typing, use <see cref="OnCharDown"/>.
    /// </summary>
    /// <param name="key">The code of the key that was pressed</param>
    /// <param name="modifiers">The modifiers</param>
    public virtual void OnKeyDown(KeyCode key, KeyModifiers modifiers) {
        foreach (SceneObject child in children) {
            if (child.UseKeyboard && child.IsEnabled) {
                child.OnKeyDown(key, modifiers);
            }
        }
    }

    /// <summary>
    /// Called when a key in the keyboard is released.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="modifiers"></param>
    public virtual void OnKeyUp(KeyCode key, KeyModifiers modifiers) {
        foreach (SceneObject child in children) {
            if (child.UseKeyboard && child.IsEnabled) {
                child.OnKeyUp(key, modifiers);
            }
        }
    }

    /// <summary>
    /// Version of <see cref="OnKeyDown"/> modified for text typing. Receives correct
    /// <see cref="char"/> that was typed and key repetition.
    /// </summary>
    /// <param name="character">The unicode character that was typed</param>
    public virtual void OnCharDown(char character) {
        foreach (SceneObject child in children) {
            if (child.UseKeyboard && child.IsEnabled) {
                child.OnCharDown(character);
            }
        }
    }

    /// <summary>
    /// Called when the mouse is moved.
    /// </summary>
    /// <param name="pos">The position of the mouse, relative to the parent object</param>
    /// <param name="delta">Mouse movement speed, relative to last call</param>
    public virtual void OnMouseMove(Vector2 pos, Vector2 delta) {
        foreach (SceneObject child in children) {
            if (child.UseMouse && child.IsEnabled) {
                Vector2 localizedPos = pos - child.Transform.Translation.ToVec2();
                child.OnMouseMove(localizedPos, delta);
            }
        }
    }

    /// <summary>
    /// Called when the user presses a mouse button.
    /// </summary>
    /// <param name="button">The button that was pressed</param>
    public virtual void OnMouseDown(MouseButton button) {
        foreach (SceneObject child in children) {
            if (child.UseMouse && child.IsEnabled) {
                child.OnMouseDown(button);
            }
        }
    }

    /// <summary>
    /// Called when the user releases a button that was pressed. 
    /// </summary>
    /// <param name="button">The button that was released</param>
    public virtual void OnMouseUp(MouseButton button) {
        foreach (SceneObject child in children) {
            if (child.UseMouse && child.IsEnabled) {
                child.OnMouseUp(button);
            }
        }
    }

    protected void AddChild(SceneObject child) {
        if (children.Contains(child)) {
            return;
        }

        children.Add(child);
        child.InitializeWithServices();
        if (!child.isInitialized) {
            throw new Exception($"SceneObject {child.Name} did not call base Initialize");
        }
    }

    public bool HasChild(SceneObject child) {
        return children.Contains(child);
    }

    public IReadOnlyList<SceneObject> GetChildren() => children;

    protected void RemoveChild(SceneObject child) {
        children.Remove(child);
    }

    public virtual void OnDebugDraw() {
        // empty
    }

    /// <summary>
    /// Returns a new placement object with this object as the target.
    /// </summary>
    /// <returns></returns>
    public Placement Place() {
        return new Placement(this);
    }
}

public enum LogLevel {
    Info,
    Warn,
    Error
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
/// Enumeration with the mouse buttons that can be passed to <see cref="SceneObject.OnMouseUp"/> and
/// <see cref="SceneObject.OnMouseDown"/>.
/// </summary>
public enum MouseButton {
    Left = 1,
    Right = 2,
    Middle = 3
}
