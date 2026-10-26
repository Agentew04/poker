using System.Numerics;
using ImGuiNET;
using InstaPoker.Client.Graphics.Objects;
using Microsoft.Extensions.Logging;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics.Layout;

public enum StackDirection {
    Horizontal,
    Vertical
}

public enum CrossAxisAlignment {
    Start,
    Center,
    End
}

public enum ChildAlignment {
    Edge,
    Pivot
}

/// <summary>
/// Lays out its children in a row or column, using <see cref="Spacing"/> between them and
/// <see cref="Padding"/> around the content area.
/// </summary>
public sealed class StackPanel(string name) : SceneObject(name) {

    public override bool UseMouse => true;
    public override bool UseKeyboard => true;

    /// <summary>The main axis of the stack.</summary>
    public StackDirection Direction { get; set; } = StackDirection.Vertical;

    /// <summary>Gap between consecutive visible children.</summary>
    public float Spacing { get; set; }

    /// <summary>Inset applied on all four sides of the panel.</summary>
    public float Padding { get; set; }

    /// <summary>How children are aligned on the axis perpendicular to <see cref="Direction"/>.</summary>
    public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;

    /// <summary>
    /// How children are aligned to the sides when using <see cref="CrossAxisAlignment"/> as
    /// <see cref="CrossAxisAlignment.Start"/> or <see cref="CrossAxisAlignment.End"/>.
    /// </summary>
    public ChildAlignment ChildAlignment { get; set; } = ChildAlignment.Pivot;
    
    public void Add(SceneObject child) {
        AddChild(child);
    }

    public override void PositionElements() {
        Vector2 cursor = new Vector2(Padding);
        Vector2 spacing = Direction == StackDirection.Vertical ? new Vector2(0, Spacing) : new Vector2(Spacing, 0);

        IReadOnlyList<SceneObject> list = GetChildren();
        for (int i = 0; i < list.Count; i++) {
            SceneObject child = list[i];
            if (!child.IsEnabled) {
                continue;
            }
            
            child.Measure();

            // child.Position = Vector2.Zero;

            if (child.Size.X <= 0 || child.Size.Y <= 0) {
                Logger.LogError("Child {child} of stack panel {stack} has empty size: {size}", child.Name, Name, child.Size);
            }
            
            child.Position = cursor;

            // cap cross axis
            if (Direction == StackDirection.Vertical 
                && child.Size.X > Size.X - Padding * 2) {
                Vector2 size = child.Size;
                size.X = Size.X - Padding * 2;
                child.Size = size;
                Logger.LogWarning("Child {child} of stack panel {stack} had overflowing width. Capping", child.Name, Name);
            }else if (Direction == StackDirection.Horizontal
                      && child.Size.Y > Size.Y - Padding * 2) {
                Vector2 size = child.Size;
                size.Y = Size.Y - Padding * 2;
                child.Size = size;
                Logger.LogWarning("Child {child} of stack panel {stack} had overflowing height. Capping", child.Name, Name);
            }
            
            // cap direction axis
            if (Direction == StackDirection.Vertical) {
                // completely outside
                if (cursor.Y > Size.Y) {
                    continue;
                }
                // partially outside, cap height
                if (cursor.Y + child.Size.Y > Size.Y - Padding) {
                    Vector2 size = child.Size;
                    size.Y -= cursor.Y + child.Size.Y - (Size.Y - Padding);
                    child.Size = size;
                }
            }else if (Direction == StackDirection.Horizontal) {
                // completely outside
                if (cursor.X > + Size.X) {
                    continue;
                }
                // partially outside, cap width 
                if (cursor.X + child.Size.X > Size.X - Padding) {
                    Vector2 size = child.Size;
                    size.X -= cursor.X + child.Size.X - (Size.X - Padding);
                    child.Size = size;
                }
            }
            
            
            // perform cross axis readjustment
            if (CrossAxisAlignment == CrossAxisAlignment.Start) {
                Bounds paddedBounds = new(new Vector2(Padding), Size - new Vector2(Padding));
                if (Direction == StackDirection.Vertical) {
                    if (ChildAlignment == ChildAlignment.Edge) {
                        child.Position = Layout.AlignLeftWith(child.Bounds, paddedBounds) + child.PivotOffset;
                    }else if (ChildAlignment == ChildAlignment.Pivot) {
                        child.Position = new Vector2(Padding, child.Position.Y);
                    }
                }else if (Direction == StackDirection.Horizontal) {
                    if (ChildAlignment == ChildAlignment.Edge) {
                        child.Position = Layout.AlignTopWith(child.Bounds, paddedBounds) + child.PivotOffset;
                    }else if (ChildAlignment == ChildAlignment.Pivot) {
                        child.Position = new Vector2(child.Position.X, Size.Y - Padding);
                    }
                }
            }else if (CrossAxisAlignment == CrossAxisAlignment.Center) {
                if (Direction == StackDirection.Vertical) {
                    child.Position = Layout.CenterHorizontallyIn(child.Bounds, Bounds) 
                                     - new Vector2(Position.X,0)
                                     + child.PivotOffset;
                }else if (Direction == StackDirection.Horizontal) {
                    child.Position = Layout.CenterVerticallyIn(child.Bounds, Bounds) 
                                     - new Vector2(0,Position.Y)
                                     + child.PivotOffset;
                }
            }else if (CrossAxisAlignment == CrossAxisAlignment.End) {
                Bounds paddedBounds = new(Vector2.Zero, Size - new Vector2(Padding));
                if (Direction == StackDirection.Vertical) {
                    if (ChildAlignment == ChildAlignment.Edge) {
                        child.Position = Layout.AlignRightWith(child.Bounds, paddedBounds) + child.PivotOffset;
                    }else if (ChildAlignment == ChildAlignment.Pivot) {
                        child.Position = new Vector2(Size.X-Padding, child.Position.Y);
                    }
                }else if (Direction == StackDirection.Horizontal) {
                    if (ChildAlignment == ChildAlignment.Edge) {
                        child.Position = Layout.AlignBottomWith(child.Bounds, paddedBounds) + child.PivotOffset;
                    }else if (ChildAlignment == ChildAlignment.Pivot) {
                        child.Position = new Vector2(child.Position.X, Size.Y-Padding);
                    }
                }
            }
            
            // increase for next
            cursor += new Vector2(Direction == StackDirection.Horizontal ? child.Size.X : 0,
                Direction == StackDirection.Vertical ? child.Size.Y : 0);
            cursor += spacing;
        }
    }

