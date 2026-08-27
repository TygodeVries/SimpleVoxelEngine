using Shared.Mathf;
using Shared.Worlds;

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

    public void WriteVector3(Vector3 vector)
    {
        WriteFloat(vector.X);
        WriteFloat(vector.Y);
        WriteFloat(vector.Z);
    }

    public void WriteByte(byte value)
    {
        writer.Write(value);
    }

    public void WriteByteArray(byte[] value)
    {
        writer.Write(value);
    }

    public void WriteBool(bool value)
    {
        writer.Write(value);
    }

    public void WriteItemStack(ItemStack? itemStack)
    {
        if (itemStack == null)
        {
            WriteBool(false);
            return;
        }

        WriteBool(true);
        WriteInt(itemStack.Count);
        WriteString(itemStack.Type.Name);
    }

    // Read methods

    public ItemStack? ReadItemStack()
    {
        bool exists = ReadBool();
        if (!exists)
            return null;

        int amount = ReadInt();
        string typeName = ReadString();

        Item? type = Registry.GetItem(typeName);
        if (type == null)
        {
            throw new Exception($"Could not find item {typeName} in registry while decoding packet!");
        }

        ItemStack itemStack = new ItemStack(type, amount);
        return itemStack;
    }

    public int ReadInt()
    {
        return reader.ReadInt32();
    }
    public bool ReadBool()
    {
        return reader.ReadBoolean();
    }

    public string ReadString()
    {
        return reader.ReadString();
    }

    public float ReadFloat()
    {
        return reader.ReadSingle();
    }

    public Vector3 ReadVector3()
    {
        return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
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