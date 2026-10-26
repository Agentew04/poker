using System.Numerics;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Cards;
using InstaPoker.Client.Subsystems;
using InstaPoker.Core.Games;
using Microsoft.Extensions.DependencyInjection;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;
using Xunit;

namespace InstaPoker.Tests;

[Collection("Allegro")]
public class CardRenderingTests {

    [Fact]
    public void Renders_a_front_and_a_back_sprite() {
        Service.Init();
        Assert.True(Al.InstallSystem(LibraryVersion.V526));

        FontManager? fonts = null;
        ImageManager? images = null;
        CardSpriteCache? cache = null;
        AllegroBitmap? target = null;
        try {
            Assert.True(Al.InitTtfAddon());
            Assert.True(Al.InitPrimitivesAddon());
            Assert.True(Al.InitImageAddon());

            fonts = Service.Provider.GetRequiredService<FontManager>();
            images = Service.Provider.GetRequiredService<ImageManager>();
            fonts.RegisterFont("ShareTech-Regular");
            images.RegisterImage("suit-clubs-512.png", "suit-clubs");
            images.RegisterImage("suit-diamonds-512.png", "suit-diamonds");
            images.RegisterImage("suit-hearts-512.png", "suit-hearts");
            images.RegisterImage("suit-spades-512.png", "suit-spades");

            target = Al.CreateBitmap(320, 240)
                ?? throw new InvalidOperationException("Could not create test target");
            Al.SetTargetBitmap(target);

            cache = new CardSpriteCache(fonts, images);
            CardStyle style = CardStyle.Default;

            AllegroBitmap front = cache.GetFront(new GameCard(7, Suit.Hearts), new Vector2(100, 140), style);
            AllegroBitmap back = cache.GetBack(new Vector2(100, 140), style);
            AllegroBitmap face = cache.GetFront(new GameCard(12, Suit.Spades), new Vector2(100, 140), style);

            Assert.NotNull(front);
            Assert.NotNull(back);
            Assert.NotNull(face);
            Assert.Equal(100, Al.GetBitmapWidth(front));
            Assert.Equal(140, Al.GetBitmapHeight(front));

            // Same key returns the cached bitmap instead of repainting it.
            Assert.Same(front, cache.GetFront(new GameCard(7, Suit.Hearts), new Vector2(100, 140), style));

            CardRenderer renderer = Service.Provider.GetRequiredService<CardRenderer>();
            CardView view = new("card view") {
                Value = new GameCard(12, Suit.Spades),
                Size = new Vector2(100, 140)
            };
            view.InitializeWithServices();
            Al.SetTargetBitmap(target);
            view.Render(new RenderContext());

            string output = Path.Combine(Path.GetTempPath(), "instapoker-card-sprites");
            Directory.CreateDirectory(output);
            Assert.True(Al.SaveBitmap(Path.Combine(output, "front.png"), front));
            Assert.True(Al.SaveBitmap(Path.Combine(output, "back.png"), back));
            Assert.True(Al.SaveBitmap(Path.Combine(output, "face.png"), face));
            Assert.True(Al.SaveBitmap(Path.Combine(output, "view.png"), target));
            Service.Provider.GetRequiredService<CardSpriteCache>().Dispose();
        }
        finally {
            cache?.Dispose();
            fonts?.Dispose();
            images?.Dispose();
            if (target is not null) {
                Al.DestroyBitmap(target);
            }
            Al.ShutdownImageAddon();
            Al.ShutdownPrimitivesAddon();
            Al.ShutdownTtfAddon();
            Al.UninstallSystem();
        }
    }
}
