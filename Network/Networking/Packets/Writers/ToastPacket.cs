namespace Shared.Networking;

public class ToastPacket : PacketWriter
{

    public string msg = "None";
    public float time;
    public override void Read(Packet packet)
    {
        msg = packet.ReadString();
        time = packet.ReadFloat();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());
        packet.WriteString(msg);
        packet.WriteFloat(time);
        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.Toast;
    }
}
