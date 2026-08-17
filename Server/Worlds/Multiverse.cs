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

    internal static void Start()
    {
        world.OnBlockPlace += World_OnBlockPlace;
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

    internal static void TickWorlds()
    {
        world.Tick();
    }

    internal static void SendWorldData(Connection connection, World world)
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
    }
}
