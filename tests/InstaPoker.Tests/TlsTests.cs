using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using InstaPoker.Client.Subsystems.Network;
using InstaPoker.Core;
using InstaPoker.Core.Messages;
using InstaPoker.Core.Messages.Requests;
using InstaPoker.Core.Messages.Responses;
using InstaPoker.Server;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InstaPoker.Tests;

public class TlsTests {

    [Fact]
    public void Server_certificate_is_self_signed_valid_for_ten_years_and_has_private_key() {
        X509Certificate2 certificate = ServerTls.Certificate;

        Assert.Equal(certificate.Subject, certificate.Issuer);
        Assert.True(certificate.HasPrivateKey);
        Assert.True(certificate.NotBefore <= DateTime.UtcNow);
        Assert.True(certificate.NotAfter > DateTime.UtcNow.AddYears(9));
        Assert.True(certificate.NotAfter < DateTime.UtcNow.AddYears(11));
        Assert.Equal("localhost", certificate.GetNameInfo(X509NameType.DnsName, false),
            ignoreCase: true);
    }

    [Fact]
    public async Task Client_accepts_the_pinned_certificate_and_exchanges_messages() {
        using TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        Task server = Task.Run(async () => {
            using TcpClient serverClient = await listener.AcceptTcpClientAsync();
            await using SslStream ssl = new(serverClient.GetStream(), false);
            await ssl.AuthenticateAsServerAsync(ServerTls.Certificate);

            using MessageReader reader = new(ssl, true);
            await using MessageWriter writer = new(ssl, true);
            Message request = await reader.ReadNextMessageAsync(CancellationToken.None);
            Assert.IsType<CreateRoomRequest>(request);
            await writer.WriteAsync(new CreateRoomResponse {
                RoomCode = "TLS1",
                Settings = new RoomSettings { MaxPlayers = 3 }
            });
        });

        using TcpClient client = new();
        await client.ConnectAsync(IPAddress.Loopback, port);
        await using SslStream clientSsl = new(client.GetStream(), false);
        await clientSsl.AuthenticateAsClientAsync(new SslClientAuthenticationOptions {
            TargetHost = "localhost",
            RemoteCertificateValidationCallback = ServerCertificatePinning.Validate
        });

        await using MessageWriter clientWriter = new(clientSsl, true);
        using MessageReader clientReader = new(clientSsl, true);
        await clientWriter.WriteAsync(new CreateRoomRequest());

        Message response = await clientReader.ReadNextMessageAsync(CancellationToken.None);
        CreateRoomResponse createRoom = Assert.IsType<CreateRoomResponse>(response);
        Assert.Equal("TLS1", createRoom.RoomCode);
        Assert.Equal(3, createRoom.Settings.MaxPlayers);

        await server;
    }

    [Fact]
    public async Task Client_rejects_a_server_certificate_signed_with_a_different_key() {
        using RSA rsa = RSA.Create(2048);
        CertificateRequest request = new("CN=InstaPoker.Server", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        SubjectAlternativeNameBuilder alternativeNames = new();
        alternativeNames.AddDnsName("localhost");
        request.CertificateExtensions.Add(alternativeNames.Build());
        using X509Certificate2 generated = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        // SChannel needs a non-ephemeral key for server auth, so round-trip through a PFX
        byte[] pfx = generated.Export(X509ContentType.Pfx);
        using X509Certificate2 impostor = X509CertificateLoader.LoadPkcs12(pfx, null, X509KeyStorageFlags.UserKeySet);

        using TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        Task server = Task.Run(async () => {
            try {
                using TcpClient serverClient = await listener.AcceptTcpClientAsync();
                await using SslStream ssl = new(serverClient.GetStream(), false);
                await ssl.AuthenticateAsServerAsync(impostor);
            }
            catch (Exception) {
                // the client aborting the handshake is the expected outcome
            }
        });

        using TcpClient client = new();
        await client.ConnectAsync(IPAddress.Loopback, port);
        await using SslStream clientSsl = new(client.GetStream(), false);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            clientSsl.AuthenticateAsClientAsync(new SslClientAuthenticationOptions {
                TargetHost = "localhost",
                RemoteCertificateValidationCallback = ServerCertificatePinning.Validate
            }));

        await server;
    }

    [Fact]
    public async Task Client_reports_certificate_mismatch_when_the_server_key_changed() {
        using RSA rsa = RSA.Create(2048);
        CertificateRequest request = new("CN=InstaPoker.Server", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        SubjectAlternativeNameBuilder alternativeNames = new();
        alternativeNames.AddDnsName("localhost");
        request.CertificateExtensions.Add(alternativeNames.Build());
        using X509Certificate2 generated = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        byte[] pfx = generated.Export(X509ContentType.Pfx);
        using X509Certificate2 impostor = X509CertificateLoader.LoadPkcs12(pfx, null, X509KeyStorageFlags.UserKeySet);

        using TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        Task server = Task.Run(async () => {
            try {
                using TcpClient serverClient = await listener.AcceptTcpClientAsync();
                NetworkStream ns = serverClient.GetStream();
                using BinaryWriter bw = new(ns, Encoding.UTF8, true);
                using BinaryReader br = new(ns, Encoding.UTF8, true);
                bw.Write("InstaPoker.Server");
                bw.Write(new Version(1, 0, 0, 0));
                bw.Flush();
                _ = br.ReadString();
                _ = br.ReadVersion();
                _ = br.ReadString();

                await using SslStream ssl = new(ns, false);
                await ssl.AuthenticateAsServerAsync(impostor);
            }
            catch (Exception) {
                // the client aborting the handshake is the expected outcome
            }
        });

        using TcpClient client = new();
        await client.ConnectAsync(IPAddress.Loopback, port);

        await Assert.ThrowsAsync<ServerCertificateMismatchException>(() =>
            ServerHandshake.IdentifyAndSecureAsync(
                client, "localhost", "mismatch-test-user", new Version(1, 0, 0, 0), NullLogger.Instance));

        await server;
    }

    [Fact]
    public async Task Real_server_completes_the_tls_handshake_and_answers_a_request() {
        _ = Task.Run(() => new InstaPoker.Server.Server().Run());
        await Task.Delay(500);

        using TcpClient client = new();
        await client.ConnectAsync("127.0.0.1", 12512);

        await using SslStream ssl = await ServerHandshake.IdentifyAndSecureAsync(
            client, "localhost", "tls-test-user", new Version(1, 0, 0, 0), NullLogger.Instance);

        Assert.True(ssl.IsAuthenticated);
        Assert.True(ssl.IsEncrypted);

        MainThreadDispatcher dispatcher = new(NullLogger<MainThreadDispatcher>.Instance);
        using MessageHandler handler = new(
            new MessageReader(ssl, true),
            new MessageWriter(ssl, true),
            dispatcher,
            NullLogger<MessageHandler>.Instance);

        CreateRoomResponse response = await handler
            .SendRequest<CreateRoomRequest, CreateRoomResponse>(new CreateRoomRequest())
            .WaitAsync(TimeSpan.FromSeconds(10));

        Assert.False(string.IsNullOrWhiteSpace(response.RoomCode));

        handler.Dispose();
        client.Close();

        // the server should notice the disconnect and drop the connection from its user list
        Stopwatch disconnectTimeout = Stopwatch.StartNew();
        while (UserManager.Connections.Any(c => c.Username == "tls-test-user") &&
               disconnectTimeout.ElapsedMilliseconds < 3000) {
            await Task.Delay(50);
        }
        Assert.DoesNotContain(UserManager.Connections, c => c.Username == "tls-test-user");
    }
}
