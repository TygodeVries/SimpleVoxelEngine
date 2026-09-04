namespace Shared.Networking;

public class AuthenticatePacket : PacketWriter
{
    public int EntityId;
    public int ServerVersion;

    public override void Read(Packet packet)
    {
        EntityId = packet.ReadInt();
        ServerVersion = packet.ReadInt();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());

        packet.WriteInt(EntityId);
        packet.WriteInt(ServerVersion);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.Authenticate;
    }
}
