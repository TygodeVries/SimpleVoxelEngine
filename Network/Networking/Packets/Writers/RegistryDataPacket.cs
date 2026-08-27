namespace Shared.Networking;

public class RegistryDataPacket : PacketWriter
{
    public byte[] Data;
    public override void Read(Packet packet)
    {
        int data = packet.ReadInt();
        Data = packet.ReadByteArray(data);
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());
        packet.WriteInt(Data.Length);
        packet.WriteByteArray(Data);
        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.RegistryData;
    }
}

