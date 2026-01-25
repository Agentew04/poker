using SubC.AllegroDotNet.Enums;

namespace InstaPoker.Client.Graphics;

public class KeyGesture : IKeyboardInteractable{
    private readonly Dictionary<KeyCode,bool> keys;
    private readonly uint modifiers;
    private readonly Action action;

    public KeyGesture(List<KeyCode> keys, uint modifiers, Action action) {
        this.keys = keys.ToDictionary(x => x, x=>false);
        this.modifiers = modifiers;
        this.action = action;
    }

    public void OnKeyDown(KeyCode key, uint modifiers) {
        if (!keys.ContainsKey(key)) {
            return;
        }
        
        keys[key] = true;
        // check if all are true, then perform
        if ((this.modifiers & modifiers) == this.modifiers && keys.All(x => x.Value)) {
            action.Invoke();
        }
    }

    public void OnKeyUp(KeyCode key, uint modifiers) {
        if (keys.ContainsKey(key)) {
            keys[key] = false;
        }
    }

    public void OnCharDown(char character) {
        // empty
    }

    public event Action? Performed = null;
}