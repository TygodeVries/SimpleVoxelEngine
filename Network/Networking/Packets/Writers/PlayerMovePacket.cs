namespace Shared.Networking;

public class PlayerMovePacket : PacketWriter
{
    public float X;
    public float Y;
    public float Z;

    public override void Read(Packet packet)
    {
        X = packet.ReadFloat();
        Y = packet.ReadFloat();
        Z = packet.ReadFloat();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());

        packet.WriteFloat(X);
        packet.WriteFloat(Y);
        packet.WriteFloat(Z);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.PlayerMove;
    }
}
