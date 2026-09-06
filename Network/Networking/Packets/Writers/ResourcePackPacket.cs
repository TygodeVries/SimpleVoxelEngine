namespace Shared.Networking;

public class ResourcePackPacket : PacketWriter
{
    public List<string>? names;
    public ResourceType resourceType;
    public byte[] resourceData;
    public int textureResolution;
    public override Packet Write()
    {
        if (names == null)
            throw new NullReferenceException("Names must be passed for this packet to be valid!");

        Packet packet = new Packet(WriterType());

        packet.WriteInt((int)resourceType);
        packet.WriteInt(textureResolution);

        packet.WriteInt(names.Count);
        foreach (string name in names)
        {
            packet.WriteString(name);
        }

        packet.WriteInt(resourceData.Length);
        packet.WriteByteArray(resourceData);

        return packet;
    }

    public override PacketType WriterType()
    {
        return PacketType.ResourcePack;
    }

    public override void Read(Packet packet)
    {
        names = new List<string>();
        resourceType = (ResourceType)packet.ReadInt();
        textureResolution = packet.ReadInt();

        int nameCount = packet.ReadInt();
        for (int i = 0; i < nameCount; i++)
        {
            names.Add(packet.ReadString());
        }

        int l = packet.ReadInt();
        resourceData = packet.ReadByteArray(l);
    }
}

public enum ResourceType
{
    BLOCKS_TEXTURES,
    ITEMS_TEXTURES,
    SOUND
}