using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Objects;

namespace InstaPoker.Client.Game.Screens;

public class WindowFrame<T> : SceneObject where T : notnull {
    public WindowFrame(string name) : base(name) {
    }

    public override bool UseMouse => true;
    public override bool UseKeyboard => true;

    private Dictionary<T,IScreen<T>> screens = [];
    private Dictionary<IScreen<T>, SceneObject> objects = [];
    
    public T ActiveKey { get; private set; }
    public SceneObject ActiveObject { get; private set; }
    

    public void AddScreen(IScreen<T> screen, bool isDefault = false) {
        screens.Add(screen.Key, screen);
        SceneObject obj = objects[screen] = (SceneObject)screen;
        obj.Position = Vector2.Zero;
        obj.Size = Size;
        obj.Initialize();

        if (isDefault) {
            SetScreen(screen.Key);
        }
    }

    public void SetScreen(T key) {
        IScreen<T> screen = screens[key];
        SceneObject obj = objects[screen];
        
        ActiveKey = key;
        ActiveObject = obj;
        
        obj.IsEnabled = true;
        screen.OnShow();
        
        
        // disable others
        foreach (KeyValuePair<T, IScreen<T>> other in screens) {
            if (other.Value == screen) {
                continue;
            }
            SceneObject otherObj = objects[other.Value];
            otherObj.IsEnabled = false;
        }
    }
}