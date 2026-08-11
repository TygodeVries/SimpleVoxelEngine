namespace Shared.Networking;

public class AuthenticatePacket : PacketWriter
{
    public int EntityId;

    public override void Read(Packet packet)
    {
        EntityId = packet.ReadInt();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());

        packet.WriteInt(EntityId);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.Authenticate;
    }
}
