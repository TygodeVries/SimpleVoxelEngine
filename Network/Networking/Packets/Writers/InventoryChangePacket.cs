using Shared.Worlds;

namespace Shared.Networking;

public class InventoryChangePacket : PacketWriter
{
    public int slot;
    public ItemStack? itemStack;
    public override void Read(Packet packet)
    {
        slot = packet.ReadInt();
        itemStack = packet.ReadItemStack();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());
        packet.WriteInt(slot);
        packet.WriteItemStack(itemStack);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.InventoryChange;
    }
}
