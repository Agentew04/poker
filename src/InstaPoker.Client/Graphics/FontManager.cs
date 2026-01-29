using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

/// <summary>
/// Class responsible for managing font files used for text drawing.
/// </summary>
public sealed class FontManager : IDisposable {

    private static FontManager? _instance;
    
    private static readonly Dictionary<string,int> FontsLoaded = [];
    private static readonly List<Dictionary<int, AllegroFont>> Fonts = [];

    /// <summary>
    /// Creates a new instance of the manager and assigns it as the singleton.
    /// </summary>
    public FontManager() {
        _instance ??= this;
    }
    
    /// <summary>
    /// Registers a new font that can be loaded dynamically.
    /// </summary>
    /// <remarks>Only accepts fonts that have the .ttf extension.</remarks>
    /// <param name="name">The name of the font. Does not include path or extension</param>
    /// <exception cref="FileNotFoundException">Thrown when no matching file is found at the Fonts/ directory</exception>
    public static void RegisterFont(string name) {
        if (!File.Exists($"./Assets/Fonts/{name}.ttf")) {
            throw new FileNotFoundException($"Font File with name {name}.ttf does not exist in the Fonts/ folder");
        }
        FontsLoaded[name] = Fonts.Count;
        Fonts.Add(new Dictionary<int, AllegroFont>());
    }

    /// <summary>
    /// Returns the specified font and loads it if it wasn't already loaded. Fonts must be previously registered
    /// using <see cref="RegisterFont"/>. 
    /// </summary>
    /// <param name="name">The name of the font</param>
    /// <param name="size">The size of the font, in units per EM</param>
    /// <param name="useSizeAsHeight">Whether <paramref name="size"/> is in units per EM or the total height of
    /// the font glyphs </param>
    /// <returns>A font matching the provided specs</returns>
    /// <exception cref="Exception">
    /// Thrown when the font was not previously registered with <see cref="RegisterFont"/>
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the corresponding font file can't be found. Ideally never thrown, because
    /// <see cref="RegisterFont"/> already thrown on error.
    /// </exception>
    public static AllegroFont GetFont(string name, int size, bool useSizeAsHeight = false) {
        if (useSizeAsHeight) {
            size = -size;
        }
        if (!FontsLoaded.TryGetValue(name, out int index)) {
            throw new Exception("Font not registered");
        }

        Dictionary<int, AllegroFont> sizes = Fonts[index];
        if (sizes.TryGetValue(size, out AllegroFont? font)) return font;
        font = Al.LoadTtfFont($"./Assets/Fonts/{name}.ttf", size, LoadFontFlags.TtfNoKerning|LoadFontFlags.TtfNoAutohint);
        sizes[size] = font ?? throw new FileNotFoundException("Could not load font " + name);
        return font;
    }
    
    /// <summary>
    /// Disposes of all previously loaded fonts.
    /// </summary>
    public void Dispose() {
        foreach (Dictionary<int, AllegroFont> sizes in Fonts) {
            foreach (KeyValuePair<int, AllegroFont> kvp in sizes) {
                Al.DestroyFont(kvp.Value);
            }
        }

        _instance = null;
    }
}