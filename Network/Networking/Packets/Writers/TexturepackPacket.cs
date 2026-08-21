namespace Shared.Networking;

public class TexturepackPacket : PacketWriter
{
    public List<string> names = new List<string>();
    public TextureType textureType;
    public byte[] textureData;
    public int textureResolution;
    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());

        packet.WriteInt((int)textureType);
        packet.WriteInt(textureResolution);

        packet.WriteInt(names.Count);
        foreach (string name in names)
        {
            packet.WriteString(name);
        }

        packet.WriteInt(textureData.Length);
        packet.WriteByteArray(textureData);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.Texturepack;
    }

    public override void Read(Packet packet)
    {
        textureType = (TextureType)packet.ReadInt();
        textureResolution = packet.ReadInt();

        int nameCount = packet.ReadInt();
        for (int i = 0; i < nameCount; i++)
        {
            names.Add(packet.ReadString());
        }

        int l = packet.ReadInt();
        textureData = packet.ReadByteArray(l);
    }
}

public enum TextureType
{
    BLOCKS
}