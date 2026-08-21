namespace Shared.Worlds;

public class Registry
{
    public static bool InRegistryStage = false;
    private static List<Block> blocks = new List<Block>();

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
    }

    public static event Action<Block>? OnBlockRegister;

    public static Block LoadBlock(byte[] data)
    {
        Block block = new Block();
        block.Deserialize(data);
        blocks.Add(block);

        Console.WriteLine($"Loaded block {block.name}");
        OnBlockRegister?.Invoke(block);
        return block;
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
            return o.name == name;
        });
    }

    public static Block? GetBlock(int id)
    {
        if (id >= blocks.Count)
            return null;
        return blocks[id];
    }
}
