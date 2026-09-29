namespace Shared.Networking.Packets.Writers
{
    public class DropItemPacket : PacketWriter
    {
        public override void Read(Packet packet)
        {
            // No fields
        }

        public override Packet Write()
        {
            return new Packet(WriterType()); // No field
        }

        public override PacketType WriterType()
        {
            return PacketType.DropItem;
        }
    }
}
