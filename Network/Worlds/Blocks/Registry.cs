namespace Shared.Worlds;

public class Registry
{
    public static bool InRegistryStage = false;
    private static List<Block> blocks = new List<Block>();
    private static List<Item> itemTypes = new List<Item>();

    /// <summary>
    /// Deletes EVERYTHING currently regisered. Use with causion!
    /// </summary>
    public static void Clear()
    {
        InRegistryStage = false;
        blocks = new List<Block>();
        itemTypes = new List<Item>();
    }
    public static byte[] SaveAll()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(blocks.Count);

        foreach (Block block in blocks)
        {
            byte[] blockData = block.Serialize();
            writer.Write(blockData.Length);
            writer.Write(blockData);
        }

        writer.Write(itemTypes.Count);

        foreach (Item item in itemTypes)
        {
            byte[] itemData = item.Serialize();
            writer.Write(itemData.Length);
            writer.Write(itemData);
        }

        writer.Flush();
        stream.Flush();

        return stream.ToArray();
    }

    public static void LoadAll(byte[] bytes)
    {
        MemoryStream stream = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(stream);

        int blockCount = reader.ReadInt32();
        blocks = new List<Block>();

        for (int i = 0; i < blockCount; i++)
        {
            int dataL = reader.ReadInt32();
            byte[] mem = reader.ReadBytes(dataL);

            LoadBlock(mem);
        }

        int itemCount = reader.ReadInt32();

        for (int i = 0; i < itemCount; i++)
        {
            int dataL = reader.ReadInt32();
            byte[] mem = reader.ReadBytes(dataL);

            LoadItem(mem);
        }
    }

    public static event Action<Block>? OnBlockRegister;

    private static Block LoadBlock(byte[] data)
    {
        Block block = new Block();
        block.Deserialize(data);
        blocks.Add(block);

        Console.WriteLine($"Loaded Block of Type: '{block.Identifier}'");
        OnBlockRegister?.Invoke(block);
        return block;
    }

    private static Item LoadItem(byte[] data)
    {
        Item itemType = new Item();
        itemType.Deserialize(data);
        itemTypes.Add(itemType);

        Console.WriteLine($"Loaded Item of Type: '{itemType.Name}'");
        return itemType;
    }

    public static Block CreateBlock(string name)
    {
        if (!InRegistryStage)
        {
            Console.WriteLine("CreateBlock() can only be called in OnRegister()");
            throw new Exception("CreateBlock() can only be called in OnRegister()");
        }

        Block block = new Block((short)blocks.Count, name);
        blocks.Add(block);
        return block;
    }

    public static Block? GetBlock(string name)
    {
        return blocks.First(o =>
        {
            return o.Identifier == name;
        });
    }

    public static Block? GetBlock(int id)
    {
        if (id >= blocks.Count)
            return null;
        return blocks[id];
    }

    public static Item CreateItem(string name)
    {
        if (!InRegistryStage)
        {
            Console.WriteLine("CreateItem() can only be called in OnRegister()");
            throw new Exception("CreateItem() can only be called in OnRegister()");
        }

        Item item = new Item((short)blocks.Count, name);
        itemTypes.Add(item);
        return item;
    }

    public static Item? GetItem(string name)
    {
        return itemTypes.FirstOrDefault(o =>
        {
            return o.Name == name;
        });
    }

    public static Item? GetItem(int id)
    {
        if (id >= itemTypes.Count)
            return null;
        return itemTypes[id];
    }
}
