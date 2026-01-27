namespace InstaPoker.Client.Graphics;

public class MultiplyFloatStack {
    private readonly Stack<float> values = new();

    public int Count => values.Count;

    public MultiplyFloatStack() => values.Push(1.0f);

    public void Pop() => values.Pop();

    public float Peek() => values.Peek();
    
    public void Push() {
        float top = values.Peek();
        values.Push(top);
    }

    public void Multiply(float value) {
        float top = values.Pop();
        top *= value;
        values.Push(top);
    }

    
}