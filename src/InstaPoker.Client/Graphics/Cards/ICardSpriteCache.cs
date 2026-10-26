using System.Numerics;
using InstaPoker.Core.Games;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Cards;

/// <summary>
/// Caches rendered card faces. Rendering a face is expensive, so it is done once per
/// (card, size, style, face) and reused for every frame/animation.
/// </summary>
public interface ICardSpriteCache : IDisposable {

    AllegroBitmap GetFront(GameCard card, Vector2 size, CardStyle style);

    AllegroBitmap GetBack(Vector2 size, CardStyle style);
}
