namespace Shared.Networking;

public class DreamsServerInfoPacket : PacketWriter
{
    // #TODO add things like name, MOTD that sort of stuff
    public DreamsServerInfoPacket()
    {

    }

    public override void Read(Packet packet)
    {

    }

    public override Packet Write()
    {
        return new Packet(WriterType());
    }

    public override PacketType WriterType()
    {
        return PacketType.DreamsServerInfo;
    }
}
