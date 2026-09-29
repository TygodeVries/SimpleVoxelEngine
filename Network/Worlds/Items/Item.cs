using Shared.Mathf;

namespace Shared.Worlds;

public class Item
{
    public static Item Unregistered { get; private set; } = new Item()
    {
        RegistryId = -1
    };

    public int RegistryId { get; private set; }
    public string Identifier { get; internal set; } = "?";
    public int MaxStackCount { get; set; } = 99;
    public string? Texture { get; private set; }
    internal void SetRegistryId(int id)
    {
        this.RegistryId = id;
    }

    public Item()
    {

    }

    /// <summary>
    /// When the item is used to right click
    /// </summary>
    public event Action<ItemClickArgs>? OnRightClick;

    /// <summary>
    /// When the item is used to left click
    /// </summary>
    public event Action<ItemClickArgs>? OnLeftClick;

    /// <summary>
    /// When this item is used to right click a block
    /// </summary>
    public event Action<ItemClickBlockArgs>? OnBlockRightClick;

    /// <summary>
    /// When this item is used to left click a block
    /// </summary>
    public event Action<ItemClickBlockArgs>? OnBlockLeftClick;

    public virtual void ExecuteRightClick(ItemClickArgs args)
        => OnRightClick?.Invoke(args);

    public virtual void ExecuteLeftClick(ItemClickArgs args)
        => OnLeftClick?.Invoke(args);

    public virtual void ExecuteBlockRightClick(ItemClickBlockArgs args)
        => OnBlockRightClick?.Invoke(args);

    public virtual void ExecuteBlockLeftClick(ItemClickBlockArgs args)
        => OnBlockLeftClick?.Invoke(args);

    public void Deserialize(byte[] data)
    {
        MemoryStream memoryStream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(memoryStream);

        RegistryId = reader.ReadInt32();
        Identifier = reader.ReadString();
        Texture = reader.ReadString();
    }

    public byte[] Serialize()
    {
        MemoryStream memoryStream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(memoryStream);

        // Write item data
        writer.Write(RegistryId);
        writer.Write(Identifier);
        writer.Write(Texture);

        // Flush
        writer.Flush();
        memoryStream.Flush();
        return memoryStream.ToArray();
    }

    public void SetTexture(string texture)
    {
        this.Texture = texture;
    }
}

public class ItemClickArgs
{
    public Entity Entity { get; set; }

    public ItemClickArgs(Entity entity)
    {
        this.Entity = entity;
    }
}

public class ItemClickBlockArgs
{
    public Entity Entity { get; set; }
    public Vector3 Block { get; set; }
    public Vector3 Normal { get; set; }
    public World GetWorld()
    {
        return Entity.GetWorld()!;
    }
    public ItemClickBlockArgs(Entity entity, Vector3 block, Vector3 normal)
    {
        this.Entity = entity;
        this.Block = block;
        this.Normal = normal;
    }
}