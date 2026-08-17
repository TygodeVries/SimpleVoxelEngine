namespace Shared.Networking;

public class DreamsPacketDataPacket : PacketWriter
{
    public int owner;
    public Packet packet;

    public DreamsPacketDataPacket()
    {

    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());
        packet.WriteInt(owner);
        byte[] data = this.packet.GetBytes();
        packet.WriteInt(data.Length);
        packet.WriteByteArray(data);
        return packet;
    }

    public override void Read(Packet packet)
    {
        owner = packet.ReadInt();

        int l = packet.ReadInt();
        byte type = packet.ReadByte();
        byte[] data = packet.ReadByteArray(l - 1);

        Packet dataPacket = new Packet((PacketType)type, data);
        this.packet = dataPacket;
    }
    public override PacketType WriterType()
    {
        return PacketType.DreamsPacketData;
    }
}
