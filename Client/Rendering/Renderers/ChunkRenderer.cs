using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Shared.Mathf;
using Shared.Worlds;

namespace Client.Rendering;

public class ChunkRenderer : Renderer
{
    public Chunk Chunk { get; set; }
    public ChunkRenderer(Chunk chunk)
    {
        this.Chunk = chunk;
        modelMatrix = Matrix4.CreateTranslation(chunk.X * 16, chunk.Y * 16, chunk.Z * 16);
    }

    private ChunkRenderType renderType = ChunkRenderType.Empty;

    public override void Render(bool isShadowPass)
    {
        bool allowedUpdateFrame = (Time.Frame + Chunk.X + (Time.Frame * 2) + Chunk.Z) % 4 == 1;

        if (Chunk.isDirty && !isShadowPass && allowedUpdateFrame)
        {
            UpdateRender();
            Chunk.Optimize();
        }


        RenderChunk(isShadowPass);
    }

    private void UpdateRender()
    {
        if (modelObjectGlId == -1)
        {
            modelObjectGlId = GL.GenVertexArray();
        }

        if (Chunk.GetChunkType() == Chunk.ChunkType.Single)
        {
            short blockType = Chunk.GetBlock(0, 0, 0);
            Block block = Registry.GetBlock(blockType);
            if (block == null || !block.isVisible)
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

        if (!isShadowPass)
        {
            RenderData.BlockTexture.Use(TextureUnit.Texture0);
        }

        GL.BindVertexArray(modelObjectGlId);
        GL.DrawElements(PrimitiveType.Triangles, indSize, DrawElementsType.UnsignedInt, 0);
    }

    public override ShaderProgram? GetShader()
    {
        if (renderType == ChunkRenderType.Solid)
        {
            return RenderData.SingleBlockShader;
        }

        return RenderData.DefaultShader;
    }

    private Matrix4 modelMatrix;
    public override Matrix4 GetModelMatrix()
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

