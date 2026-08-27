namespace Shared.Networking;

public class SelectSlotPacket : PacketWriter
{
    public int Slot;

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());

        packet.WriteInt(Slot);

        return packet;
    }

    public override void Read(Packet packet)
    {
        Slot = packet.ReadInt();
    }

    public override PacketType WriterType()
    {
        return PacketType.SelectSlot;
    }
}
