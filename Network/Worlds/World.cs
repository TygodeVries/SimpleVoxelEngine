namespace Shared.Worlds;

using Shared.Mathf;

public class World
{
    public World()
    {

    }

    public WorldGenerator WorldGenerator { get; private set; } = new DefaultWorldGenerator();

    public void SetWorldGenerator(WorldGenerator worldGenerator)
    {
        this.WorldGenerator = worldGenerator;
    }

    public void GenerateChunk(int x, int y, int z)
    {
        Chunk chunk = new Chunk(x, y, z);
        for (int bx = 0; bx < 16; bx++)
            for (int by = 0; by < 16; by++)
                for (int bz = 0; bz < 16; bz++)
                {
                    int worldX = (x * 16) + bx;
                    int worldY = (y * 16) + by;
                    int worldZ = (z * 16) + bz;
                    chunk.SetBlock(WorldGenerator.Generate(worldX, worldY, worldZ), bx, by, bz);
                }

        chunk.isDirty = true;

        AddChunk(chunk);
    }

    public Chunk GetOrGenerateChunkAt(int x, int y, int z)
    {
        if (chunks.TryGetValue((x, y, z), out Chunk chunk))
        {
            return chunk;
        }
        else
        {
            GenerateChunk(x, y, z);
            return chunks[(x, y, z)];
        }
    }

    private List<Entity> entities = new List<Entity>();
    private List<Entity> futureEntities = new List<Entity>();
    private List<Entity> graveYard = new List<Entity>();
    public void SpawnEntity(Entity entity, int forceId = -1)
    {
        this.futureEntities.Add(entity);
        entity.SetWorld(this);

        if (forceId == -1)
        {
            entity.SetId(IdCount);
            IdCount++;
        }
        else
        {
            entity.SetId(forceId);
            IdCount = forceId + 1;
        }
    }

    public List<Entity> GetEntities()
    {
        return entities;
    }

    public Entity? GetEntityWithId(int id)
    {
        return entities.FirstOrDefault((e) =>
        {
            return e.Id == id;
        });
    }

    private int IdCount = 0;
    public void DestroyEntity(Entity entity)
    {
        graveYard.Add(entity);
    }

    public void Tick()
    {
        foreach (Entity entity in entities)
        {
            entity.Tick();
        }

        foreach (Entity entity in graveYard)
        {
            entity.OnDestroy();
            entities.Remove(entity);
        }

        graveYard.Clear();

        foreach (Entity entity in futureEntities)
        {
            entity.OnSpawn();
            entities.Add(entity);
        }

        futureEntities.Clear();
    }

    public RaycastHit? Raycast(Vector3 position, Vector3 direction, float maxDistance = 5)
    {
        Vector3 pointer = position;
        Vector3 lastVoxel = pointer.Floor();

        while (Vector3.Distance(pointer, position) < maxDistance)
        {
            int voxelX = (int)MathF.Floor(pointer.X);
            int voxelY = (int)MathF.Floor(pointer.Y);
            int voxelZ = (int)MathF.Floor(pointer.Z);

            int chunkX = FloorDiv(voxelX, 16);
            int chunkY = FloorDiv(voxelY, 16);
            int chunkZ = FloorDiv(voxelZ, 16);

            int blockX = Mod(voxelX, 16);
            int blockY = Mod(voxelY, 16);
            int blockZ = Mod(voxelZ, 16);

            Vector3 currentVoxel = new Vector3(voxelX, voxelY, voxelZ);

            if (chunks.TryGetValue((chunkX, chunkY, chunkZ), out Chunk? chunk) && chunk != null)
            {
                short block = chunk.GetBlock(blockX, blockY, blockZ);
                if (!BlockData.IsInvisible(block))
                {
                    Vector3 delta = currentVoxel - lastVoxel;
                    Vector3 normal = new Vector3(-delta.X, -delta.Y, -delta.Z);

                    if (normal.EuclideanLengthSquared > 1)
                    {
                        if (MathF.Abs(delta.X) >= MathF.Abs(delta.Y) && MathF.Abs(delta.X) >= MathF.Abs(delta.Z))
                            normal = new Vector3(-MathF.Sign(delta.X), 0, 0);
                        else if (MathF.Abs(delta.Y) >= MathF.Abs(delta.X) && MathF.Abs(delta.Y) >= MathF.Abs(delta.Z))
                            normal = new Vector3(0, -MathF.Sign(delta.Y), 0);
                        else
                            normal = new Vector3(0, 0, -MathF.Sign(delta.Z));
                    }

                    return new RaycastHit(
                        chunk,
                        new Vector3(blockX, blockY, blockZ),
                        currentVoxel,
                        normal,
                        block
                    );
                }
            }

            lastVoxel = currentVoxel;
            pointer += direction / 100;
        }

        return null;
    }

    private Dictionary<(int x, int y, int z), Chunk> chunks = new Dictionary<(int x, int y, int z), Chunk>();

    public void AddChunk(Chunk chunk)
    {
        var cords = (chunk.X, chunk.Y, chunk.Z);
        if (chunks.ContainsKey(cords))
        {
            Console.WriteLine("Chunk already loaded!? #TODO");
            chunks[cords] = chunk;
            return;
        }
        else
        {
            chunks.Add(cords, chunk);
        }
        OnAddChunk?.Invoke(chunk);
    }

