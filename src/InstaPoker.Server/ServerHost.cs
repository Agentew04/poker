using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InstaPoker.Server;

/// <summary>
/// Composition root of the server. Kept separate from <see cref="Program"/> so tests can boot the same
/// host (same services, same configuration) without spawning another process.
/// </summary>
public static class ServerHost {

    public static IHost Build(string[] args) {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.Configure<ServerOptions>(
            builder.Configuration.GetSection(ServerOptions.SectionName)
        );

        builder.Services.AddSingleton(ServerTls.Certificate);
        builder.Services.AddSingleton<MessageBus>();
        builder.Services.AddSingleton<RoomManager>();
        builder.Services.AddSingleton<UserManager>();
        builder.Services.AddSingleton<Router>();

        builder.Services.AddHostedService<Server>();
        builder.Services.AddHostedService<Router>();

        return builder.Build();
    }
}
