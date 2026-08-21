using Shared.ActionArguments;

namespace Shared.Worlds;

public class Block
{
    public short id { get; private set; }
    public string name { get; private set; }
    internal Block(short id, string name)
    {
        this.id = id;
        this.name = name;
    }

    internal Block()
    {

    }

    /// <summary>
    /// Default: True If the block has collision, or can be walked trough
    /// </summary>
    public bool isSolid = true;

    /// <summary>
    /// Default: True If the block is visible.
    /// Setting it to false will make it not generate any topology.
    /// </summary>
    public bool isVisible = true;

    /// <summary>
    /// The texture of the block
    /// </summary>
    public BlockTexture? texture;

    /// <summary>
    /// Runs when the block is broken
    /// </summary>
    public event Action<BlockBrokenArgs>? OnBlockBreak;
    internal void TriggerBlockBreak(BlockBrokenArgs args)
    {
        OnBlockBreak?.Invoke(args);
    }

    public byte[] Serialize()
    {
        MemoryStream memoryStream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(memoryStream);

        writer.Write(id);
        writer.Write(name);
        writer.Write(isSolid);
        writer.Write(isVisible);

        byte[] textureData = new byte[0];
        if (texture != null)
        {
            textureData = texture.Serialize();

            writer.Write(textureData.Length);
            writer.Write(textureData);
        }
        else
        {
            writer.Write(0);
        }


        writer.Flush();
        memoryStream.Flush();
        return memoryStream.ToArray();
    }

    public void Deserialize(byte[] data)
    {
        MemoryStream memoryStream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(memoryStream);

        id = reader.ReadInt16();
        name = reader.ReadString();
        isSolid = reader.ReadBoolean();
        isVisible = reader.ReadBoolean();

        int l = reader.ReadInt32();

        if (l != 0)
        {
            byte[] tex = reader.ReadBytes(l);
            texture = new BlockTexture();
            texture.Load(tex);
        }
    }
}