    public event Action<Chunk>? OnAddChunk;
    public event Action<Chunk>? OnRemoveChunk;

    public void RemoveChunk(Chunk chunk)
    {
        var cords = (chunk.X, chunk.Y, chunk.Z);
        chunks.Remove(cords);
        OnRemoveChunk?.Invoke(chunk);
    }

    public short GetBlockAt(int x, int y, int z)
    {
        int chunkX = FloorDiv(x, 16);
        int chunkY = FloorDiv(y, 16);
        int chunkZ = FloorDiv(z, 16);

        int blockX = Mod(x, 16);
        int blockY = Mod(y, 16);
        int blockZ = Mod(z, 16);

        if (chunks.TryGetValue((chunkX, chunkY, chunkZ), out Chunk? chunk))
        {
            if (chunk == null)
                return 0;

            return chunk.GetBlock(blockX, blockY, blockZ);
        }
        else
        {
            return 0;
        }
    }

    private static int FloorDiv(int a, int b)
    {
        return (int)Math.Floor((double)a / b);
    }

    private static int Mod(int a, int b)
    {
        if (b == 0) return 0;
        return ((a % b) + b) % b;
    }

    public event Action<(short type, int x, int y, int z)>? OnBlockPlace;
    public void SetBlockAt(short block, int x, int y, int z)
    {
        OnBlockPlace?.Invoke((block, x, y, z));

        int chunkX = FloorDiv(x, 16);
        int chunkY = FloorDiv(y, 16);
        int chunkZ = FloorDiv(z, 16);

        int blockX = Mod(x, 16);
        int blockY = Mod(y, 16);
        int blockZ = Mod(z, 16);

        var targetKey = (chunkX, chunkY, chunkZ);
        if (chunks.TryGetValue(targetKey, out Chunk? targetChunk) && targetChunk != null)
        {
            targetChunk.SetBlock(block, blockX, blockY, blockZ);
            targetChunk.isDirty = true;
        }
        else
        {
            return;
        }

        if (blockX == 0) MarkChunkDirty(chunkX - 1, chunkY, chunkZ);
        if (blockX == 15) MarkChunkDirty(chunkX + 1, chunkY, chunkZ);

        if (blockY == 0) MarkChunkDirty(chunkX, chunkY - 1, chunkZ);
        if (blockY == 15) MarkChunkDirty(chunkX, chunkY + 1, chunkZ);

        if (blockZ == 0) MarkChunkDirty(chunkX, chunkY, chunkZ - 1);
        if (blockZ == 15) MarkChunkDirty(chunkX, chunkY, chunkZ + 1);
    }

    private void MarkChunkDirty(int cx, int cy, int cz)
    {
        if (chunks.TryGetValue((cx, cy, cz), out Chunk? chunk) && chunk != null)
        {
            chunk.isDirty = true;
        }
    }

    public void FillSquare(short type, Vector3 center, Vector3 size)
    {
        int halfX = size.iX / 2;
        int halfY = size.iY / 2;
        int halfZ = size.iZ / 2;

        for (int x = center.iX - halfX; x <= center.X + halfX; x++)
        {
            for (int y = center.iY - halfY; y <= center.Y + halfY; y++)
            {
                for (int z = center.iZ - halfZ; z <= center.Z + halfZ; z++)
                {
                    SetBlockAt(type, x, y, z);
                }
            }
        }
    }

    public void FillSphere(short type, Vector3 center, float radius)
    {
        int r = (int)MathF.Ceiling(radius);
        float radiusSquared = radius * radius;

        for (int x = center.iX - r; x <= center.X + r; x++)
        {
            for (int y = center.iY - r; y <= center.Y + r; y++)
            {
                for (int z = center.iZ - r; z <= center.Z + r; z++)
                {
                    float dx = x - center.X;
                    float dy = y - center.Y;
                    float dz = z - center.Z;

                    if ((dx * dx) + (dy * dy) + (dz * dz) <= radiusSquared)
                    {
                        SetBlockAt(type, x, y, z);
                    }
                }
            }
        }
    }

    public void FillLine(short type, Vector3 start, Vector3 end, int radius)
    {
        Vector3 delta = new Vector3(
            end.X - start.X,
            end.Y - start.Y,
            end.Z - start.Z);

        float length = delta.Length;

        if (length == 0)
        {
            FillSphere(type, start, radius);
            return;
        }

        Vector3 direction = delta / length;

        int steps = (int)MathF.Ceiling(length);

        for (int i = 0; i <= steps; i++)
        {
            Vector3 pos = new Vector3(
                start.X,
                start.Y,
                start.Z) + (direction * i);

            FillSphere(
                type,
                new Vector3(
                    (int)MathF.Round(pos.X),
                    (int)MathF.Round(pos.Y),
                    (int)MathF.Round(pos.Z)),
                radius);
        }
    }

    public List<Entity> GetEntitiesNear(Vector3 position, float radius)
    {
        List<Entity> e = new List<Entity>();

        foreach (Entity entity in entities)
        {
            if (Vector3.Distance(entity.position, position) < radius)
            {
                e.Add(entity);
            }
        }

        return e;
    }

    public List<Chunk> GetChunks()
    {
        return chunks.Values.ToList();
    }
}