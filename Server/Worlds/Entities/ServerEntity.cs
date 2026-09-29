using Shared.Networking;
using Shared.Worlds;

namespace Server.Worlds;

public abstract class ServerEntity : Entity
{
    public ServerEntity()
    {

    }

    private Dictionary<string, string> metadata = new Dictionary<string, string>();
    public void SetMetadata(string key, string value)
    {
        metadata[key] = value;

        if (isSpawned)
        {
            EntityMetadataPacket entityMetadataPacket = new EntityMetadataPacket()
            {
                Id = Id,
                Key = key,
                Value = metadata[key]
            };

            Program.server.BroadcastPacket(entityMetadataPacket.Write());
        }
    }

    public void SendToClient(Connection connection)
    {
        SpawnEntityPacket spawnEntityPacket = new SpawnEntityPacket()
        {
            Id = Id,
            Type = GetEntityType()
        };

        connection.SendPacket(spawnEntityPacket.Write());

        MoveEntityPacket moveEntityPacket = new MoveEntityPacket()
        {
            Id = Id,
            X = Position.X,
            Y = Position.Y,
            Z = Position.Z
        };

        connection.SendPacket(moveEntityPacket.Write());

        foreach (string key in metadata.Keys)
        {
            EntityMetadataPacket entityMetadataPacket = new EntityMetadataPacket()
            {
                Id = Id,
                Key = key,
                Value = metadata[key]
            };

            connection.SendPacket(entityMetadataPacket.Write());
        }
    }

    protected bool isSpawned = false;
    public override void OnSpawn()
    {
        isSpawned = true;
        // When we move, send a packet
        OnTeleport += () =>
        {
            MoveEntityPacket moveEntityPacket = new MoveEntityPacket()
            {
                Id = Id,
                X = Position.X,
                Y = Position.Y,
                Z = Position.Z
            };

            Program.server.BroadcastPacket(moveEntityPacket.Write());
        };

        Teleport(Position);

        foreach (PlayerEntity player in GetWorld().GetEntitiesOfType<PlayerEntity>())
        {
            SendToClient(player.Connection);
        }

        base.OnSpawn();
    }

    public override void OnDestroy()
    {
        DestroyEntityPacket destroyEntityPacket = new DestroyEntityPacket()
        {
            Id = Id
        };

        Program.server.BroadcastPacket(destroyEntityPacket.Write());

        base.OnDestroy();
    }
}
