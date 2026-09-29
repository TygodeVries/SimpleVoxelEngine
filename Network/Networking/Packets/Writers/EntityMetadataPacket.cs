namespace Shared.Networking;

public class EntityMetadataPacket : PacketWriter
{
    public int Id;
    public string Key;
    public string Value;

    public override void Read(Packet packet)
    {
        Id = packet.ReadInt();
        Key = packet.ReadString();
        Value = packet.ReadString();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());

        packet.WriteInt(Id);
        packet.WriteString(Key);
        packet.WriteString(Value);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.EntityMetadata;
    }
}
