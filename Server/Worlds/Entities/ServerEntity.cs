
using Shared.Mathf;
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

    public void Teleport(Vector3 position)
    {
        this.position = position;
        MoveEntityPacket moveEntityPacket = new MoveEntityPacket()
        {
            Id = Id,
            X = position.X,
            Y = position.Y,
            Z = position.Z
        };

        Program.server.BroadcastPacket(moveEntityPacket.Write());
    }

}
