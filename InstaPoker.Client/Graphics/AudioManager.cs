using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public class AudioManager : IDisposable{

    private static AudioManager? _instance;
    
    private static readonly Dictionary<string, AllegroSample> Samples = [];

    public AudioManager() {
        _instance ??= this;
    }
    
    public static void RegisterAudio(string filename, string alias) {
        AllegroSample? sample = Al.LoadSample($"./Assets/Audio/{filename}");
        Samples[alias] = sample ?? throw new FileNotFoundException($"Could not load sample {alias} from {filename}");
    }

    public static AllegroSample GetAudio(string alias) {
        if (Samples.TryGetValue(alias, out AllegroSample? sample)) {
            return sample;
        }
        throw new InvalidOperationException("Audio sample not loaded.");
    }

    public void Dispose() {
        foreach (KeyValuePair<string, AllegroSample> sample in Samples) {
            Al.DestroySample(sample.Value);
        }
    }
}