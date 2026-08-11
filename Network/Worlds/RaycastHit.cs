using OpenTK.Mathematics;

namespace Shared.Worlds;

public class RaycastHit
{
    public Chunk Chunk;
    public Vector3i ChunkBlockPos;
    public Vector3i WorldBlockPos;
    public Vector3i Normal;
    public short Block;

    public RaycastHit(Chunk chunk, Vector3i chunkBlockPos, Vector3i worldBlockPos, Vector3i normal, short block)
    {
        Normal = normal;
        Chunk = chunk;
        ChunkBlockPos = chunkBlockPos;
        WorldBlockPos = worldBlockPos;
        Block = block;
    }
}