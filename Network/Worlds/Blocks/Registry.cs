namespace Shared.Worlds;

public class Registry
{
    public static bool InRegistryStage = false;
    private static List<Block> blockTypes = new List<Block>();
    private static List<Item> itemTypes = new List<Item>();
    private static List<EntityType> entityTypes = new List<EntityType>();

    /// <summary>
    /// Deletes EVERYTHING currently regisered. Use with causion!
    /// </summary>
    public static void Clear()
    {
        InRegistryStage = false;
        blockTypes = new List<Block>();
        itemTypes = new List<Item>();
        entityTypes = new List<EntityType>();
    }
    public static byte[] SaveAll()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(blockTypes.Count);

        foreach (Block block in blockTypes)
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

        writer.Write(entityTypes.Count);

        foreach (EntityType entityType in entityTypes)
        {
            byte[] entityData = entityType.Serialize();
            writer.Write(entityData.Length);
            writer.Write(entityData);
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
        blockTypes = new List<Block>();

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

        int entityCount = reader.ReadInt32();

        for (int i = 0; i < entityCount; i++)
        {
            int dataL = reader.ReadInt32();
            byte[] mem = reader.ReadBytes(dataL);

            LoadEntity(mem);
        }
    }

    public static event Action<Block>? OnBlockRegister;

    private static Block LoadBlock(byte[] data)
    {
        Block block = new Block();
        block.Deserialize(data);
        blockTypes.Add(block);

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


    private static EntityType LoadEntity(byte[] data)
    {
        EntityType entityType = new EntityType();
        entityType.Deserialize(data);
        entityTypes.Add(entityType);

        Console.WriteLine($"Loaded Entity of type: '{entityType.Name}'");
        return entityType;
    }

    public static Block CreateBlock(string name)
    {
        if (!InRegistryStage)
        {
            Console.WriteLine("CreateBlock() can only be called in OnRegister()");
            throw new Exception("CreateBlock() can only be called in OnRegister()");
        }

        Console.WriteLine($"> Adding block {name} to registry.");


        Block block = new Block((short)blockTypes.Count, name);
        blockTypes.Add(block);
        return block;
    }

    public static Block? GetBlock(string name)
    {
        return blockTypes.First(o =>
        {
            return o.Identifier == name;
        });
    }

    public static Block? GetBlock(int id)
    {
        if (id >= blockTypes.Count)
            return null;
        return blockTypes[id];
    }

    public static Item CreateItem(string name)
    {
        if (!InRegistryStage)
        {
            Console.WriteLine("CreateItem() can only be called in OnRegister()");
            throw new Exception("CreateItem() can only be called in OnRegister()");
        }


        Console.WriteLine($"> Adding item {name} to registry.");
        Item item = new Item((short)itemTypes.Count, name);
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

    public static EntityType CreateEntity(string name)
    {
        if (!InRegistryStage)
        {
            Console.WriteLine("CreateEntity() can only be called in OnRegister()");
            throw new Exception("CreateEntity() can only be called in OnRegister()");
        }

        Console.WriteLine($"> Adding entity {name} to registry.");

        EntityType entity = new EntityType((short)entityTypes.Count, name);
        entityTypes.Add(entity);
        return entity;
    }

    public static EntityType? GetEntity(string name)
    {
        return entityTypes.FirstOrDefault(o =>
        {
            return o.Name == name;
        });
    }

    public static EntityType? GetEntity(int id)
    {
        foreach (EntityType types in entityTypes)
        {
            Console.WriteLine($"- {types.Name}");
        }

        if (id >= entityTypes.Count)
            return null;

        return entityTypes[id];
    }
}
