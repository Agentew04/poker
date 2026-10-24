using System.Numerics;
using InstaPoker.Client.Graphics.Objects;

namespace InstaPoker.Client.Graphics.Layout;

/// <summary>
/// Fluent placement for a <see cref="SceneObject"/>. All math is done by <see cref="Layout"/>; this type
/// only measures the objects when needed and applies the resulting top-left corner.
/// </summary>
public readonly struct Placement {
    private readonly SceneObject target;

    public Placement(SceneObject target) {
        this.target = target;
    }

    /// <summary>
    /// Places the target by its resolved top-left corner, regardless of its alignment.
    /// </summary>
    public Placement At(Vector2 topLeft) {
        target.Measure();
        target.PlaceAt(topLeft);
        return this;
    }

    /// <summary>
    /// Places the target by its resolved top-left corner, regardless of its alignment.
    /// </summary>
    public Placement At(float x, float y) => At(new Vector2(x, y));

    /// <summary>
    /// Sets the target anchor directly. For a centered label this is the center point, not the top-left.
    /// </summary>
    public Placement AtAnchor(Vector2 anchor) {
        target.Position = anchor;
        return this;
    }

    /// <summary>
    /// Sets the target anchor directly. For a centered label this is the center point, not the top-left.
    /// </summary>
    public Placement AtAnchor(float x, float y) => AtAnchor(new Vector2(x, y));

    /// <summary>
    /// Sets the target size explicitly. Objects that measure their own content (like <see cref="Label"/>)
    /// do not need this.
    /// </summary>
    public Placement WithSize(Vector2 size) {
        target.Size = size;
        return this;
    }

    /// <summary>
    /// Sets the target size explicitly. Objects that measure their own content (like <see cref="Label"/>)
    /// do not need this.
    /// </summary>
    public Placement WithSize(float width, float height) => WithSize(new Vector2(width, height));

    /// <summary>
    /// Positions the target on the left of the <paramref name="sibling"/> with a margin.
    /// Only changes the X coordinate of the resolved bounds.
    /// </summary>
    public Placement LeftOf(SceneObject sibling, float margin = 0) {
        target.Measure();
        sibling.Measure();
        target.PlaceAt(Layout.LeftOf(target.Bounds, sibling.Bounds, margin));
        return this;
    }

    /// <summary>
    /// Positions the target on the right of the <paramref name="sibling"/> with a margin.
    /// Only changes the X coordinate of the resolved bounds.
    /// </summary>
    public Placement RightOf(SceneObject sibling, float margin = 0) {
        target.Measure();
        sibling.Measure();
        target.PlaceAt(Layout.RightOf(target.Bounds, sibling.Bounds, margin));
        return this;
    }

    /// <summary>
    /// Positions the target on top of the <paramref name="sibling"/> with a margin.
    /// Only changes the Y coordinate of the resolved bounds.
    /// </summary>
    public Placement Above(SceneObject sibling, float margin = 0) {
        target.Measure();
        sibling.Measure();
        target.PlaceAt(Layout.Above(target.Bounds, sibling.Bounds, margin));
        return this;
    }

    /// <summary>
    /// Positions the target below the <paramref name="sibling"/> with a margin.
    /// Only changes the Y coordinate of the resolved bounds.
    /// </summary>
    public Placement Below(SceneObject sibling, float margin = 0) {
        target.Measure();
        sibling.Measure();
        target.PlaceAt(Layout.Below(target.Bounds, sibling.Bounds, margin));
        return this;
    }

    /// <summary>Aligns the left edges of the target and the sibling.</summary>
    public Placement AlignLeftWith(SceneObject sibling) {
        target.Measure();
        sibling.Measure();
        target.PlaceAt(Layout.AlignLeftWith(target.Bounds, sibling.Bounds));
        return this;
    }

    /// <summary>Aligns the right edges of the target and the sibling.</summary>
    public Placement AlignRightWith(SceneObject sibling) {
        target.Measure();
        sibling.Measure();
        target.PlaceAt(Layout.AlignRightWith(target.Bounds, sibling.Bounds));
        return this;
    }

    /// <summary>Aligns the top edges of the target and the sibling.</summary>
    public Placement AlignTopWith(SceneObject sibling) {
        target.Measure();
        sibling.Measure();
        target.PlaceAt(Layout.AlignTopWith(target.Bounds, sibling.Bounds));
        return this;
    }

    /// <summary>Aligns the bottom edges of the target and the sibling.</summary>
    public Placement AlignBottomWith(SceneObject sibling) {
        target.Measure();
        sibling.Measure();
        target.PlaceAt(Layout.AlignBottomWith(target.Bounds, sibling.Bounds));
        return this;
    }

    /// <summary>Aligns the target with the horizontal center of the sibling.</summary>
    public Placement AlignCenterXWith(SceneObject sibling) {
        target.Measure();
        sibling.Measure();
        target.PlaceAt(Layout.CenterHorizontallyWith(target.Bounds, sibling.Bounds));
        return this;
    }

    /// <summary>Aligns the target with the vertical center of the sibling.</summary>
    public Placement AlignCenterYWith(SceneObject sibling) {
        target.Measure();
        sibling.Measure();
        target.PlaceAt(Layout.CenterVerticallyWith(target.Bounds, sibling.Bounds));
        return this;
    }

    /// <summary>Centers the target inside the container.</summary>
    public Placement CenterIn(SceneObject container) {
        target.Measure();
        container.Measure();
        target.PlaceAt(Layout.CenterIn(target.Bounds, container.Bounds));
        return this;
    }

    /// <summary>Centers the target horizontally inside the container.</summary>
    public Placement CenterHorizontallyIn(SceneObject container) {
        target.Measure();
        container.Measure();
        target.PlaceAt(Layout.CenterHorizontallyIn(target.Bounds, container.Bounds));
        return this;
    }

    /// <summary>Centers the target vertically inside the container.</summary>
    public Placement CenterVerticallyIn(SceneObject container) {
        target.Measure();
        container.Measure();
        target.PlaceAt(Layout.CenterVerticallyIn(target.Bounds, container.Bounds));
        return this;
    }
}
