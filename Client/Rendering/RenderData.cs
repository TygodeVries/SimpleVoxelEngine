using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Client.Rendering;

public class RenderData
{
    private static int solidChunkVertex;
    private static int solidChunkUv;
    private static int solidChunkNormal;
    private static int solidChunkIndex;
    public const int SolidIndexCount = 36;


    public static Texture? BlockTexture { get; private set; }
    public static TextureMap? BlockTexturesMap { get; private set; }

    public static ShaderProgram? DefaultShader { get; private set; }
    public static ShaderProgram? SingleBlockShader { get; private set; }
    public static ShaderProgram? DepthShader { get; private set; }
    public static ShaderProgram? UIShader { get; private set; }
    public static void SetupDefaults()
    {
        BlockTexture = ImageTexture.LoadFromPng("Textures/Blocks.png");

        BlockTexturesMap = new TextureMap((ImageTexture)BlockTexture);

        BlockTexturesMap.AddMapping(1, 1, BlockFace.Up);
        BlockTexturesMap.AddMapping(2, 1, BlockFace.Side);
        BlockTexturesMap.AddMapping(3, 1, BlockFace.Down);


        DefaultShader = new ShaderProgram(
            File.ReadAllText("Shaders/default.vert"),
            File.ReadAllText("Shaders/default.frag"));

        UIShader = new ShaderProgram(
            File.ReadAllText("Shaders/ui.vert"),
            File.ReadAllText("Shaders/ui.frag"));

        SingleBlockShader = new ShaderProgram(
            File.ReadAllText("Shaders/default.vert"),
            File.ReadAllText("Shaders/single.frag"));

        SingleBlockShader.SetVector4("u_TextureInfo", new Vector4(BlockTexturesMap.row, BlockTexturesMap.col, 16, 0));

        DepthShader = new ShaderProgram(
            File.ReadAllText("Shaders/shadow.vert"),
            File.ReadAllText("Shaders/shadow.frag"));

        DefaultShader.SetTextureId("u_Color", 0);

        CreateSolidChunkVertexBuffer();
        CreateSolidChunkNormalBuffer();
        CreateSolidChunkIndexBuffer();
        CreateSolidChunkUvBuffer();
    }

    private static void CreateSolidChunkVertexBuffer()
    {
        float[] vertices =
        {
            // +X
            16,0,0,  16,16,0,  16,16,16,  16,0,16,

            // -X
            0,0,16,  0,16,16,  0,16,0,  0,0,0,

            // +Y
            0,16,0,  0,16,16,  16,16,16,  16,16,0,

            // -Y
            0,0,16,  0,0,0,  16,0,0,  16,0,16,

            // +Z
            0,0,16,  16,0,16,  16,16,16,  0,16,16,

            // -Z
            16,0,0,  0,0,0,  0,16,0,  16,16,0
        };

        solidChunkVertex = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, solidChunkVertex);
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            vertices.Length * sizeof(float),
            vertices,
            BufferUsage.StaticDraw);
    }

    private static void CreateSolidChunkNormalBuffer()
    {
        float[] normals =
        {
            // +X
            1,0,0, 1,0,0, 1,0,0, 1,0,0,

            // -X
            -1,0,0, -1,0,0, -1,0,0, -1,0,0,

            // +Y
            0,1,0, 0,1,0, 0,1,0, 0,1,0,

            // -Y
            0,-1,0, 0,-1,0, 0,-1,0, 0,-1,0,

            // +Z
            0,0,1, 0,0,1, 0,0,1, 0,0,1,

            // -Z
            0,0,-1, 0,0,-1, 0,0,-1, 0,0,-1
        };

        solidChunkNormal = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, solidChunkNormal);
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            normals.Length * sizeof(float),
            normals,
            BufferUsage.StaticDraw);
    }

    private static void CreateSolidChunkIndexBuffer()
    {
        int[] indices =
        {
            0,1,2, 2,3,0,
            4,5,6, 6,7,4,
            8,9,10, 10,11,8,
            12,13,14, 14,15,12,
            16,17,18, 18,19,16,
            20,21,22, 22,23,20
        };

        solidChunkIndex = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, solidChunkIndex);
        GL.BufferData(
            BufferTarget.ElementArrayBuffer,
            indices.Length * sizeof(int),
            indices,
            BufferUsage.StaticDraw);
    }

    private static void CreateSolidChunkUvBuffer()
    {
        solidChunkUv = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, solidChunkUv);

        GL.BufferData(
            BufferTarget.ArrayBuffer,
            24 * 2 * sizeof(float),
            IntPtr.Zero,
            BufferUsage.DynamicDraw);
    }

    public static void MakeSingleChunk(int vao, short blockType)
    {
        Vector2[] topFace = BlockTexturesMap.GetBlockTexture(blockType, BlockFace.Up);

        float[] uv = new float[24 * 2];

        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
        {
            int offset = faceIndex * 8;

            uv[offset + 0] = topFace[0].X;
            uv[offset + 1] = topFace[0].Y;

            uv[offset + 2] = topFace[1].X;
            uv[offset + 3] = topFace[1].Y;

            uv[offset + 4] = topFace[2].X;
            uv[offset + 5] = topFace[2].Y;

            uv[offset + 6] = topFace[3].X;
            uv[offset + 7] = topFace[3].Y;
        }


        GL.BindVertexArray(vao);


        // Position
        GL.BindBuffer(BufferTarget.ArrayBuffer, solidChunkVertex);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            0,
            0);


        // Create unique UV buffer for this VAO
        int uvBuffer = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, uvBuffer);
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            uv.Length * sizeof(float),
            uv,
            BufferUsage.StaticDraw);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(
            1,
            2,
            VertexAttribPointerType.Float,
            false,
            0,
            0);


        // Normal
        GL.BindBuffer(BufferTarget.ArrayBuffer, solidChunkNormal);
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(
            2,
            3,
            VertexAttribPointerType.Float,
            false,
            0,
            0);


        // Index
        GL.BindBuffer(
            BufferTarget.ElementArrayBuffer,
            solidChunkIndex);


        GL.BindVertexArray(0);
    }
}
