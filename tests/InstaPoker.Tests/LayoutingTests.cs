using System.Numerics;
using InstaPoker.Client.Graphics.Layout;
using InstaPoker.Client.Graphics.Objects;
using Xunit;

namespace InstaPoker.Tests;

public class BoundsTests {

    [Fact]
    public void Exposes_edges_and_center() {
        Bounds bounds = new(new Vector2(10, 20), new Vector2(100, 50));

        Assert.Equal(10, bounds.Left);
        Assert.Equal(110, bounds.Right);
        Assert.Equal(20, bounds.Top);
        Assert.Equal(70, bounds.Bottom);
        Assert.Equal(new Vector2(60, 45), bounds.Center);
    }

    [Fact]
    public void A_zero_sized_bounds_has_its_center_at_the_position() {
        Bounds bounds = new(new Vector2(7, 9), Vector2.Zero);

        Assert.Equal(new Vector2(7, 9), bounds.Center);
        Assert.Equal(7, bounds.Right);
        Assert.Equal(9, bounds.Bottom);
    }
}

public class LayoutMathTests {

    [Fact]
    public void RightOf_uses_the_anchor_right_edge_plus_margin_and_keeps_the_target_y() {
        Bounds target = new(new Vector2(0, 100), new Vector2(50, 20));
        Bounds anchor = new(new Vector2(10, 20), new Vector2(30, 40));

        Vector2 position = Layout.RightOf(target, anchor, margin: 5);

        Assert.Equal(new Vector2(45, 100), position);
    }

    [Fact]
    public void CenterVerticallyWith_centers_the_target_on_the_anchor_and_keeps_the_target_x() {
        Bounds target = new(new Vector2(7, 9), new Vector2(50, 20));
        Bounds anchor = new(new Vector2(10, 20), new Vector2(30, 40));

        Vector2 position = Layout.CenterVerticallyWith(target, anchor);

        // anchor center Y = 40, target height = 20 -> Y = 30
        Assert.Equal(new Vector2(7, 30), position);
    }

    [Fact]
    public void CenterIn_centers_the_target_inside_the_container_including_its_origin() {
        Bounds target = new(new Vector2(7, 9), new Vector2(50, 20));
        Bounds container = new(new Vector2(10, 20), new Vector2(200, 100));

        Vector2 position = Layout.CenterIn(target, container);

        // 10 + (200-50)/2 = 85 | 20 + (100-20)/2 = 60
        Assert.Equal(new Vector2(85, 60), position);
    }

    private static Bounds Target() => new(new Vector2(7, 9), new Vector2(50, 20));
    private static Bounds Anchor() => new(new Vector2(100, 200), new Vector2(30, 40));

    [Fact]
    public void LeftOf_uses_the_anchor_left_edge_minus_margin_and_keeps_the_target_y() {
        Assert.Equal(new Vector2(45, 9), Layout.LeftOf(Target(), Anchor(), margin: 5));
    }

    [Fact]
    public void RightOf_with_a_non_zero_target_y_keeps_that_y() {
        Assert.Equal(new Vector2(135, 9), Layout.RightOf(Target(), Anchor(), margin: 5));
    }

    [Fact]
    public void Above_uses_the_anchor_top_edge_minus_margin_and_keeps_the_target_x() {
        Assert.Equal(new Vector2(7, 175), Layout.Above(Target(), Anchor(), margin: 5));
    }

    [Fact]
    public void Below_uses_the_anchor_bottom_edge_plus_margin_and_keeps_the_target_x() {
        Assert.Equal(new Vector2(7, 245), Layout.Below(Target(), Anchor(), margin: 5));
    }

    [Fact]
    public void AlignLeftWith_lines_up_the_left_edges() {
        Assert.Equal(new Vector2(100, 9), Layout.AlignLeftWith(Target(), Anchor()));
    }

    [Fact]
    public void AlignRightWith_lines_up_the_right_edges() {
        Assert.Equal(new Vector2(80, 9), Layout.AlignRightWith(Target(), Anchor()));
    }

    [Fact]
    public void AlignTopWith_lines_up_the_top_edges() {
        Assert.Equal(new Vector2(7, 200), Layout.AlignTopWith(Target(), Anchor()));
    }

