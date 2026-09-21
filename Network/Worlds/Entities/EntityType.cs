namespace Shared.Worlds;

public class EntityType
{
    public static EntityType Unregisterd = new EntityType(-1, "unregisterd");

    public string Name { get; set; }
    public int Id { get; set; }

    public string? Mesh { get; set; }

    public void SetMesh(string name)
    {
        Mesh = name;
    }

    public EntityType(int id, string name)
    {
        this.Name = name;
        this.Id = id;
    }

    public EntityType()
    {
        Name = "";
        Id = -1;
    }

    public void Deserialize(byte[] data)
    {
        MemoryStream memoryStream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(memoryStream);

        Id = reader.ReadInt32();
        Name = reader.ReadString();
        Mesh = reader.ReadString();
    }

    public byte[] Serialize()
    {
        MemoryStream memoryStream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(memoryStream);

        // Write item data
        writer.Write(Id);
        writer.Write(Name);

        if (Mesh != null)
        {
            writer.Write(Mesh);
        }
        else
        {
            writer.Write("?");
        }
        // Flush
        writer.Flush();
        memoryStream.Flush();
        return memoryStream.ToArray();
    }
}