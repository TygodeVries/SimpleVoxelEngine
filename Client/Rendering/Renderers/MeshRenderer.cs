using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
namespace Client.Rendering;

public class MeshRenderer : Renderer
{
    private ShaderProgram shader;
    public MeshRenderer(ShaderProgram shader)
    {
        this.shader = shader;
    }

    public MeshRenderer(Mesh mesh, ShaderProgram shader)
    {
        this.shader = shader;
        this.mesh = mesh;
    }

    private int vao;
    private int vbo;
    private int ebo;

    private int nbo;
    private int uvbo;
    private int tbo;
    private Mesh? _mesh;  // backing field
    public Mesh? mesh
    {
        get => _mesh;
        set => SetMesh(value);
    }

    public void SetMesh(Mesh? mesh)
    {
        if (mesh != null)
            Upload(mesh);
        _mesh = mesh;
    }

    private int indexCount;

    private void Upload(Mesh mesh)
    {
        _mesh = mesh;
        if (mesh == null)
            return;
        // Delete old stuff
        if (vao != 0) GL.DeleteVertexArray(vao);
        if (vbo != 0) GL.DeleteBuffer(vbo);
        if (ebo != 0) GL.DeleteBuffer(ebo);
        if (tbo != 0) GL.DeleteBuffer(tbo);

        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, mesh.vertices!.Length * sizeof(float), mesh.vertices, BufferUsage.StaticDraw);

        ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.indices!.Length * sizeof(uint), mesh.indices, BufferUsage.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // Normal
        if (mesh.normals != null)
        {
            nbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, nbo);
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.normals.Length * sizeof(float), mesh.normals, BufferUsage.StaticDraw);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
        }

        // Uvs
        if (mesh.uvs != null)
        {
            uvbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvbo);
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.uvs.Length * sizeof(float), mesh.uvs, BufferUsage.StaticDraw);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(2);
        }

        GL.BindVertexArray(0);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);

        indexCount = mesh.indices.Length;
        totalTrisCount += indexCount / 3;
    }

    public static int totalTrisCount;
    public Texture? Texture { get; set; }
    public override void Render(bool isShadowPass)
    {
        if (mesh == null)
        {
            Console.WriteLine("Mesh renderer has no mesh!");
            return;
        }

        if (Texture != null)
        {
            shader.SetTextureId("u_Color", 0);
            Texture?.Use(TextureUnit.Texture0);
        }

        GL.BindVertexArray(vao);
        GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);
    }


    public void Dispose()
    {
        if (vao != 0) GL.DeleteVertexArray(vao);
        if (vbo != 0) GL.DeleteBuffer(vbo);
        if (ebo != 0) GL.DeleteBuffer(ebo);
        if (nbo != 0) GL.DeleteBuffer(nbo);
        if (uvbo != 0) GL.DeleteBuffer(uvbo);
        if (tbo != 0) GL.DeleteBuffer(tbo);

        vao = 0;
        vbo = 0;
        ebo = 0;
        nbo = 0;
        uvbo = 0;
        tbo = 0;
    }

    public override ShaderProgram? GetShader()
    {
        return shader;
    }

    private Matrix4 matrix;
    public void SetModelMatrix(Matrix4 matrix)
    {
        this.matrix = matrix;
    }

    public override Matrix4 GetModelMatrix()
    {
        return matrix;
    }
}
