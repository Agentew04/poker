using System.Reflection;
using Microsoft.Extensions.Logging;

namespace InstaPoker.Client.Graphics;

public class AssetLoader {

    private readonly ILogger<AssetLoader> logger;
    
    public AssetLoader(ILogger<AssetLoader> logger) {
        this.logger = logger;
    }
    
    public byte[] Read(string path) {
        Assembly asm = Assembly.GetExecutingAssembly();
        using Stream? stream = asm.GetManifestResourceStream(path);
        if (stream == null) {
            throw new Exception($"Invalid resource name: {path}");
        }
        using MemoryStream ms = new();
        stream.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        byte[] bytes = ms.ToArray();
        return bytes;
    }
}