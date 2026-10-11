using Shared.Mathf;

namespace Shared.Networking;

public class PlayerMovePacket : PacketWriter
{
    public Vector3 Position;
    public Vector3 Direction;

    public override void Read(Packet packet)
    {
        Position = packet.ReadVector3();
        Direction = packet.ReadVector3();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());

        packet.WriteVector3(Position);
        packet.WriteVector3(Direction);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.PlayerMove;
    }
}
