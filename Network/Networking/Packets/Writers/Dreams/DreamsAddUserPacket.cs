namespace Shared.Networking;

public class DreamsAddUserPacket : PacketWriter
{
    public int id;

    public DreamsAddUserPacket()
    {

    }

    public override void Read(Packet packet)
    {
        id = packet.ReadInt();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());
        packet.WriteInt(id);
        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.DreamsAddUser;
    }
}
