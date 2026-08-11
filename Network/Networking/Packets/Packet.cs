namespace Shared.Networking;

public class Packet
{
    private MemoryStream packetStream;
    private BinaryReader reader;
    private BinaryWriter writer;

    private PacketType packetType { get; set; }
    public PacketType GetPacketType()
    {
        return packetType;
    }

    public byte[] GetBytes()
    {
        writer.Flush();

        byte[] data = packetStream.ToArray();
        byte[] result = new byte[data.Length + 1];

        result[0] = (byte)packetType;
        Buffer.BlockCopy(data, 0, result, 1, data.Length);

        return result;
    }

    public Packet(PacketType packetType, byte[] data)
    {
        this.packetType = packetType;
        packetStream = new MemoryStream(data);

        reader = new BinaryReader(packetStream);
        writer = new BinaryWriter(packetStream);
    }

    public Packet(PacketType packetType)
    {
        this.packetType = packetType;
        packetStream = new MemoryStream();

        reader = new BinaryReader(packetStream);
        writer = new BinaryWriter(packetStream);
    }

    // Write methods
    public void WriteInt(int value)
    {
        writer.Write(value);
    }

    public void WriteString(string value)
    {
        writer.Write(value);
    }

    public void WriteFloat(float value)
    {
        writer.Write(value);
    }

    public void WriteByte(byte value)
    {
        writer.Write(value);
    }

    public void WriteByteArray(byte[] value)
    {
        writer.Write(value);
    }

    // Read methods
    public int ReadInt()
    {
        return reader.ReadInt32();
    }

    public string ReadString()
    {
        return reader.ReadString();
    }

    public float ReadFloat()
    {
        return reader.ReadSingle();
    }

    public byte ReadByte()
    {
        return reader.ReadByte();
    }

    // Reads a fixed number of bytes
    public byte[] ReadByteArray(int length)
    {
        return reader.ReadBytes(length);
    }
}