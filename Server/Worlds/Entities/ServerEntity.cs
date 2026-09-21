using Shared.Networking;
using Shared.Worlds;

namespace Server.Worlds;

public abstract class ServerEntity : Entity
{
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

        Console.WriteLine($"Sending packet for type {GetEntityType().Name}");
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

        Teleport(Position);

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