    [Fact]
    public void AlignBottomWith_lines_up_the_bottom_edges() {
        Assert.Equal(new Vector2(7, 220), Layout.AlignBottomWith(Target(), Anchor()));
    }

    [Fact]
    public void CenterHorizontallyWith_centers_x_on_the_anchor_and_keeps_y() {
        Assert.Equal(new Vector2(90, 9), Layout.CenterHorizontallyWith(Target(), Anchor()));
    }

    [Fact]
    public void CenterVerticallyWith_centers_y_on_the_anchor_and_keeps_x() {
        Assert.Equal(new Vector2(7, 210), Layout.CenterVerticallyWith(Target(), Anchor()));
    }

    [Fact]
    public void CenterHorizontallyIn_centers_x_inside_the_container_and_keeps_y() {
        Bounds container = new(new Vector2(10, 20), new Vector2(200, 100));

        Assert.Equal(new Vector2(85, 9), Layout.CenterHorizontallyIn(Target(), container));
    }

    [Fact]
    public void CenterVerticallyIn_centers_y_inside_the_container_and_keeps_x() {
        Bounds container = new(new Vector2(10, 20), new Vector2(200, 100));

        Assert.Equal(new Vector2(7, 60), Layout.CenterVerticallyIn(Target(), container));
    }
}

public class PlacementTests {

    private sealed class Node(string name) : SceneObject(name) {
        public override bool UseMouse => false;
        public override bool UseKeyboard => false;
    }

    private sealed class CenteredNode : SceneObject {
        public override bool UseMouse => false;
        public override bool UseKeyboard => false;

        public CenteredNode(string name) : base(name) {
            HorizontalAlign = InstaPoker.Client.Graphics.Styles.HorizontalAlign.Center;
            VerticalAlign = InstaPoker.Client.Graphics.Styles.VerticalAlign.Center;
        }
    }

    private sealed class AutoSizeNode(string name) : SceneObject(name) {
        public override bool UseMouse => false;
        public override bool UseKeyboard => false;

        public Vector2 DesiredSize { get; set; } = new(50, 20);
        public bool WasMeasured { get; private set; }

        public override void Measure() {
            WasMeasured = true;
            Size = DesiredSize;
        }
    }

    private static Node CreateNode(float x, float y, float width, float height) {
        return new Node("node") {
            Position = new Vector2(x, y),
            Size = new Vector2(width, height)
        };
    }

    private static (Node Target, Node Sibling) CreatePair() {
        return (
            CreateNode(7, 9, 50, 20),
            CreateNode(100, 200, 30, 40)
        );
    }

    [Fact]
    public void Bounds_reflects_the_object_position_and_size() {
        Node node = CreateNode(7, 9, 50, 20);

        Assert.Equal(new Vector2(7, 9), node.Bounds.Position);
        Assert.Equal(new Vector2(50, 20), node.Bounds.Size);
    }

    [Fact]
    public void At_sets_the_position() {
        Node node = CreateNode(7, 9, 50, 20);

        new Placement(node).At(new Vector2(11, 13));

        Assert.Equal(new Vector2(11, 13), node.Position);
    }

    [Fact]
    public void WithSize_sets_the_size_and_keeps_the_position() {
        Node node = CreateNode(7, 9, 50, 20);

        node.Place().WithSize(new Vector2(80, 30));

        Assert.Equal(new Vector2(80, 30), node.Size);
        Assert.Equal(new Vector2(7, 9), node.Position);
    }

    [Fact]
    public void RightOf_places_the_target_after_the_sibling_with_a_margin_and_keeps_y() {
        (Node target, Node sibling) = CreatePair();

        target.Place().WithSize(new Vector2(50, 20)).RightOf(sibling, margin: 5);

        // sibling.Right = 130 + 5 = 135, y is untouched
        Assert.Equal(new Vector2(135, 9), target.Position);
    }

