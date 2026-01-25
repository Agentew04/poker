using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public sealed class AudioManager : IDisposable{

    private static AudioManager? _instance;
    
    private static readonly Dictionary<string, AllegroSample> Samples = [];
    private static readonly Dictionary<string, List<string>> Groups = [];

    public AudioManager() {
        _instance ??= this;
    }
    
    public static void RegisterAudio(string filename, string alias) {
        AllegroSample? sample = Al.LoadSample($"./Assets/Audio/{filename}");
        Samples[alias] = sample ?? throw new FileNotFoundException($"Could not load sample {alias} from {filename}");
    }

    public static void CreateGroup(string groupName, params string[] aliases) {
        if (Samples.ContainsKey(groupName)) {
            throw new InvalidOperationException("Group cannot have same name as an existing audio sample");
        }

        foreach (string alias in aliases) {
            if (!Samples.ContainsKey(alias)) {
                throw new InvalidOperationException($"Alias {alias} is not registered");
            }
        }
        Groups.Add(groupName, aliases.ToList());
    }

    public static AllegroSample GetAudio(string alias) {
        if (Samples.TryGetValue(alias, out AllegroSample? sample)) {
            return sample;
        }
        // must be an group
        if (Groups.TryGetValue(alias, out List<string>? aliases)) {
            int audioIndex = Random.Shared.Next(aliases.Count);
            return Samples[aliases[audioIndex]];
        }
        throw new InvalidOperationException("Audio sample not loaded.");
    }

    public void Dispose() {
        foreach (KeyValuePair<string, AllegroSample> sample in Samples) {
            Al.DestroySample(sample.Value);
        }
        Groups.Clear();
        Samples.Clear();
    }
}