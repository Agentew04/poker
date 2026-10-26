using System.Numerics;
using InstaPoker.Client.Graphics.Layout;
using InstaPoker.Client.Graphics.Objects;
using Xunit;

namespace InstaPoker.Tests;

public class StackPanelTests {

    private sealed class TestNode(string name) : SceneObject(name) {
        public override bool UseMouse => false;
        public override bool UseKeyboard => false;

        public Vector2? MeasuredSize { get; set; }

        public override void Measure() {
            if (MeasuredSize is not null) {
                Size = MeasuredSize.Value;
            }
        }
    }

    private static TestNode Node(float width, float height, bool enabled = true) {
        return new TestNode("node") {
            Size = new Vector2(width, height),
            IsEnabled = enabled
        };
    }

    private static StackPanel Panel(StackDirection direction, float width, float height,
        float padding = 0, float spacing = 0) {
        return new StackPanel("panel") {
            Direction = direction,
            Size = new Vector2(width, height),
            Padding = padding,
            Spacing = spacing
        };
    }

    [Fact]
    public void Vertical_places_children_in_order_and_applies_spacing() {
        StackPanel panel = Panel(StackDirection.Vertical, 200, 300, padding: 10, spacing: 5);
        TestNode first = Node(50, 20);
        TestNode second = Node(60, 30);
        TestNode third = Node(70, 40);
        panel.Add(first);
        panel.Add(second);
        panel.Add(third);

        panel.PositionElements();

        Assert.Equal(new Vector2(10, 10), first.Bounds.Position);
        Assert.Equal(new Vector2(10, 35), second.Bounds.Position);
        Assert.Equal(new Vector2(10, 70), third.Bounds.Position);
    }

    [Fact]
    public void Horizontal_places_children_in_order_and_applies_spacing() {
        StackPanel panel = Panel(StackDirection.Horizontal, 300, 200, padding: 10, spacing: 5);
        TestNode first = Node(20, 50);
        TestNode second = Node(30, 60);
        TestNode third = Node(40, 70);
        panel.Add(first);
        panel.Add(second);
        panel.Add(third);

        panel.PositionElements();

        Assert.Equal(new Vector2(10, 10), first.Bounds.Position);
        Assert.Equal(new Vector2(35, 10), second.Bounds.Position);
        Assert.Equal(new Vector2(70, 10), third.Bounds.Position);
    }

    [Fact]
    public void Padding_is_applied_on_both_axes() {
        StackPanel panel = Panel(StackDirection.Horizontal, 200, 100, padding: 7);
        TestNode child = Node(20, 20);
        panel.Add(child);

        panel.PositionElements();

        Assert.Equal(new Vector2(7, 7), child.Bounds.Position);
    }

    [Fact]
    public void CrossAxisAlignment_center_centers_children_on_the_cross_axis() {
        StackPanel panel = Panel(StackDirection.Vertical, 200, 300, padding: 10);
        panel.CrossAxisAlignment = CrossAxisAlignment.Center;
        TestNode narrow = Node(60, 20);
        TestNode wide = Node(100, 20);
        panel.Add(narrow);
        panel.Add(wide);

        panel.PositionElements();

        Assert.Equal(new Vector2(70, 10), narrow.Bounds.Position);
        Assert.Equal(new Vector2(50, 30), wide.Bounds.Position);
    }

    [Fact]
    public void CrossAxisAlignment_end_aligns_children_to_the_far_edge() {
        StackPanel panel = Panel(StackDirection.Vertical, 200, 300, padding: 10);
        panel.CrossAxisAlignment = CrossAxisAlignment.End;
        TestNode child = Node(60, 20);
        panel.Add(child);

        panel.PositionElements();

        Assert.Equal(new Vector2(130, 10), child.Bounds.Position);
    }

    [Fact]
    public void Horizontal_cross_axis_center_and_end_use_the_panel_height() {
        StackPanel centered = Panel(StackDirection.Horizontal, 200, 100, padding: 10);
        centered.CrossAxisAlignment = CrossAxisAlignment.Center;
        TestNode centerChild = Node(20, 20);
        centered.Add(centerChild);
        centered.PositionElements();
        Assert.Equal(new Vector2(10, 40), centerChild.Bounds.Position);

        StackPanel end = Panel(StackDirection.Horizontal, 200, 100, padding: 10);
        end.CrossAxisAlignment = CrossAxisAlignment.End;
        TestNode endChild = Node(20, 20);
        end.Add(endChild);
        end.PositionElements();
        Assert.Equal(new Vector2(10, 70), endChild.Bounds.Position);
    }

    [Fact]
    public void Disabled_children_are_skipped_and_do_not_take_space() {
        StackPanel panel = Panel(StackDirection.Vertical, 200, 300, padding: 10, spacing: 5);
        TestNode first = Node(50, 20);
        TestNode hidden = Node(50, 100, enabled: false);
        TestNode third = Node(50, 20);
        panel.Add(first);
        panel.Add(hidden);
        panel.Add(third);

        panel.PositionElements();

        Assert.Equal(new Vector2(10, 10), first.Bounds.Position);
        Assert.Equal(new Vector2(10, 35), third.Bounds.Position);
    }

    [Fact]
    public void Children_are_measured_before_their_size_is_used() {
        StackPanel panel = Panel(StackDirection.Vertical, 200, 300, padding: 10, spacing: 5);
        TestNode measured = new("measured") {
            MeasuredSize = new Vector2(40, 30)
        };
        TestNode next = Node(40, 20);
        panel.Add(measured);
        panel.Add(next);

        panel.PositionElements();

        Assert.Equal(new Vector2(10, 10), measured.Bounds.Position);
        Assert.Equal(new Vector2(10, 45), next.Bounds.Position);
    }

    [Fact]
    public void Empty_panel_does_not_throw() {
        StackPanel panel = Panel(StackDirection.Vertical, 200, 300);

        panel.PositionElements();
    }

    [Fact]
    public void Layout_is_idempotent() {
        StackPanel panel = Panel(StackDirection.Horizontal, 300, 200, padding: 10, spacing: 5);
        TestNode first = Node(20, 30);
        TestNode second = Node(40, 50);
        panel.Add(first);
        panel.Add(second);

        panel.PositionElements();
        Vector2 firstPosition = first.Bounds.Position;
        Vector2 secondPosition = second.Bounds.Position;
        panel.PositionElements();

        Assert.Equal(firstPosition, first.Bounds.Position);
        Assert.Equal(secondPosition, second.Bounds.Position);
    }
}
