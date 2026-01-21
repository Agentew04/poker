using System.Numerics;
using System.Text;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public class MatrixStack {

    private readonly Stack<Matrix4x4> matrices = [];

    public MatrixStack() {
        matrices.Push(Matrix4x4.Identity);
    }
    
    public void Pop() {
        if (matrices.Count > 1) {
            matrices.Pop();
        }
    }

    public void Push() {
        Matrix4x4 last = matrices.Peek();
        matrices.Push(last);
    }

    public void Multiply(in Matrix4x4 matrix) {
        Matrix4x4 last = matrices.Pop();
        last = last * matrix;
        matrices.Push(last);
    }

    public Matrix4x4 Peek()
    {
        return matrices.Peek();
    }

    public void AsTransform(ref AllegroTransform t) {
        Matrix4x4 m = matrices.Peek();
        
        t[0,0] = m.M11; t[0,1] = m.M12; t[0,2] = m.M13; t[0,3] = m.M14;
        t[1,0] = m.M21; t[1,1] = m.M22; t[1,2] = m.M23; t[1,3] = m.M24;
        t[2,0] = m.M31; t[2,1] = m.M32; t[2,2] = m.M33; t[2,3] = m.M34;
        t[3,0] = m.M41; t[3,1] = m.M42; t[3,2] = m.M43; t[3,3] = m.M44;
    }
}