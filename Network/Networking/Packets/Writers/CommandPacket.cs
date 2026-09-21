namespace Shared.Networking.Packets.Writers
{
    public class CommandPacket : PacketWriter
    {
        public string Command = "";

        public override void Read(Packet packet)
        {
            Command = packet.ReadString();
        }

        public override Packet Write()
        {
            Packet packet = new Packet(PacketType.Command);
            packet.WriteString(Command);
            return packet;
        }

        public override PacketType WriterType()
        {
            return PacketType.Command;
        }
    }
}
