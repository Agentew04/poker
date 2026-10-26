using System.Numerics;
using InstaPoker.Core.Games;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Cards;

public sealed class CardSpriteCache : ICardSpriteCache {

    private readonly CardFacePainter painter;
    private readonly Dictionary<SpriteKey, AllegroBitmap> sprites = [];

    public CardSpriteCache(FontManager fonts, ImageManager images) {
        painter = new CardFacePainter(fonts, images);
    }

    public AllegroBitmap GetFront(GameCard card, Vector2 size, CardStyle style) {
        SpriteKey key = CreateKey(card, size, style, back: false);
        if (sprites.TryGetValue(key, out AllegroBitmap? sprite)) {
            return sprite;
        }

        sprite = CreateSprite(key.Width, key.Height,
            () => painter.PaintFront(card, new Vector2(key.Width, key.Height), style));
        sprites[key] = sprite;
        return sprite;
    }

    public AllegroBitmap GetBack(Vector2 size, CardStyle style) {
        SpriteKey key = CreateKey(default, size, style, back: true);
        if (sprites.TryGetValue(key, out AllegroBitmap? sprite)) {
            return sprite;
        }

        sprite = CreateSprite(key.Width, key.Height,
            () => painter.PaintBack(new Vector2(key.Width, key.Height), style));
        sprites[key] = sprite;
        return sprite;
    }

    private static SpriteKey CreateKey(GameCard card, Vector2 size, CardStyle style, bool back) {
        int width = Math.Max(1, (int)MathF.Round(size.X));
        int height = Math.Max(1, (int)MathF.Round(size.Y));
        return new SpriteKey(width, height, back ? default : card, back, style);
    }

    private static AllegroBitmap CreateSprite(int width, int height, Action paint) {
        AllegroBitmap bitmap = Al.CreateBitmap(width, height)
            ?? throw new InvalidOperationException($"Could not create a {width}x{height} card sprite");

        AllegroBitmap? previousTarget = Al.GetTargetBitmap();
        try {
            Al.SetTargetBitmap(bitmap);
            Al.SetBlender(BlendOperation.Add, BlendMode.Alpha, BlendMode.InverseAlpha);
            Al.ResetClippingRectangle();
            paint();
            return bitmap;
        }
        catch {
            Al.DestroyBitmap(bitmap);
            throw;
        }
        finally {
            Al.SetTargetBitmap(previousTarget);
            Al.ResetClippingRectangle();
        }
    }

    public void Dispose() {
        foreach (AllegroBitmap sprite in sprites.Values) {
            Al.DestroyBitmap(sprite);
        }
        sprites.Clear();
    }

    private readonly record struct SpriteKey(
        int Width,
        int Height,
        GameCard Card,
        bool Back,
        CardStyle Style);
}
