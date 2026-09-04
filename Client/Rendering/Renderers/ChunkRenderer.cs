using OpenTK.Graphics.OpenGL;

using Shared.Mathf;
using Shared.Worlds;

namespace Client.Rendering;

public class ChunkRenderer : Renderer
{
    public static List<ChunkRenderer> RequestingUpdate = new List<ChunkRenderer>(256);
    public static ChunkRenderer? PopNearest()
    {
        int count = RequestingUpdate.Count;
        if (count == 0) return null;
        if (count == 1)
        {
            ChunkRenderer singleChunk = RequestingUpdate[0];
            RequestingUpdate.Clear();
            return singleChunk;
        }

        Vector3 cameraPosition = Camera.Position;

        int nearestIndex = 0;
        float minSqrDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            float sqrDist = (RequestingUpdate[i].Chunk.Center - cameraPosition).Length;

            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                nearestIndex = i;
            }
        }

        ChunkRenderer nearestChunk = RequestingUpdate[nearestIndex];

        int lastIndex = count - 1;
        if (nearestIndex != lastIndex)
        {
            RequestingUpdate[nearestIndex] = RequestingUpdate[lastIndex];
        }
        RequestingUpdate.RemoveAt(lastIndex);

        return nearestChunk;
    }


    public Chunk Chunk { get; set; }
    public ChunkRenderer(Chunk chunk)
    {

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
        if (Chunk.isDirty)
        {
            Chunk.isDirty = false;
            RequestingUpdate.Add(this);
        }
    }

    public void Regenerate()
    {
        Chunk.Optimize();
        UpdateGeometry();
    }

    private void UpdateGeometry()
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