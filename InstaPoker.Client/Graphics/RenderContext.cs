using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public class RenderContext {
    public MatrixStack Stack { get; set; } = new();
    public AllegroTransform Transform = new();
    public MultiplyFloatStack AlphaStack { get; set; } = new();
    public float Alpha => AlphaStack.Peek();
    


    public void UpdateTransform() {
        Stack.AsTransform(ref Transform);
        Al.UseTransform(ref Transform);
    }
}