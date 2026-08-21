namespace Shared.Worlds;

public class BlockTexture
{
    public int Up;
    public string UpName;

    public int Down;
    public string DownName;


    public int Left;
    public string LeftName;


    public int Right;
    public string RightName;

    public int Forward;
    public string ForwardName;

    public int Backward;
    public string BackwardName;

    internal BlockTexture()
    {
        Up = 0;
        Down = 0;
        Left = 0;
        Right = 0;
        Forward = 0;
        Backward = 0;

        UpName = "none";
        DownName = "none";
        RightName = "none";
        LeftName = "none";
        ForwardName = "none";
        BackwardName = "none";
    }
    public BlockTexture(string texture)
    {
        int id = BlockTextureAtlas.GetTextureId(texture);

        Up = id;
        UpName = texture;

        Down = id;
        DownName = texture;

        Left = id;
        LeftName = texture;

        Right = id;
        RightName = texture;

        Forward = id;
        ForwardName = texture;

        Backward = id;
        BackwardName = texture;
    }

    public void SetFaceTexture(BlockFace face, string texture)
    {

        int id = BlockTextureAtlas.GetTextureId(texture);

        if (face == BlockFace.Up)
        {
            Up = id;
            UpName = texture;
        }

        if (face == BlockFace.Down)
        {
            Down = id;
            DownName = texture;
        }

        if (face == BlockFace.Left)
        {
            Left = id;
            LeftName = texture;
        }

        if (face == BlockFace.Right)
        {
            Right = id;
            RightName = texture;
        }

        if (face == BlockFace.Forward)
        {
            Forward = id;
            ForwardName = texture;
        }

        if (face == BlockFace.Backward)
        {
            Backward = id;
            BackwardName = texture;
        }
    }

    public byte[] Serialize()
    {
        MemoryStream str = new MemoryStream();
        BinaryWriter binaryWriter = new BinaryWriter(str);
        binaryWriter.Write(UpName);
        binaryWriter.Write(DownName);
        binaryWriter.Write(LeftName);
        binaryWriter.Write(RightName);
        binaryWriter.Write(ForwardName);
        binaryWriter.Write(BackwardName);

        binaryWriter.Flush();
        str.Flush();
        return str.ToArray();
    }

    public void Load(byte[] texture)
    {
        MemoryStream str = new MemoryStream(texture);
        BinaryReader binaryReader = new BinaryReader(str);
        UpName = binaryReader.ReadString();
        DownName = binaryReader.ReadString();
        LeftName = binaryReader.ReadString();
        RightName = binaryReader.ReadString();
        ForwardName = binaryReader.ReadString();
        BackwardName = binaryReader.ReadString();

        SetFaceTexture(BlockFace.Up, UpName);
        SetFaceTexture(BlockFace.Down, DownName);
        SetFaceTexture(BlockFace.Left, LeftName);
        SetFaceTexture(BlockFace.Right, RightName);
        SetFaceTexture(BlockFace.Forward, ForwardName);
        SetFaceTexture(BlockFace.Backward, BackwardName);
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