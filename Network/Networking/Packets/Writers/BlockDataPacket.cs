namespace Shared.Networking;

public class BlockDataPacket : PacketWriter
{
    public byte[] BlockData;
    public override void Read(Packet packet)
    {
        int data = packet.ReadInt();
        BlockData = packet.ReadByteArray(data);
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());
        packet.WriteInt(BlockData.Length);
        packet.WriteByteArray(BlockData);
        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.BlockData;
    }
}

