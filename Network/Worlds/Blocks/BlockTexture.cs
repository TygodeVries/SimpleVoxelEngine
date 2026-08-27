namespace Shared.Worlds;

public class BlockTexture
{
    public string Up;
    public string Down;
    public string Left;
    public string Right;
    public string Forward;
    public string Backward;

    public BlockTexture()
    {
        Up = "none";
        Down = "none";
        Left = "none";
        Right = "none";
        Forward = "none";
        Backward = "none";
    }

    public BlockTexture Fill(string texture)
    {
        Up = texture;
        Down = texture;
        Left = texture;
        Right = texture;
        Forward = texture;
        Backward = texture;
        return this;
    }

    public BlockTexture SetFace(BlockFace face, string texture)
    {
        if (face == BlockFace.Up)
        {
            Up = texture;
        }

        if (face == BlockFace.Down)
        {
            Down = texture;
        }

        if (face == BlockFace.Left)
        {
            Left = texture;
        }

        if (face == BlockFace.Right)
        {
            Right = texture;
        }

        if (face == BlockFace.Forward)
        {
            Forward = texture;
        }

        if (face == BlockFace.Backward)
        {
            Backward = texture;
        }

        return this;
    }

    public byte[] Serialize()
    {
        MemoryStream str = new MemoryStream();
        BinaryWriter binaryWriter = new BinaryWriter(str);
        binaryWriter.Write(Up);
        binaryWriter.Write(Down);
        binaryWriter.Write(Left);
        binaryWriter.Write(Right);
        binaryWriter.Write(Forward);
        binaryWriter.Write(Backward);

        binaryWriter.Flush();
        str.Flush();
        return str.ToArray();
    }

    public void Load(byte[] texture)
    {
        MemoryStream str = new MemoryStream(texture);
        BinaryReader binaryReader = new BinaryReader(str);
        Up = binaryReader.ReadString();
        Down = binaryReader.ReadString();
        Left = binaryReader.ReadString();
        Right = binaryReader.ReadString();
        Forward = binaryReader.ReadString();
        Backward = binaryReader.ReadString();

        SetFace(BlockFace.Up, Up);
        SetFace(BlockFace.Down, Down);
        SetFace(BlockFace.Left, Left);
        SetFace(BlockFace.Right, Right);
        SetFace(BlockFace.Forward, Forward);
        SetFace(BlockFace.Backward, Backward);
    }
}

public enum BlockFace
{
    Up,
    Down,
    Left,
    Right,
    Forward,
    Backward
}