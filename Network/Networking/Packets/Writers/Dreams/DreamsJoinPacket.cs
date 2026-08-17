namespace Shared.Networking;

public class DreamsJoinPacket : PacketWriter
{
    public string code;

    public DreamsJoinPacket()
    {

    }

    public override void Read(Packet packet)
    {
        code = packet.ReadString();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());

        packet.WriteString(code);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.DreamsJoin;
    }
}
