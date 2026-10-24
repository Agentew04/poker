using System.ComponentModel.Design;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Subsystems.Account;
using InstaPoker.Client.Subsystems.Network;
using InstaPoker.Client.Subsystems.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InstaPoker.Client.Subsystems;

public static class Service {
    public static IServiceProvider Provider { get; private set; } = null!;

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
        
        services.AddLogging(builder => {
            builder.AddConsole();
#if DEBUG
            builder.SetMinimumLevel(LogLevel.Trace);
#else
            builder.SetMinimumLevel(LogLevel.Error);
#endif
        });
    }
}