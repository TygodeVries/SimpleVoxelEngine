using Shared.Worlds;

namespace Shared.Networking;

public class SpawnEntityPacket : PacketWriter
{
    public int Id;
    public EntityType Type;

    public override void Read(Packet packet)
    {
        Id = packet.ReadInt();
        Type = (EntityType)packet.ReadInt();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());

        packet.WriteInt(Id);
        packet.WriteInt((int)Type);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.SpawnEntity;
    }
}
