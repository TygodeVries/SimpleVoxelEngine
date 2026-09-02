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
    private static List<float> aos = new List<float>();
    public static int GenerateMesh(Chunk chunk, out int indexSize)
    {
        verts.Clear();
        ind.Clear();
        uvs.Clear();
        normals.Clear();
        aos.Clear();


        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                for (int z = 0; z < 16; z++)
                {
                    short voxel = chunk.GetBlock(x, y, z);
                    Block? block = Registry.GetBlock(voxel);
                    if (block == null || !block.Visible)
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

        int aoVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, aoVbo);

        GL.BufferData(BufferTarget.ArrayBuffer, aos.Count * sizeof(float), aos.ToArray(), BufferUsage.StaticDraw);
        GL.EnableVertexAttribArray(3);

        GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, 0, 0);

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
        if (block == null || !block.Visible)
        {
            AddFace(x, y, z, dir, voxel, worldX, worldY, worldZ);
        }
    }

    private static void AddFace(int x, int y, int z, Vector3i normal, short voxel, int worldX, int worldY, int worldZ)
    {
        Vector3i world = new Vector3i(worldX, worldY, worldZ);
        BlockFace face = BlockFace.Up;

        if (normal == Vector3i.UnitY)
            face = BlockFace.Up;
        else if (normal == -Vector3i.UnitY)
            face = BlockFace.Down;
        else if (normal == Vector3i.UnitZ)
            face = BlockFace.Forward;
        else if (normal == -Vector3i.UnitZ)
            face = BlockFace.Backward;
        else if (normal == Vector3i.UnitX)
            face = BlockFace.Right;
        else if (normal == -Vector3i.UnitX)
            face = BlockFace.Left;

        Vector2[] uv =
            Shared.Mathf.Vector2.ArrayToOpenTK(
                RenderData.BlockTexturesMap.GetBlockTexture(
                    voxel,
                    face));

        if (uv.Length == 0)
            throw new Exception("No UV found for block.");

        Vector3 n = new(
            normal.X,
            normal.Y,
            normal.Z);


        if (normal.X > 0)
        {
            Vector3 v0 = new(x + 1, y, z);
            Vector3 v1 = new(x + 1, y + 1, z);
            Vector3 v2 = new(x + 1, y + 1, z + 1);
            Vector3 v3 = new(x + 1, y, z + 1);

            AddQuad(world,
                v0, v1, v2, v3,
                uv[3], uv[0], uv[1], uv[2],
                n);
        }

        else if (normal.X < 0)
        {
            Vector3 v0 = new(x, y, z);
            Vector3 v1 = new(x, y, z + 1);
            Vector3 v2 = new(x, y + 1, z + 1);
            Vector3 v3 = new(x, y + 1, z);

            AddQuad(world,
                v0, v1, v2, v3,
                uv[3], uv[2], uv[1], uv[0],
                n);
        }

        else if (normal.Y > 0)
        {
            Vector3 v0 = new(x, y + 1, z);
            Vector3 v1 = new(x, y + 1, z + 1);
            Vector3 v2 = new(x + 1, y + 1, z + 1);
            Vector3 v3 = new(x + 1, y + 1, z);

            AddQuad(world,
                v0, v1, v2, v3,
                uv[0], uv[3], uv[2], uv[1],
                n);
        }

        else if (normal.Y < 0)
        {
            Vector3 v0 = new(x, y, z);
            Vector3 v1 = new(x + 1, y, z);
            Vector3 v2 = new(x + 1, y, z + 1);
            Vector3 v3 = new(x, y, z + 1);

            AddQuad(world,
                v0, v1, v2, v3,
                uv[0], uv[1], uv[2], uv[3],
                n);
        }

        else if (normal.Z > 0)
        {
            Vector3 v0 = new(x, y, z + 1);
            Vector3 v1 = new(x + 1, y, z + 1);
            Vector3 v2 = new(x + 1, y + 1, z + 1);
            Vector3 v3 = new(x, y + 1, z + 1);

            AddQuad(world,
                v0, v1, v2, v3,
                uv[3], uv[2], uv[1], uv[0],
                n);
        }

        else
        {
            Vector3 v0 = new(x, y, z);
            Vector3 v1 = new(x, y + 1, z);
            Vector3 v2 = new(x + 1, y + 1, z);
            Vector3 v3 = new(x + 1, y, z);

            AddQuad(world,
                v0, v1, v2, v3,
                uv[3], uv[0], uv[1], uv[2],
                n);
        }
    }


    private static void AddQuad(
    Vector3i worldPos,
    Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3,
    Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
    Vector3 normal)
    {
        int x = worldPos.X;
        int y = worldPos.Y;
        int z = worldPos.Z;

        Vector3i u;
        Vector3i v;

        if (MathF.Abs(normal.X) > 0.5f)
        {
            u = new Vector3i(0, 1, 0);
            v = new Vector3i(0, 0, 1);
        }
        else if (MathF.Abs(normal.Y) > 0.5f)
        {
            u = new Vector3i(1, 0, 0);
            v = new Vector3i(0, 0, 1);
        }
        else
        {
            u = new Vector3i(1, 0, 0);
            v = new Vector3i(0, 1, 0);
        }

        Vector3i face = new Vector3i(
            (int)MathF.Round(normal.X),
            (int)MathF.Round(normal.Y),
            (int)MathF.Round(normal.Z)
        );

        Vector3i p = new Vector3i(x, y, z) + face;

        // Vertex 0
        bool v0Side1 = IsVisible(p + u);
        bool v0Side2 = IsVisible(p + v);
        bool v0Corner = IsVisible(p + u + v);

        float ao0 = VertexAO(v0Side1, v0Side2, v0Corner);

        // Vertex 1
        bool v1Side1 = IsVisible(p - u);
        bool v1Side2 = IsVisible(p + v);
        bool v1Corner = IsVisible(p - u + v);

        float ao1 = VertexAO(v1Side1, v1Side2, v1Corner);

        // Vertex 2
        bool v2Side1 = IsVisible(p - u);
        bool v2Side2 = IsVisible(p - v);
        bool v2Corner = IsVisible(p - u - v);

        float ao2 = VertexAO(v2Side1, v2Side2, v2Corner);

        // Vertex 3
        bool v3Side1 = IsVisible(p + u);
        bool v3Side2 = IsVisible(p - v);
        bool v3Corner = IsVisible(p + u - v);

        float ao3 = VertexAO(v3Side1, v3Side2, v3Corner);

        (float a0, float a1, float a2, float a3) map = (0, 0, 0, 0);

        if (normal.Y > 0.5f)
        {
            map = GetMapping(14, ao0, ao1, ao2, ao3);
        }

        if (normal.Y < -0.5f)
        {
            map = GetMapping(16, ao0, ao1, ao2, ao3);
        }

        if (normal.X > 0.5f)
        {
            map = GetMapping(16, ao0, ao1, ao2, ao3);
        }

        if (normal.X < -0.5f)
        {
            map = GetMapping(14, ao0, ao1, ao2, ao3);
        }

        if (normal.Z > 0.5f)
        {
            map = GetMapping(16, ao0, ao1, ao2, ao3);
        }

        if (normal.Z < -0.5f)
        {
            map = GetMapping(14, ao0, ao1, ao2, ao3);
        }


        if (map.a0 + map.a2 > map.a1 + map.a3)
        {
            AddTris(
                v0, v1, v2,
                uv0, uv1, uv2,
                map.a0, map.a1, map.a2,
                normal);

            AddTris(
                v2, v3, v0,
                uv2, uv3, uv0,
                map.a2, map.a3, map.a0,
                normal);
        }
        else
        {
            AddTris(
                v0, v1, v3,
                uv0, uv1, uv3,
                map.a0, map.a1, map.a3,
                normal);

            AddTris(
                v1, v2, v3,
                uv1, uv2, uv3,
                map.a1, map.a2, map.a3,
                normal);
        }

    }

    private static float VertexAO(bool side1, bool side2, bool corner)
    {
        if (side1 && side2)
            return 0;

        int ao = 3;

        if (side1) ao--;
        if (side2) ao--;
        if (corner) ao--;

        return ao / 3f;
    }

    public static int AOID = 0;

    public static (float, float, float, float) GetMapping(
    int id,
    float a, float b, float c, float d)
    {
        float[] v = { a, b, c, d };

        // 24 permutations of 4 values.
        id %= 24;
        if (id < 0)
            id += 24;

        return id switch
        {
            0 => (v[0], v[1], v[2], v[3]),
            1 => (v[0], v[1], v[3], v[2]),
            2 => (v[0], v[2], v[1], v[3]),
            3 => (v[0], v[2], v[3], v[1]),
            4 => (v[0], v[3], v[1], v[2]),
            5 => (v[0], v[3], v[2], v[1]),

            6 => (v[1], v[0], v[2], v[3]),
            7 => (v[1], v[0], v[3], v[2]),
            8 => (v[1], v[2], v[0], v[3]),
            9 => (v[1], v[2], v[3], v[0]),
            10 => (v[1], v[3], v[0], v[2]),
            11 => (v[1], v[3], v[2], v[0]),

            12 => (v[2], v[0], v[1], v[3]),
            13 => (v[2], v[0], v[3], v[1]),
            14 => (v[2], v[1], v[0], v[3]),
            15 => (v[2], v[1], v[3], v[0]),
            16 => (v[2], v[3], v[0], v[1]),
            17 => (v[2], v[3], v[1], v[0]),

            18 => (v[3], v[0], v[1], v[2]),
            19 => (v[3], v[0], v[2], v[1]),
            20 => (v[3], v[1], v[0], v[2]),
            21 => (v[3], v[1], v[2], v[0]),
            22 => (v[3], v[2], v[0], v[1]),
            23 => (v[3], v[2], v[1], v[0]),

            _ => throw new Exception()
        };
    }



    private static void AddTris(Vector3 a, Vector3 b, Vector3 c, Vector2 uvA, Vector2 uvB, Vector2 uvC, float aoA, float aoB, float aoC, Vector3 normal)
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

        aos.Add(aoA);
        aos.Add(aoB);
        aos.Add(aoC);
    }

    private static bool IsVisible(Vector3i pos)
    {


        Block? block = LocalWorld.World.GetBlockAt(pos.X, pos.Y, pos.Z);
        return block != null && block.Visible;
    }
}
