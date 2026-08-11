namespace Shared.Networking;

public class MoveEntityPacket : PacketWriter
{
    public int Id;
    public float X;
    public float Y;
    public float Z;

    public override void Read(Packet packet)
    {
        Id = packet.ReadInt();
        X = packet.ReadFloat();
        Y = packet.ReadFloat();
        Z = packet.ReadFloat();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());

        packet.WriteInt(Id);
        packet.WriteFloat(X);
        packet.WriteFloat(Y);
        packet.WriteFloat(Z);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.MoveEntity;
    }
}
