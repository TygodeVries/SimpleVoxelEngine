using Shared.Mathf;

namespace Shared.Networking.Packets.Writers
{
    public class BlockBreakProgressPacket : PacketWriter
    {
        public Vector3 position;
        public int stage;

        public override void Read(Packet packet)
        {
            position = packet.ReadVector3();
            stage = packet.ReadInt();
        }

        public override Packet Write()
        {
            Packet packet = new Packet(WriterType());
            packet.WriteVector3(position);
            packet.WriteInt(stage);
            return packet;
        }

        public override PacketType WriterType()
        {
            return PacketType.BlockBreakProgress;
        }
    }
}
