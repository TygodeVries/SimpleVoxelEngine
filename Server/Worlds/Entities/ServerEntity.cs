using Shared.Networking;
using Shared.Worlds;

namespace Server.Worlds;

public abstract class ServerEntity : Entity
{
    public abstract EntityType GetEntityType();
    public ServerEntity()
    {

    }

    public override void OnSpawn()
    {
        SpawnEntityPacket spawnEntityPacket = new SpawnEntityPacket()
        {
            Id = Id,
            Type = GetEntityType()
        };

        Program.server.BroadcastPacket(spawnEntityPacket.Write());

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
