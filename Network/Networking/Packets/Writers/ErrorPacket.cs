namespace Shared.Networking;

public class ErrorPacket : PacketWriter
{
    public string Message;

    public override Packet Write()
    {
        Packet packet = new Packet(PacketType.Error);
        packet.WriteString(Message);
        return packet;
    }

    public override void Read(Packet packet)
    {
        Message = packet.ReadString();
    }

    public override PacketType WriterType()
    {
        return PacketType.Error;
    }
}
