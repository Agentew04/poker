using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

/// <summary>
/// Class that manages audio files.
/// </summary>
public sealed class AudioManager : IDisposable{

    private static AudioManager? _instance;
    
    private static readonly Dictionary<string, AllegroSample> Samples = [];
    private static readonly Dictionary<string, List<string>> Groups = [];

    public AudioManager() {
        _instance ??= this;
    }
    
    /// <summary>
    /// Registers a new audio effect.
    /// </summary>
    /// <param name="filename">The path to the audio file. Relative to the <c>Assets/Audio/</c> folder</param>
    /// <param name="alias">The unique name for this audio</param>
    /// <exception cref="FileNotFoundException">Thrown when the file is not found on the disk. Common when the
    /// developer writes the wrong extension or the filepath is not correctly relative to the Audio folder.</exception>
    public static void RegisterAudio(string filename, string alias) {
        AllegroSample? sample = Al.LoadSample($"./Assets/Audio/{filename}");
        Samples[alias] = sample ?? throw new FileNotFoundException($"Could not load sample {alias} from {filename}");
    }

    /// <summary>
    /// Creates a group from a collection of previously registered audio files.
    /// </summary>
    /// <param name="groupName">The name of the group</param>
    /// <param name="aliases">A list of all audio files that belong to this group</param>
    /// <exception cref="InvalidOperationException">Thrown when already exists a audio file with the same
    /// name as <paramref name="groupName"/> or an alias passed in <paramref name="aliases"/> was not previously
    /// registered with <see cref="RegisterAudio"/> </exception>
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

    /// <summary>
    /// Returns the Allegro handle for the given audio. If <paramref cref="alias"/> is a group name, one of the
    /// members of the group is chosen randomly.
    /// </summary>
    /// <param name="alias">The name of the audio or group to be played</param>
    /// <returns>A handle to the audio file</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when there is no audio or group loaded with the given alias
    /// </exception>
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

    /// <summary>
    /// Disposes of all loaded audio files. 
    /// </summary>
    public void Dispose() {
        foreach (KeyValuePair<string, AllegroSample> sample in Samples) {
            Al.DestroySample(sample.Value);
        }
        Groups.Clear();
        Samples.Clear();
        _instance = null;
    }
}