    [Fact]
    public void LeftOf_places_the_target_before_the_sibling_with_a_margin_and_keeps_y() {
        (Node target, Node sibling) = CreatePair();

        target.Place().WithSize(new Vector2(50, 20)).LeftOf(sibling, margin: 5);

        // sibling.Left = 100 - 5 - target.Width 50 = 45
        Assert.Equal(new Vector2(45, 9), target.Position);
    }

    [Fact]
    public void Above_places_the_target_above_the_sibling_with_a_margin_and_keeps_x() {
        (Node target, Node sibling) = CreatePair();

        target.Place().WithSize(new Vector2(50, 20)).Above(sibling, margin: 5);

        // sibling.Top = 200 - 5 - target.Height 20 = 175
        Assert.Equal(new Vector2(7, 175), target.Position);
    }

    [Fact]
    public void Below_places_the_target_below_the_sibling_with_a_margin_and_keeps_x() {
        (Node target, Node sibling) = CreatePair();

        target.Place().WithSize(new Vector2(50, 20)).Below(sibling, margin: 5);

        // sibling.Bottom = 240 + 5 = 245
        Assert.Equal(new Vector2(7, 245), target.Position);
    }

    [Fact]
    public void AlignLeftWith_matches_the_sibling_left_edge() {
        (Node target, Node sibling) = CreatePair();

        target.Place().WithSize(new Vector2(50, 20)).AlignLeftWith(sibling);

        Assert.Equal(new Vector2(100, 9), target.Position);
    }

    [Fact]
    public void AlignRightWith_matches_the_sibling_right_edge() {
        (Node target, Node sibling) = CreatePair();

        target.Place().WithSize(new Vector2(50, 20)).AlignRightWith(sibling);

        // sibling.Right = 130, target.Width = 50 -> x = 80
        Assert.Equal(new Vector2(80, 9), target.Position);
    }

    [Fact]
    public void AlignTopWith_matches_the_sibling_top_edge() {
        (Node target, Node sibling) = CreatePair();

        target.Place().WithSize(new Vector2(50, 20)).AlignTopWith(sibling);

        Assert.Equal(new Vector2(7, 200), target.Position);
    }

    [Fact]
    public void AlignBottomWith_matches_the_sibling_bottom_edge() {
        (Node target, Node sibling) = CreatePair();

        target.Place().WithSize(new Vector2(50, 20)).AlignBottomWith(sibling);

        // sibling.Bottom = 240, target.Height = 20 -> y = 220
        Assert.Equal(new Vector2(7, 220), target.Position);
    }

    [Fact]
    public void AlignCenterXWith_matches_the_sibling_horizontal_center() {
        (Node target, Node sibling) = CreatePair();

        target.Place().WithSize(new Vector2(50, 20)).AlignCenterXWith(sibling);

        // sibling center X = 115, target.Width = 50 -> x = 90
        Assert.Equal(new Vector2(90, 9), target.Position);
    }

    [Fact]
    public void AlignCenterYWith_matches_the_sibling_vertical_center() {
        (Node target, Node sibling) = CreatePair();

        target.Place().WithSize(new Vector2(50, 20)).AlignCenterYWith(sibling);

        // sibling center Y = 220, target.Height = 20 -> y = 210
        Assert.Equal(new Vector2(7, 210), target.Position);
    }

    [Fact]
    public void CenterIn_centers_the_target_inside_the_container() {
        Node target = CreateNode(7, 9, 50, 20);
        Node container = CreateNode(10, 20, 200, 100);

        target.Place().WithSize(new Vector2(50, 20)).CenterIn(container);

        Assert.Equal(new Vector2(85, 60), target.Position);
    }

    [Fact]
    public void CenterHorizontallyIn_centers_x_and_keeps_y() {
        Node target = CreateNode(7, 9, 50, 20);
        Node container = CreateNode(10, 20, 200, 100);

        target.Place().WithSize(new Vector2(50, 20)).CenterHorizontallyIn(container);

        Assert.Equal(new Vector2(85, 9), target.Position);
    }

    [Fact]
    public void CenterVerticallyIn_centers_y_and_keeps_x() {
        Node target = CreateNode(7, 9, 50, 20);
        Node container = CreateNode(10, 20, 200, 100);

        target.Place().WithSize(new Vector2(50, 20)).CenterVerticallyIn(container);

        Assert.Equal(new Vector2(7, 60), target.Position);
    }

