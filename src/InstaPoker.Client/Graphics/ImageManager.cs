using System.Data;
using System.Runtime.InteropServices.ComTypes;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

/// <summary>
/// Class responsible for registering, loading and creating new bitmaps from image files.
/// </summary>
public sealed class ImageManager : IDisposable{

    private static ImageManager? _instance;

    /// <summary>
    /// Creates an instance of the manager and sets it as the singleton if there are no other instances created.
    /// </summary>
    public ImageManager() {
        _instance ??= this;
    }

    private static readonly Dictionary<string, AllegroBitmap> LoadedImages = [];
    private static readonly Dictionary<(string, int, int), AllegroBitmap> ResizedBitmaps = [];
    private static readonly Dictionary<(int, int), AllegroBitmap> ScratchPads = [];

    /// <summary>
    /// Registers and loads an image given a filename.
    /// </summary>
    /// <remarks>File paths are relative to the <c>Assets/Textures</c> folder</remarks>
    /// <param name="name">The name of the file, <b>with</b> extension</param>
    /// <param name="alias">The alias given to the image</param>
    /// <exception cref="FileNotFoundException">Thrown when the file was not found in the correct directory</exception>
    /// <exception cref="FileLoadException">Thrown when the Allegro library can't load the file
    /// <exception cref="ArgumentException">Thrown when a image with the given alias already exists</exception>
    /// as a bitmap image</exception>
    public static void RegisterImage(string name, string alias) {
        if (LoadedImages.ContainsKey(alias)) {
            throw new ArgumentException($"Already exists a image with the alias {alias}");
        }
        string path = $"Assets/Textures/{name}";
        if (!File.Exists(path)) {
            throw new FileNotFoundException($"The file {name} was not found. Is the extension correct?");
        }
        AllegroBitmap? bitmap = Al.LoadBitmap(path);
        if (bitmap is null) {
            throw new FileLoadException($"Could not load file {name} as an image.");
        }
        LoadedImages.Add(alias, bitmap);
    }

    /// <summary>
    /// Returns a handle to the requested image that was previously loaded with <see cref="RegisterImage"/>.
    /// </summary>
    /// <param name="alias">The alias of the image</param>
    /// <returns>A handle to the loaded image</returns>
    /// <exception cref="KeyNotFoundException">Thrown when there are no registered images with the given alias
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when the width or height are less or qual than 0</exception>
    public static AllegroBitmap GetImage(string alias, int? width = null, int? height = null) {
        if (LoadedImages.TryGetValue(alias, out AllegroBitmap? image)) {
            if (width is null && height is null) {
                return image;
            }
            width ??= Al.GetBitmapWidth(image);
            height ??= Al.GetBitmapHeight(image);
            if (width <= 0 || height <= 0) {
                throw new ArgumentException("Invalid image size", width <= 0 ? nameof(width) : nameof(height));
            }
            // check if a resized bitmap already exists
            if (ResizedBitmaps.TryGetValue((alias, width.Value, height.Value), out AllegroBitmap? resized)) {
                return resized;
            }
            // does not exist. resize
            resized = ResizeBitmap(image, width.Value, height.Value);
            ResizedBitmaps.Add((alias, width.Value, height.Value), resized);
            return resized;
        }

        throw new KeyNotFoundException($"There are not images loaded with the alias {alias}");
    }

    /// <summary>
    /// Creates or gets an existing bitmap with the specified size. Returns the same bitmap if dimensions are equal.
    /// Call this, draw to it and render immediately.
    /// </summary>
    /// <param name="width">The horizontal length of the image</param>
    /// <param name="height">The vertical length of the image</param>
    /// <param name="clear">If the bitmap must be cleared before returning</param>
    /// <returns>A handle to a bitmap with the given dimensions</returns>
    public static AllegroBitmap Borrow(int width, int height, bool clear = true) {
        if (width <= 0 || height <= 0) {
            throw new ArgumentException("Invalid image size", width <= 0 ? nameof(width) : nameof(height));
        }
        if (ScratchPads.TryGetValue((width, height), out AllegroBitmap? bitmap)) {
            if (clear) {
                bitmap.Clear();
            }
            return bitmap;
        }
        bitmap = Al.CreateBitmap(width, height);
        if (bitmap is null) {
            throw new Exception("Could not create bitmap with given size");
        }
        ScratchPads.Add((width,height), bitmap);
        if (clear) {
            bitmap.Clear();
        }
        return bitmap;
    }

    private static AllegroBitmap ResizeBitmap(AllegroBitmap bitmap, int newWidth, int newHeight) {
        AllegroBitmap? resized = Al.CreateBitmap(newWidth, newHeight);
        if (resized is null) {
            throw new Exception("Could not create resized bitmap");
        }
        
        AllegroBitmap prevTarget = Al.GetTargetBitmap()!;
        Al.SetTargetBitmap(resized);
        Al.DrawScaledBitmap(bitmap,
            0,0, Al.GetBitmapWidth(bitmap), Al.GetBitmapHeight(bitmap),
            0,0, newWidth, newHeight, FlipFlags.None);
        Al.SetTargetBitmap(prevTarget);
        return resized;
    }
    
    /// <summary>
    /// Releases all loaded images
    /// </summary>
    public void Dispose() {
        _instance = null;
        foreach ((string _, AllegroBitmap image) in LoadedImages) {
            Al.DestroyBitmap(image);
        }
        LoadedImages.Clear();
        
        foreach (((string,int,int) _, AllegroBitmap image) in ResizedBitmaps) {
            Al.DestroyBitmap(image);
        }
        ResizedBitmaps.Clear();

        foreach (((int, int) _, AllegroBitmap img) in ScratchPads) {
            Al.DestroyBitmap(img);
        }
        ScratchPads.Clear();
    }
    
}