namespace Shared.Networking;

public class DreamsRemoveUserPacket : PacketWriter
{
    public int id;

    public DreamsRemoveUserPacket()
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
        return PacketType.DreamsRemoveUser;
    }
}
