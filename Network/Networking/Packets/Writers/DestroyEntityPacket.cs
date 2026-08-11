namespace Shared.Networking;

public class DestroyEntityPacket : PacketWriter
{
    public int Id;

    public override void Read(Packet packet)
    {
        Id = packet.ReadInt();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());

        packet.WriteInt(Id);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.DestroyEntity;
    }
}
