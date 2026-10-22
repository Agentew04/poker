using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InstaPoker.Client.Subsystems.Settings;

public class SettingsService : IDisposable {
    private readonly string applicationFolder;
    private readonly string configFolder;
    private readonly List<Repo> repositories = [];

    private readonly ILogger<SettingsService> logger;
    private readonly IServiceProvider serviceProvider;
    
    private bool initialized;

    public SettingsService(ILogger<SettingsService> logger, IServiceProvider provider) {
        this.logger = logger;
        serviceProvider = provider;
        applicationFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "InstaPoker"
        );
        configFolder = Path.Combine(applicationFolder, "config");
        if (!Directory.Exists(applicationFolder)) {
            Directory.CreateDirectory(applicationFolder);
            logger.LogInformation("Application folder did not exist. Creating at {path}", applicationFolder);
        }

        if (!Directory.Exists(configFolder)) {
            Directory.CreateDirectory(configFolder);
            logger.LogInformation("Configuration folder did not exist. Creating at {path}", configFolder);
        }
    }

    private void Init() {
        if (initialized) {
            return;
        }
        initialized = true;
        logger.LogInformation("Initializing SettingsService...");
        
        IEnumerable<string> files = Directory.EnumerateFiles(configFolder);
        foreach (string file in files) {
            logger.LogInformation("Loading config file {file}", Path.GetFileName(file));
            try {
                string key = Path.GetFileNameWithoutExtension(file);
                repositories.Add(new Repo(key, file, serviceProvider.GetRequiredService<ILogger<Repo>>()));
            }
            catch (Exception ex) {
                logger.LogError(ex, "Error parsing file {file}: {err}", file, ex.Message);
            }
        }
    }

    public IConfigRepository GetRepository(string key) {
        if (!initialized) {
            Init();
        }
        
        // check if repository is registered
        Repo? repo = repositories.FirstOrDefault(x => x.Key == key);
        if (repo is null) {
            // create repo
            string path = Path.Combine(configFolder, key + ".json");
            logger.LogInformation("Tried getting non-existing repository({repo}), creating new file: {path}", key, path);
            File.WriteAllText(path, "{}");
            repo = new Repo(key, path, serviceProvider.GetRequiredService<ILogger<Repo>>());
            repositories.Add(repo);
        }
        
        return repo;
    }

    private class Repo : IConfigRepository {

        // private bool signed;
        // private bool encrypted; // encrypt values with an embedded key.
        // does not enforce safety, just on barrier for someone curious
        
        public string Key { get; init; }
        
        private readonly ILogger<Repo> logger;
        
        public Repo(string key, string path, ILogger<Repo> logger) {
            this.logger = logger;
            Key = key;
            filePath = path;
            ReadFile();
        }

        private void ReadFile() {
            string json = File.ReadAllText(filePath);
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            Stack<string> paths = new();
            Traverse(root);
            return;
            void Traverse(JsonElement objectElement) {
                foreach (JsonProperty property in objectElement.EnumerateObject()) {
                    if (property.Value.ValueKind == JsonValueKind.Object) {
                        paths.Push(property.Name);
                        Traverse(property.Value);
                    }else if (property.Value.ValueKind == JsonValueKind.String) {
                        Setting s = new(
                            $"{GetPath()}{property.Name}", 
                            property.Value.GetString());
                        settings.Add(s);
                    }else if (property.Value.ValueKind == JsonValueKind.Number) {
                        Setting s = new(
                            $"{GetPath()}{property.Name}", 
                            property.Value.GetSingle());
                        settings.Add(s);
                    }else if (property.Value.ValueKind == JsonValueKind.True ||
                              property.Value.ValueKind == JsonValueKind.False) {
                        Setting s = new(
                            $"{GetPath()}{property.Name}", 
                            property.Value.GetBoolean());
                        settings.Add(s);
                    }else if (property.Value.ValueKind == JsonValueKind.Null) {
                        Setting s = new(
                            $"{GetPath()}{property.Name}", 
                            null);
                        settings.Add(s);
                    }
                }
            }

            string GetPath() {
                StringBuilder sb = new();
                foreach (string path in paths) {
                    sb.Append(path);
                    sb.Append("/");
                }
                return sb.ToString();
            }
        }
        
        private readonly List<Setting> settings = [];
        private readonly string filePath;
        public bool Dirty { get; private set; }
        
        public T? GetValue<T>(string path) {
            foreach (Setting setting in settings) {
                if (setting.Path == path) {
                    return (T?)setting.Value;
                }
            }
            throw new KeyNotFoundException($"Key {path} not found");
        }

        public void CreateIfNotExists<T>(string path, T value) {
            bool exists = false;
            foreach (Setting setting in settings) {
                if (setting.Path == path) {
                    exists = true;
                }
            }

            if (exists) {
                return;
            }
            
            // create
            Setting set = new(path, value);
            settings.Add(set);
            Dirty = true;
        }

        public bool TryGetValue<T>(string path, out T? value) {
            foreach (Setting setting in settings) {
                if (setting.Path == path) {
                    value = (T?)setting.Value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        public void SetValue<T>(string path, T value) {
            foreach (Setting setting in settings) {
                if (setting.Path == path) {
                    setting.Value = value;
                    return;
                }
            }
            
            // doesn't exist. create key
            Setting newSetting = new(path, value);
            settings.Add(newSetting);
            Dirty = true;
        }
        
        public void Save() {
            logger.LogInformation("Saving settings repository {key}", Key);
            JsonObject json = BuildJson();
            using FileStream fs = File.OpenWrite(filePath);
            using Utf8JsonWriter writer = new(fs);
            json.WriteTo(writer, new JsonSerializerOptions() {
                IndentSize = 4,
                WriteIndented = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            });
            Dirty = false;
        }

        private JsonObject BuildJson() {
            JsonObject root = new();
            
            foreach (Setting setting in settings) {
                string[] parts = setting.Path.Split(['/', ':', '\\', '.']);
                JsonObject last = root;
                for (int i = 0; i < parts.Length; i++) {
                    string part = parts[i];

                    if (i != parts.Length - 1) {
                        // first/second/property
                        JsonNode? node = last[part];
                        if (node == null) {
                            // create
                            node = new JsonObject();
                            last.Add(part, node);
                        }
                        last = (JsonObject)node;
                    }
                    else {
                        // final
                        last.Add(part, JsonValue.Create(setting.Value));
                    }
                }
            }

            return root;
        }
    }

    private record Setting(string Path, object? Value) {
        public string Path { get; set; } = Path;
        public object? Value { get; set; } = Value;
    }

    public void Dispose() {
        if(!initialized) {
            return;
        }
        logger.LogInformation("Disposing SettingsService. Saving dirty repos");
        foreach (Repo repo in repositories) {
            if (!repo.Dirty) {
                continue;
            }
            repo.Save();
        }
    }
}

public interface IConfigRepository {
    T? GetValue<T>(string path);
    bool TryGetValue<T>(string path, out T? value);
    void SetValue<T>(string path, T value);
    void Save();
}