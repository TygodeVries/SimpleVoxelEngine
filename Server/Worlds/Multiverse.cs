using Server.Worlds.Entities;
using Shared.Networking;
using Shared.Worlds;

namespace Server.Worlds;

public class Multiverse
{
    public static List<World> worlds = new List<World>();

    public static World? GetWorld(int index)
    {
        if (index >= worlds.Count)
            return null;

        return worlds[index];
    }
    public static World CreateWorld(string name)
    {
        World world = new World(name);
        worlds.Add(world);

        world.OnBlockPlace += (args) =>
        {
            SetBlockPacket blockPacket = new SetBlockPacket
            {
                X = args.x,
                Y = args.y,
                Z = args.z,
                Type = args.block.RegistryId
            };

            Packet packet = blockPacket.Write();

            int chunkX = (int)Math.Floor(args.x / 16.0);
            int chunkY = (int)Math.Floor(args.y / 16.0);
            int chunkZ = (int)Math.Floor(args.z / 16.0);

            foreach (PlayerEntity player in world.GetEntitiesOfType<PlayerEntity>())
            {
                if (player.IsChunkLoaded(chunkX, chunkY, chunkZ))
                {
                    player.Connection.SendPacket(packet);
                }
            }
        };

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

        return world;
    }

    public static World? GetWorld(string name)
    {
        return worlds.FirstOrDefault(o =>
        {
            return o.Name == name;
        });
    }

    public static List<PlayerEntity> GetPlayers()
    {
        List<PlayerEntity> players = new List<PlayerEntity>();

        foreach (World world in worlds)
        {
            players.AddRange(world.GetEntitiesOfType<PlayerEntity>());
        }

        return players;
    }
    public static event Action<PlayerEntity>? OnPlayerJoin;

    internal static void TickWorlds()
    {
        foreach (World world in worlds)
        {
            world.Tick();

            foreach (Chunk chunk in world.GetChunks())
            {
                if (chunk.isDirty)
                {
                    chunk.Optimize();
                    chunk.isDirty = false;
                }
            }
        }
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
