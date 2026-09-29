using Server.Worlds.Entities;
using Shared.Networking;
using Shared.Worlds;

namespace Server.Worlds;

public class Multiverse
{
    // #TODO support mutliple worlds

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

        world.OnDropItem += (args) =>
        {
            ItemEntity itemEntity = new ItemEntity(args.itemStack);
            itemEntity.Teleport(args.position + new Shared.Mathf.Vector3(0.5f, 0.5f, 0.5f));
            itemEntity.SetVelocity(args.velocity);

            world.SpawnEntity(itemEntity);
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

    public static List<PlayerEntity> GetPlayers()
    {
        return world.GetEntitiesOfType<PlayerEntity>();
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
                serverEntity.SendToClient(connection);
            }
        }
    }
}
