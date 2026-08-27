using Shared.Mathf;

namespace Shared.Worlds;

public class Item
{
    public static Item Unregistered { get; private set; } = new Item(-1, "unregistered");

    public int id { get; private set; }
    public string Name { get; private set; }
    public string? texture;

    internal Item(int id, string name)
    {
        this.id = id;
        this.Name = name;
    }

    internal Item()
    {
        Name = "?";
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

    public void ExecuteRightClick(ItemClickArgs args)
    {
        OnRightClick?.Invoke(args);
    }

    public void ExecuteLeftClick(ItemClickArgs args)
    {
        OnLeftClick?.Invoke(args);
    }

    public void ExecuteBlockRightClick(ItemClickBlockArgs args)
    {
        OnBlockRightClick?.Invoke(args);
    }

    public void ExecuteBlockLeftClick(ItemClickBlockArgs args)
    {
        OnBlockLeftClick?.Invoke(args);
    }

    public void Deserialize(byte[] data)
    {
        MemoryStream memoryStream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(memoryStream);

        id = reader.ReadInt32();
        Name = reader.ReadString();
        texture = reader.ReadString();
    }

    public byte[] Serialize()
    {
        MemoryStream memoryStream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(memoryStream);

        // Write item data
        writer.Write(id);
        writer.Write(Name);
        writer.Write(texture);

        // Flush
        writer.Flush();
        memoryStream.Flush();
        return memoryStream.ToArray();
    }

    public void SetTexture(string texture)
    {
        this.texture = texture;
    }
}

public class ItemClickArgs
{
    public Entity Clicker { get; set; }

    public ItemClickArgs(Entity clicker)
    {
        this.Clicker = clicker;
    }
}

public class ItemClickBlockArgs
{
    public Entity Clicker { get; set; }
    public Vector3 Block { get; set; }
    public Vector3 Normal { get; set; }
    public World GetWorld()
    {
        return Clicker.GetWorld()!;
    }
    public ItemClickBlockArgs(Entity clicker, Vector3 block, Vector3 normal)
    {
        this.Clicker = clicker;
        this.Block = block;
        this.Normal = normal;
    }
}