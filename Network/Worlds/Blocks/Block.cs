using Shared.ActionArguments;

namespace Shared.Worlds;

public class Block
{
    public static Block Unregistered { get; private set; } = new Block(-1, "unregistered");

    /// <summary>
    /// The internal RegistryId of the block
    /// </summary>
    public short RegistryId { get; private set; }


    public string Identifier { get; private set; } = "invalid";
    internal Block(short id, string identifier)
    {
        this.RegistryId = id;
        this.Identifier = identifier;
    }

    internal Block()
    {

    }

    /// <summary>
    /// Default: True If the block has collision, or can be walked trough
    /// </summary>
    public bool Solid { get; set; } = true;

    /// <summary>
    /// Default: True If the block is visible.
    /// Setting it to false will make it not generate any topology.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// The Texture of the block
    /// </summary>
    public BlockTexture? Texture { get; set; }

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

        writer.Write(RegistryId);
        writer.Write(Identifier);
        writer.Write(Solid);
        writer.Write(Visible);

        byte[] textureData = new byte[0];
        if (Texture != null)
        {
            textureData = Texture.Serialize();

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

        RegistryId = reader.ReadInt16();
        Identifier = reader.ReadString();
        Solid = reader.ReadBoolean();
        Visible = reader.ReadBoolean();

        int l = reader.ReadInt32();

        if (l != 0)
        {
            byte[] tex = reader.ReadBytes(l);
            Texture = new BlockTexture();
            Texture.Load(tex);
        }
    }
}