    public override void OnImGuiDraw() {
        ImGui.Text($"Pad: {Padding}  Space: {Spacing}");
        ImGui.Text($"Direction: {Direction}");
        ImGui.Text($"CrossAxisAlign: {CrossAxisAlignment}");
        ImGui.Text($"ChildAlign: {ChildAlignment}");
    }

    public override void OnDebugDraw(Bounds bounds) {
        AllegroColor blue = new() {
            R = 0,
            G = 0,
            B = 1,
            A = 1,
        };
        AllegroColor green = new() {
            R = 0,
            G = 1,
            B = 0,
            A = 1
        };
        
        // draw cross line gizmo
        if (Direction == StackDirection.Vertical) {
            if (CrossAxisAlignment == CrossAxisAlignment.Start) {
                Al.DrawLine(bounds.Left, bounds.Top, bounds.Left, bounds.Bottom, blue, 2);
            }else if (CrossAxisAlignment == CrossAxisAlignment.Center) {
                Al.DrawLine(bounds.Center.X, bounds.Top, bounds.Center.X, bounds.Bottom, blue, 2);
            }
            else {
                Al.DrawLine(bounds.Right, bounds.Top, bounds.Right, bounds.Bottom, blue, 2);
            }
        }else if (Direction == StackDirection.Horizontal) {
            if (CrossAxisAlignment == CrossAxisAlignment.Start) {
                Al.DrawLine(bounds.Left, bounds.Top, bounds.Right, bounds.Top, blue, 2);
            }else if (CrossAxisAlignment == CrossAxisAlignment.Center) {
                Al.DrawLine(bounds.Left, bounds.Center.Y, bounds.Right, bounds.Center.Y, blue, 2);
            }
            else {
                Al.DrawLine(bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom, blue, 2);
            }
        }
        
        // draw padding
        if (Padding > 0) {
            Al.DrawRectangle(bounds.Left + Padding, bounds.Top + Padding,
                bounds.Right - Padding, bounds.Bottom - Padding, green, 2);
        }
    }
}
