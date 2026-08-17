
using Shared.Mathf;
namespace Shared.Worlds;

public class RaycastHit
{
    public Chunk Chunk;
    public Vector3 ChunkBlockPos;
    public Vector3 WorldBlockPos;
    public Vector3 Normal;
    public short Block;

    public RaycastHit(Chunk chunk, Vector3 chunkBlockPos, Vector3 worldBlockPos, Vector3 normal, short block)
    {
        Normal = normal;
        Chunk = chunk;
        ChunkBlockPos = chunkBlockPos;
        WorldBlockPos = worldBlockPos;
        Block = block;
    }
}