using Shared.Worlds;

namespace Shared.Networking;

public class SpawnEntityPacket : PacketWriter
{
    public int Id;
    public EntityType? Type;

    public override void Read(Packet packet)
    {
        Id = packet.ReadInt();
        Type = Registry.GetEntity(packet.ReadInt());
        Console.WriteLine(Type.Id);
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());

        packet.WriteInt(Id);
        if (Type == null)
        {
            throw new Exception("Entity does not exist.");
        }
        packet.WriteInt(Type.Id);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.SpawnEntity;
    }
}
