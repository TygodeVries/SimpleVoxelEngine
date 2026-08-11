namespace Shared.Networking;

public class PlaceBlockPacket : PacketWriter
{
    public short Type;
    public int X;
    public int Y;
    public int Z;

    public PlaceBlockPacket()
    {

    }

    public override void Read(Packet packet)
    {
        Type = (short)packet.ReadInt();
        X = packet.ReadInt();
        Y = packet.ReadInt();
        Z = packet.ReadInt();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(PacketType.PlaceBlock);

        packet.WriteInt(Type);
        packet.WriteInt(X);
        packet.WriteInt(Y);
        packet.WriteInt(Z);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.PlaceBlock;
    }
}
