using InstaPoker.Client.Subsystems;
using Microsoft.Extensions.DependencyInjection;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

/// <summary>
/// Class responsible for managing font files used for text drawing.
/// </summary>
public sealed class FontManager : IDisposable {

    private const LoadFontFlags Flags = LoadFontFlags.TtfNoKerning | LoadFontFlags.TtfNoAutohint;

    private static FontManager? _instance;

    private static readonly Dictionary<string, FontEntry> LoadedFonts = [];

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
        if (LoadedFonts.ContainsKey(name)) {
            return;
        }

        AssetLoader loader = Service.Provider.GetRequiredService<AssetLoader>();
        // the pinned buffer must live as long as the fonts: Allegro keeps reading from the memfile
        // while glyphs are rasterized
        LoadedFonts[name] = new FontEntry {
            Bytes = new PinnedBytes(loader.Read($"Assets/Fonts/{name}.ttf"))
        };
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
        if (!LoadedFonts.TryGetValue(name, out FontEntry? entry)) {
            throw new Exception("Font not registered");
        }

        if (entry.Sizes.TryGetValue(size, out AllegroFont? font)) {
            return font;
        }

        // Each font owns the file it was loaded from and closes it on al_destroy_font, so every size
        // needs its own memfile. Reusing one file makes the second load start at the previous font's
        // seek position, which makes FT_Open_Face fail.
        AllegroFile file = Al.OpenMemfile(entry.Bytes.Pointer, entry.Bytes.Length, "r")
            ?? throw new InvalidOperationException($"Could not open memory file for font {name}");

        font = Al.LoadTtfFontF(file, $"{name}.ttf", size, Flags);
        if (font is null) {
            // on failure FreeType already closed the file (see allegro ttf.c), so don't close it here
            throw new FileNotFoundException($"Could not load font {name} at size {size}");
        }

        entry.Sizes[size] = font;
        return font;
    }

    /// <summary>
    /// Disposes of all previously loaded fonts.
    /// </summary>
    public void Dispose() {
        foreach (FontEntry entry in LoadedFonts.Values) {
            foreach (AllegroFont font in entry.Sizes.Values) {
                // destroying the font also closes the memfile it owns
                Al.DestroyFont(font);
            }
            entry.Bytes.Dispose();
        }

        LoadedFonts.Clear();
        _instance = null;
    }

    private sealed class FontEntry {
        public required PinnedBytes Bytes { get; init; }
        public Dictionary<int, AllegroFont> Sizes { get; } = [];
    }
}
