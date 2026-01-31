using SubC.AllegroDotNet.Enums;

namespace InstaPoker.Client.Graphics;

public class KeyGesture {
    private readonly Dictionary<KeyCode,bool> keys;
    private readonly KeyModifiers modifiers;
    private readonly Action action;

    public KeyGesture(List<KeyCode> keys, KeyModifiers modifiers, Action action) {
        this.keys = keys.ToDictionary(x => x, x=>false);
        this.modifiers = modifiers;
        this.action = action;
    }

    public void OnKeyDown(KeyCode key, KeyModifiers modifiers) {
        if (!keys.ContainsKey(key)) {
            return;
        }
        
        keys[key] = true;
        // check if all are true, then perform
        if ((this.modifiers & modifiers) == this.modifiers && keys.All(x => x.Value)) {
            action.Invoke();
        }
    }

    public void OnKeyUp(KeyCode key, KeyModifiers modifiers) {
        if (keys.ContainsKey(key)) {
            keys[key] = false;
        }
    }

    public void OnCharDown(char character) {
        // empty
    }
}