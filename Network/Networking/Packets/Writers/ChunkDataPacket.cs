using Shared.Mathf;
using Shared.Worlds;

namespace Shared.Networking;

public class ChunkDataPacket : PacketWriter
{
    public int X;
    public int Y;
    public int Z;
    public byte[] data;

    public override Packet Write()
    {
        Packet packet = new Packet(PacketType.ChunkData);
        packet.WriteInt(X);
        packet.WriteInt(Y);
        packet.WriteInt(Z);

        byte[] compr = Compression.Compress(data);
        packet.WriteInt(compr.Length);
        packet.WriteByteArray(compr);

        return packet;
    }

    public override void Read(Packet packet)
    {
        X = packet.ReadInt();
        Y = packet.ReadInt();
        Z = packet.ReadInt();

        int l = packet.ReadInt();
        byte[] compr = packet.ReadByteArray(l);
        data = Compression.Decompress(compr);
    }

    public ChunkDataPacket()
    {
        X = 0;
        Y = 0;
        Z = 0;
        data = new byte[0];
    }
    public ChunkDataPacket(int x, int y, int z, byte[] data)
    {
        X = x;
        Y = y;
        Z = z;
        this.data = data;
    }

    public ChunkDataPacket(Chunk chunk)
    {
        X = chunk.X;
        Y = chunk.Y;
        Z = chunk.Z;
        data = chunk.GetByteArray();
    }

    public override PacketType WriterType()
    {
        return PacketType.ChunkData;
    }
}
