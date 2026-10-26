using System.ComponentModel.Design;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Cards;
using InstaPoker.Client.Subsystems.Account;
using InstaPoker.Client.Subsystems.Network;
using InstaPoker.Client.Subsystems.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InstaPoker.Client.Subsystems;

public static class Service {
    public static IServiceProvider Provider {
        get => _disposed ? throw new InvalidOperationException("Provider is already disposed") : field;
        private set;
    } = null!;

    private static bool _disposed = false;

    public static void Init() {
        ServiceCollection services = new();
        Register(services);
        Provider = services.BuildServiceProvider();
    }

    private static void Register(ServiceCollection services) {
        services.AddSingleton<SettingsService>();
        services.AddSingleton<AccountStore>();
        services.AddSingleton<MainThreadDispatcher>();
        services.AddSingleton<NetworkManager>();
        services.AddSingleton<AssetLoader>();
        services.AddSingleton<MouseManager>();
        services.AddSingleton<FontManager>();
        services.AddSingleton<AudioManager>();
        services.AddSingleton<ImageManager>();
        services.AddSingleton<CardSpriteCache>();
        services.AddSingleton<ICardSpriteCache>(sp => sp.GetRequiredService<CardSpriteCache>());
        services.AddSingleton<CardRenderer>();
        
        services.AddLogging(builder => {
            builder.AddConsole();
#if DEBUG
            builder.SetMinimumLevel(LogLevel.Trace);
#else
            builder.SetMinimumLevel(LogLevel.Error);
#endif
        });
    }

    public static void Dispose() {
        ServiceProvider concrete = (ServiceProvider)Provider;
        _disposed = true;
        concrete.Dispose();
    }
}