    [Fact]
    public void Chained_placements_compose_on_the_expected_axes() {
        (Node target, Node sibling) = CreatePair();

        target.Place()
            .WithSize(new Vector2(50, 20))
            .RightOf(sibling, margin: 5)   // x = 135
            .AlignCenterYWith(sibling);    // y = 210

        Assert.Equal(new Vector2(135, 210), target.Position);
    }

    [Fact]
    public void The_same_placement_can_be_applied_twice_without_accumulating() {
        (Node target, Node sibling) = CreatePair();
        Placement placement = target.Place().WithSize(new Vector2(50, 20));

        placement.RightOf(sibling, margin: 5).AlignCenterYWith(sibling);
        Vector2 first = target.Position;

        placement.RightOf(sibling, margin: 5).AlignCenterYWith(sibling);

        Assert.Equal(first, target.Position);
    }

    [Fact]
    public void WithSize_is_applied_before_centering() {
        Node target = CreateNode(7, 9, 999, 999);
        Node container = CreateNode(10, 20, 200, 100);

        target.Place().WithSize(new Vector2(50, 20)).CenterIn(container);

        Assert.Equal(new Vector2(50, 20), target.Size);
        Assert.Equal(new Vector2(85, 60), target.Position);
    }

    [Fact]
    public void Bounds_resolves_a_centered_anchor_to_the_top_left() {
        CenteredNode node = new("centered") { Position = new Vector2(100, 100), Size = new Vector2(50, 20) };

        Assert.Equal(new Vector2(75, 90), node.Bounds.Position);
        Assert.Equal(new Vector2(100, 100), node.Position);
    }

    [Fact]
    public void At_places_the_resolved_top_left_even_for_a_centered_object() {
        CenteredNode node = new("centered") { Size = new Vector2(50, 20) };

        node.Place().At(new Vector2(10, 20));

        Assert.Equal(new Vector2(35, 30), node.Position);
        Assert.Equal(new Vector2(10, 20), node.Bounds.Position);
    }

    [Fact]
    public void AtAnchor_sets_the_pivot_position_directly() {
        CenteredNode node = new("centered") { Size = new Vector2(50, 20) };

        node.Place().AtAnchor(new Vector2(100, 100));

        Assert.Equal(new Vector2(100, 100), node.Position);
        Assert.Equal(new Vector2(75, 90), node.Bounds.Position);
    }

    [Fact]
    public void RightOf_accounts_for_a_centered_target_pivot() {
        CenteredNode target = new("centered") { Position = new Vector2(0, 0), Size = new Vector2(50, 20) };
        Node sibling = CreateNode(100, 200, 30, 40);

        target.Place().RightOf(sibling, margin: 5);

        // resolved top-left x = 135; the centered anchor is shifted right by half the width
        Assert.Equal(new Vector2(160, 0), target.Position);
        Assert.Equal(new Vector2(135, -10), target.Bounds.Position);
    }

    [Fact]
    public void Relative_placement_uses_the_resolved_bounds_of_a_centered_sibling() {
        CenteredNode sibling = new("centered sibling") {
            Position = new Vector2(500, 425),
            Size = new Vector2(100, 50)
        };
        CenteredNode target = new("centered target") { Size = new Vector2(100, 50) };

        target.Place().LeftOf(sibling, margin: 20).AlignCenterYWith(sibling);

        Assert.Equal(new Vector2(330, 400), target.Bounds.Position);
        Assert.Equal(new Vector2(380, 425), target.Position);
    }

    [Fact]
    public void Placement_measures_content_sized_targets_without_WithSize() {
        AutoSizeNode target = new("auto") { DesiredSize = new Vector2(50, 20) };
        Node sibling = CreateNode(100, 200, 30, 40);

        target.Place().RightOf(sibling, margin: 5).AlignCenterYWith(sibling);

        Assert.True(target.WasMeasured);
        Assert.Equal(new Vector2(50, 20), target.Size);
        Assert.Equal(new Vector2(135, 210), target.Position);
    }
}
