using Shared.Networking;
using Shared.Worlds;

namespace Server.Worlds;

public class Multiverse
{
    private static World world = new World();
    public static World GetMainWorld()
    {
        return world;
    }

    public static void Start()
    {
        world.OnBlockPlace += World_OnBlockPlace;
        world.OnAddChunk += World_OnAddChunk;


        for (int x = -3; x <= 3; x++)
        {
            for (int y = -3; y <= 3; y++)
            {
                for (int z = -3; z <= 3; z++)
                {
                    Chunk chunk = new Chunk(x, y, z);
                    if (y < 0)
                    {
                        chunk.Fill(1);
                    }
                    world.AddChunk(chunk);
                }
            }
        }
    }

    private static void World_OnAddChunk(Chunk chunk)
    {
        ChunkDataPacket chunkDataPacket = new ChunkDataPacket(chunk);
        Program.server.BroadcastPacket(chunkDataPacket.Write());
    }

    private static void World_OnBlockPlace((short type, int x, int y, int z) obj)
    {
        PlaceBlockPacket packet = new PlaceBlockPacket();
        packet.X = obj.x;
        packet.Y = obj.y;
        packet.Z = obj.z;
        packet.Type = obj.type;

        Program.server.BroadcastPacket(packet.Write());
    }

    public static void TickWorlds()
    {
        world.Tick();
    }

    public static void SendWorldData(Connection connection, World world)
    {
        foreach (Entity entity in world.GetEntities())
        {
            if (entity is ServerEntity serverEntity)
            {
                SpawnEntityPacket spawnEntityPacket = new SpawnEntityPacket();
                spawnEntityPacket.Id = serverEntity.Id;
                spawnEntityPacket.Type = serverEntity.GetEntityType();

                connection.SendPacket(spawnEntityPacket.Write());

                MoveEntityPacket moveEntityPacket = new MoveEntityPacket();
                moveEntityPacket.Id = serverEntity.Id;
                moveEntityPacket.X = serverEntity.position.X;
                moveEntityPacket.Y = serverEntity.position.Y;
                moveEntityPacket.Z = serverEntity.position.Z;

                connection.SendPacket(moveEntityPacket.Write());
            }
        }

        foreach (Chunk chunk in world.GetChunks())
        {
            ChunkDataPacket chunkDataPacket = new ChunkDataPacket(chunk);
            connection.SendPacket(chunkDataPacket.Write());
        }
    }
}
