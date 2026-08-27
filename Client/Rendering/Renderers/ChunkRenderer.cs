using OpenTK.Graphics.OpenGL;

using Shared.Mathf;
using Shared.Worlds;

namespace Client.Rendering;

public class ChunkRenderer : Renderer
{
    public Chunk Chunk { get; set; }
    public Vector3 Center;
    public Vector3 Size;
    public ChunkRenderer(Chunk chunk)
    {
        Center = new Vector3(
            (chunk.X * 16) + 8,
            (chunk.Y * 16) + 8,
            (chunk.Z * 16) + 8);

        Size = new Vector3(16, 16, 16);

        this.Chunk = chunk;
        modelMatrix = OpenTK.Mathematics.Matrix4.CreateTranslation(chunk.X * 16, chunk.Y * 16, chunk.Z * 16);
    }

    public ChunkRenderType renderType = ChunkRenderType.Empty;

    public override void Render(bool isShadowPass)
    {
        RenderChunk(isShadowPass);
    }

    public void Update()
    {
        bool allowedUpdateFrame = (Time.Frame + Chunk.X + (Time.Frame * 2) + Chunk.Z) % 4 == 1;

        if (Chunk.isDirty && allowedUpdateFrame)
        {
            UpdateGeo();
            Chunk.Optimize();
        }
    }

    private void UpdateGeo()
    {
        if (modelObjectGlId == -1)
        {
            modelObjectGlId = GL.GenVertexArray();
        }

        if (Chunk.GetChunkType() == Chunk.ChunkType.Single)
        {
            short blockType = Chunk.GetBlock(0, 0, 0);
            Block block = Registry.GetBlock(blockType);
            if (block == null || !block.Visible)
            {
                renderType = ChunkRenderType.Empty;
                return;
            }
            else
            {
                indSize = RenderData.SolidIndexCount;
                RenderData.MakeSingleChunk(modelObjectGlId, blockType);
                renderType = ChunkRenderType.Solid;
            }
        }
        else
        {
            if (modelObjectGlId != -1)
            {
                GL.DeleteVertexArray(modelObjectGlId);
                modelObjectGlId = -1;
            }

            modelObjectGlId = ChunkMesher.GenerateMesh(Chunk, out indSize);

            if (modelObjectGlId == -1)
            {
                renderType = ChunkRenderType.Empty;
            }
            else
            {
                renderType = ChunkRenderType.Normal;
            }
        }

        Chunk.isDirty = false;
    }

    private int modelObjectGlId = -1;
    private int indSize = 0;
    private void RenderChunk(bool isShadowPass)
    {

        // If there is only air, render nothing!
        if (renderType == ChunkRenderType.Empty)
            return;

        GL.BindVertexArray(modelObjectGlId);
        GL.DrawElements(PrimitiveType.Triangles, indSize, DrawElementsType.UnsignedInt, 0);
    }

    public override ShaderProgram? GetShader()
    {
        return null;
    }

    private OpenTK.Mathematics.Matrix4 modelMatrix;
    public override OpenTK.Mathematics.Matrix4 GetModelMatrix()
    {
        return modelMatrix;
    }
}

public enum ChunkRenderType
{
    Empty,
    Solid,
    Normal
}

