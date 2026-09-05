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
        world.OnEntitySpawn += (args) =>
        {
            if (args.Entity is PlayerEntity player)
            {
                OnPlayerJoin?.Invoke(player);
            }
        };

        world.OnSoundPlay += (args) =>
        {
            foreach (PlayerEntity player in world.GetEntitiesOfType<PlayerEntity>())
            {
                if (args.IsGlobal)
                {
                    player.PlaySound(args.Sound);
                }
                else
                {
                    player.PlaySound(args.Sound, args.Position, args.Volume, args.ReferenceDistance, args.MaxDistance, args.RolloffFactor);
                }
            }
        };
    }

    public static event Action<PlayerEntity>? OnPlayerJoin;

    private static void World_OnBlockPlace((Block block, int x, int y, int z) obj)
    {
        SetBlockPacket blockPacket = new SetBlockPacket
        {
            X = obj.x,
            Y = obj.y,
            Z = obj.z,
            Type = obj.block.RegistryId
        };

        Packet packet = blockPacket.Write();

        int chunkX = (int)Math.Floor(obj.x / 16.0);
        int chunkY = (int)Math.Floor(obj.y / 16.0);
        int chunkZ = (int)Math.Floor(obj.z / 16.0);

        foreach (PlayerEntity player in world.GetEntities()
            .Where(o => o is PlayerEntity)
            .Cast<PlayerEntity>())
        {
            if (player.IsChunkLoaded(chunkX, chunkY, chunkZ))
            {
                player.Connection.SendPacket(packet);
            }
        }
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
                moveEntityPacket.X = serverEntity.Position.X;
                moveEntityPacket.Y = serverEntity.Position.Y;
                moveEntityPacket.Z = serverEntity.Position.Z;

                connection.SendPacket(moveEntityPacket.Write());
            }
        }
    }
}
