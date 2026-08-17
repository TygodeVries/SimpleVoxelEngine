namespace Shared.Networking;

public class UnloadChunkPacket : PacketWriter
{
    public int X;
    public int Y;
    public int Z;
    public override void Read(Packet packet)
    {
        X = packet.ReadInt();
        Y = packet.ReadInt();
        Z = packet.ReadInt();
    }

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());
        packet.WriteInt(X);
        packet.WriteInt(Y);
        packet.WriteInt(Z);
        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.UnloadChunk;
    }
}
