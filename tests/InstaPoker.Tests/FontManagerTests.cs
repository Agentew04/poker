using InstaPoker.Client.Graphics;
using InstaPoker.Client.Subsystems;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;
using Xunit;

namespace InstaPoker.Tests;

[Collection("Allegro")]
public class FontManagerTests {

    [Fact]
    public void Loads_multiple_sizes_from_the_same_ttf() {
        Service.Init();
        Assert.True(Al.InstallSystem(LibraryVersion.V526));

        try {
            Assert.True(Al.InitTtfAddon());

            FontManager fontManager = new();
            try {
                fontManager.RegisterFont("ShareTech-Regular");

                AllegroFont font30 = fontManager.GetFont("ShareTech-Regular", 30);
                AllegroFont font28 = fontManager.GetFont("ShareTech-Regular", 28);
                AllegroFont font22 = fontManager.GetFont("ShareTech-Regular", 22);

                Assert.NotNull(font30);
                Assert.NotNull(font28);
                Assert.NotNull(font22);
            }
            finally {
                fontManager.Dispose();
                Al.ShutdownTtfAddon();
            }
        }
        finally {
            Al.UninstallSystem();
        }
    }
}
