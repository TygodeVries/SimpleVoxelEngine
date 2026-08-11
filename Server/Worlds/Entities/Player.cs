using Shared.Networking;
using Shared.Worlds;

namespace Server.Worlds;

public class Player : ServerEntity
{
    public Connection Connection;
    public Player(Connection connection)
    {
        this.Connection = connection;

        connection.OnPacket += OnPlayerPacket;
    }

    private void OnPlayerPacket(Packet packet)
    {
        if (packet.GetPacketType() == PacketType.PlayerMove)
        {
            PlayerMovePacket playerMovePacket = new PlayerMovePacket();
            playerMovePacket.Read(packet);

            position.X = playerMovePacket.X;
            position.Y = playerMovePacket.Y;
            position.Z = playerMovePacket.Z;

            Teleport(position);
        }

        if (packet.GetPacketType() == PacketType.PlaceBlock)
        {
            PlaceBlockPacket placeBlockPacket = new PlaceBlockPacket();
            placeBlockPacket.Read(packet);

            GetWorld().SetBlockAt(placeBlockPacket.Type, placeBlockPacket.X, placeBlockPacket.Y, placeBlockPacket.Z);

        }
    }

    public override EntityType GetEntityType()
    {
        return EntityType.Player;
    }
}
