using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Shared.Worlds;

namespace Client.Rendering;

public class ChunkMesher
{
    private static List<float> verts = new List<float>();
    private static List<int> ind = new List<int>();
    private static List<float> uvs = new List<float>();
    private static List<float> normals = new List<float>();
    public static int GenerateMesh(Chunk chunk, out int indexSize)
    {
        verts.Clear();
        ind.Clear();
        uvs.Clear();
        normals.Clear();


        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                for (int z = 0; z < 16; z++)
                {
                    short voxel = chunk.GetBlock(x, y, z);
                    Block? block = Registry.GetBlock(voxel);
                    if (block == null || !block.isVisible)
                        continue;

                    TryAddSide(x, y, z, chunk, Vector3i.UnitX, voxel);
                    TryAddSide(x, y, z, chunk, -Vector3i.UnitX, voxel);
                    TryAddSide(x, y, z, chunk, Vector3i.UnitY, voxel);
                    TryAddSide(x, y, z, chunk, -Vector3i.UnitY, voxel);
                    TryAddSide(x, y, z, chunk, Vector3i.UnitZ, voxel);
                    TryAddSide(x, y, z, chunk, -Vector3i.UnitZ, voxel);
                }
            }
        }

        indexSize = ind.Count;
        return CreateGlMesh();
    }

    private static int CreateGlMesh()
    {
        int vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        // Positions
        int positionVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, positionVbo);
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            verts.Count * sizeof(float),
            verts.ToArray(),
            BufferUsage.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            0,
            0);

        // UVs
        int uvVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, uvVbo);
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            uvs.Count * sizeof(float),
            uvs.ToArray(),
            BufferUsage.StaticDraw);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(
            1,
            2,
            VertexAttribPointerType.Float,
            false,
            0,
            0);

        // Normals
        int normalVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, normalVbo);
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            normals.Count * sizeof(float),
            normals.ToArray(),
            BufferUsage.StaticDraw);

        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(
            2,
            3,
            VertexAttribPointerType.Float,
            false,
            0,
            0);

        // Indices
        int ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(
            BufferTarget.ElementArrayBuffer,
            ind.Count * sizeof(int),
            ind.ToArray(),
            BufferUsage.StaticDraw);

        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        return vao;
    }

    private static void TryAddSide(int x, int y, int z, Chunk chunk, Vector3i dir, short voxel)
    {
        int worldX = (chunk.X * 16) + x;
        int worldY = (chunk.Y * 16) + y;
        int worldZ = (chunk.Z * 16) + z;

        Block block = LocalWorld.World.GetBlockAt(worldX + dir.X, worldY + dir.Y, worldZ + dir.Z);
        if (block == null || !block.isVisible)
        {
            AddFace(x, y, z, dir, voxel);
        }
    }

    private static void AddFace(int x, int y, int z, Vector3i normal, short voxel)
    {
        BlockFace face = BlockFace.Up;

        if (normal.X == 0 && normal.Y == 1 && normal.Z == 0)
            face = BlockFace.Up;

        if (normal.X == 0 && normal.Y == -1 && normal.Z == 0)
            face = BlockFace.Down;

        if (normal.X == 0 && normal.Y == 0 && normal.Z == 1)
            face = BlockFace.Forward;

        if (normal.X == 0 && normal.Y == 0 && normal.Z == -1)
            face = BlockFace.Backward;

        if (normal.X == 1 && normal.Y == 0 && normal.Z == 0)
            face = BlockFace.Right;

        if (normal.X == -1 && normal.Y == 0 && normal.Z == 0)
            face = BlockFace.Left;

        Vector2[] uv = RenderData.BlockTexturesMap.GetBlockTexture(voxel, face);
        if (uv.Length == 0)
        {
            throw new Exception("No UV found for block.");
        }

        Vector3 faceNormal = new(normal.X, normal.Y, normal.Z);

        if (normal.X != 0)
        {
            float nx = normal.X > 0 ? 1f : 0f;

            Vector3 v0 = new(x + nx, y + 1, z);
            Vector3 v1 = new(x + nx, y, z);
            Vector3 v2 = new(x + nx, y, z + 1);
            Vector3 v3 = new(x + nx, y + 1, z + 1);

            AddTris(v0, v1, v2, uv[0], uv[3], uv[2], faceNormal);
            AddTris(v2, v3, v0, uv[2], uv[1], uv[0], faceNormal);
        }
        else if (normal.Y != 0)
        {
            float ny = normal.Y > 0 ? 1f : 0f;

            Vector3 v0 = new(x, y + ny, z);
            Vector3 v1 = new(x + 1, y + ny, z);
            Vector3 v2 = new(x + 1, y + ny, z + 1);
            Vector3 v3 = new(x, y + ny, z + 1);

            AddTris(v0, v1, v2, uv[0], uv[1], uv[2], faceNormal);
            AddTris(v2, v3, v0, uv[2], uv[3], uv[0], faceNormal);
        }
        else
        {
            float nz = normal.Z > 0 ? 1f : 0f;

            Vector3 v0 = new(x, y + 1, z + nz);
            Vector3 v1 = new(x + 1, y + 1, z + nz);
            Vector3 v2 = new(x + 1, y, z + nz);
            Vector3 v3 = new(x, y, z + nz);

            AddTris(v0, v1, v2, uv[0], uv[1], uv[2], faceNormal);
            AddTris(v2, v3, v0, uv[2], uv[3], uv[0], faceNormal);
        }
    }

    private static void AddTris(Vector3 a, Vector3 b, Vector3 c, Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector3 normal)
    {

        verts.Add(a.X);
        verts.Add(a.Y);
        verts.Add(a.Z);

        verts.Add(b.X);
        verts.Add(b.Y);
        verts.Add(b.Z);

        verts.Add(c.X);
        verts.Add(c.Y);
        verts.Add(c.Z);

        ind.Add(ind.Count); ind.Add(ind.Count); ind.Add(ind.Count);

        uvs.Add(uvA.X); uvs.Add(uvA.Y);
        uvs.Add(uvB.X); uvs.Add(uvB.Y);
        uvs.Add(uvC.X); uvs.Add(uvC.Y);


        for (int i = 0; i < 3; i++)
        {
            normals.Add(normal.X);
            normals.Add(normal.Y);
            normals.Add(normal.Z);
        }
    }
}
