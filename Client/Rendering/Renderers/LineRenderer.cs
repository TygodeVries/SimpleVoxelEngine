using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Client.Rendering;

internal class LineRenderer : Renderer
{
    private List<Vector3> points = new List<Vector3>();
    private ShaderProgram shader;

    private int vao;
    private int vbo;
    private int maxSize = 0;

    public LineRenderer(ShaderProgram shaderProgram)
    {
        this.shader = shaderProgram;
        Setup();
    }

    private void Setup()
    {
        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void AddPoint(Vector3 point)
    {
        points.Add(point);
    }

    public void Clear()
    {
        points.Clear();
    }

    public Shared.Mathf.Vector3 position;

    public override Matrix4 GetModelMatrix()
    {
        return Matrix4.CreateTranslation(position.X, position.Y, position.Z) * Matrix4.CreateScale(1);
    }

    public override ShaderProgram? GetShader()
    {
        return shader;
    }

    public override void Render(bool isShadowPass)
    {
        if (points.Count < 2 || isShadowPass) return;

        int vertexCount = points.Count;
        if (vertexCount % 2 != 0)
        {
            vertexCount--;
        }

        Vector3[] rawData = points.ToArray();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

        int neededSize = vertexCount * Vector3.SizeInBytes;

        if (neededSize > maxSize)
        {
            GL.BufferData(BufferTarget.ArrayBuffer, neededSize, rawData, BufferUsage.DynamicDraw);
            maxSize = neededSize;
        }
        else
        {
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, neededSize, rawData);
        }

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.DrawArrays(PrimitiveType.Lines, 0, vertexCount);

        GL.Disable(EnableCap.Blend);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(vbo);
        GL.DeleteVertexArray(vao);
    }

    public void LoadCubeWireframe()
    {
        this.Clear();

        float low = -0.001f;
        float high = 1.001f;

        this.AddPoint(new Vector3(low, low, low)); this.AddPoint(new Vector3(high, low, low));
        this.AddPoint(new Vector3(high, low, low)); this.AddPoint(new Vector3(high, low, high));
        this.AddPoint(new Vector3(high, low, high)); this.AddPoint(new Vector3(low, low, high));
        this.AddPoint(new Vector3(low, low, high)); this.AddPoint(new Vector3(low, low, low));

        this.AddPoint(new Vector3(low, high, low)); this.AddPoint(new Vector3(high, high, low));
        this.AddPoint(new Vector3(high, high, low)); this.AddPoint(new Vector3(high, high, high));
        this.AddPoint(new Vector3(high, high, high)); this.AddPoint(new Vector3(low, high, high));
        this.AddPoint(new Vector3(low, high, high)); this.AddPoint(new Vector3(low, high, low));

        this.AddPoint(new Vector3(low, low, low)); this.AddPoint(new Vector3(low, high, low));
        this.AddPoint(new Vector3(high, low, low)); this.AddPoint(new Vector3(high, high, low));
        this.AddPoint(new Vector3(high, low, high)); this.AddPoint(new Vector3(high, high, high));
        this.AddPoint(new Vector3(low, low, high)); this.AddPoint(new Vector3(low, high, high));
    }
}