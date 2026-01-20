using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public class FontManager : IDisposable {

    private static FontManager? _instance;
    
    private static readonly Dictionary<string,int> FontsLoaded = [];
    private static readonly List<Dictionary<int, AllegroFont>> Fonts = [];

    public FontManager() {
        _instance ??= this;
    }
    
    public static void RegisterFont(string name) {
        FontsLoaded[name] = Fonts.Count;
        Fonts.Add(new Dictionary<int, AllegroFont>());
    }

    public static AllegroFont GetFont(string name, int size) {
        if (!FontsLoaded.TryGetValue(name, out int index)) {
            throw new Exception("Font not registered");
        }

        Dictionary<int, AllegroFont> sizes = Fonts[index];
        if (sizes.TryGetValue(size, out AllegroFont? font)) return font;
        font = Al.LoadTtfFont($"./Assets/Fonts/{name}.ttf", size, LoadFontFlags.TtfNoKerning|LoadFontFlags.TtfNoAutohint);
        sizes[size] = font ?? throw new Exception("Could not load font " + name);
        return font;
    }
    
    public void Dispose() {
        foreach (Dictionary<int, AllegroFont> sizes in Fonts) {
            foreach (KeyValuePair<int, AllegroFont> kvp in sizes) {
                Al.DestroyFont(kvp.Value);
            }
        }
    }
}