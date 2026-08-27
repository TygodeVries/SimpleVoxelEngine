using Shared.Mathf;

namespace Shared.Networking;

public class PlayerInteractPacket : PacketWriter
{
    public InteractionType InteractionType;
    public Vector3 BlockPos = Vector3.Zero;
    public Vector3 BlockNormal = Vector3.Zero;

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());
        packet.WriteByte((byte)InteractionType);

        packet.WriteVector3(BlockPos);
        packet.WriteVector3(BlockNormal);

        return packet;
    }

    public override void Read(Packet packet)
    {
        InteractionType = (InteractionType)packet.ReadByte();

        BlockPos = packet.ReadVector3();
        BlockNormal = packet.ReadVector3();
    }

    public override PacketType WriterType()
    {
        return PacketType.PlayerInteract;
    }
}

public enum InteractionType : byte
{
    RightClickBlock,
    LeftClickBlock,

    RightClickAir,
    LeftClickAir
}
