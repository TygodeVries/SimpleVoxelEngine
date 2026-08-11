using System.Runtime.InteropServices;

namespace Shared.Worlds;

public class Chunk
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Z { get; private set; }

    public Chunk(int x, int y, int z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    public byte[] GetByteArray()
    {
        byte[] result = new byte[data.Length + 1];

        result[0] = (byte)type;
        Buffer.BlockCopy(data, 0, result, 1, data.Length);

        return result;
    }

    public void Fill(short type)
    {
        if (this.type == ChunkType.Single && BitConverter.ToInt16(data, 0) == type)
        {
            return;
        }

        this.type = ChunkType.Single;

        this.data = BitConverter.GetBytes(type);

        isDirty = true;
    }

    public void SetByteArray(byte[] source)
    {
        if (source == null || source.Length < 1)
        {
            throw new ArgumentException("Source array is empty or null.");
        }

        this.type = (ChunkType)source[0];

        this.data = new byte[source.Length - 1];
        Buffer.BlockCopy(source, 1, this.data, 0, this.data.Length);

        isDirty = true;
    }

    public static Chunk GetChunk(byte[] bytes, int x, int y, int z)
    {
        Chunk chunk = new Chunk(x, y, z);

        chunk.type = (ChunkType)bytes[0];

        chunk.data = new byte[bytes.Length - 1];
        Buffer.BlockCopy(bytes, 1, chunk.data, 0, chunk.data.Length);

        return chunk;
    }

    private ChunkType type = ChunkType.Single;
    private byte[] data = new byte[2] { 0, 0 }; // Block 0 by default.
    private const int SIMPLE_CHUNK_DATA_SIZE = 4096;

    public ChunkType GetChunkType()
    {
        return type;
    }

    /// <summary>
    /// Using this function will NOT send a packet to the clients. please use World.SetBlock instead!
    /// </summary>
    /// <param name="blockType"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void SetBlock(short blockType, int x, int y, int z)
    {
        // If the block is already this value, we don't have to do anything.
        if (GetBlock(x, y, z) == blockType)
        {
            return;
        }
        isDirty = true;

        // Since our block is NOT the same as the one we currently are.
        // Here we are converting from a Single, to a simple.
        if (type == ChunkType.Single)
        {
            short currentBlock = GetBlock(x, y, z);

            // We change our type to Simple, as we have more then one type of block
            type = ChunkType.Simple;

            // 4096 bytes for chunk data, 4 for the block types and one for the block count
            // Initialized at 0.
            data = new byte[SIMPLE_CHUNK_DATA_SIZE + 4];

            // Block type one. The one that is currently filling the whole chunk, we put at index 0.
            // This is because all the other values are also started at 0.
            Array.Copy(BitConverter.GetBytes(currentBlock), 0, data, SIMPLE_CHUNK_DATA_SIZE, 2);

            // Block type two, the new kid on the block
            Array.Copy(BitConverter.GetBytes(blockType), 0, data, SIMPLE_CHUNK_DATA_SIZE + 2, 2);

            // Calculate the position of the block in memory
            int index = x + (y * 16) + (z * 16 * 16);

            data[index] = 1;
            return;
        }

        if (type == ChunkType.Simple)
        {
            short[] blockMap = new short[(data.Length - SIMPLE_CHUNK_DATA_SIZE) / 2];

            // First we fill out our block map.
            for (int i = 0; i < blockMap.Length; i++)
            {
                blockMap[i] = BitConverter.ToInt16(data, (i * 2) + SIMPLE_CHUNK_DATA_SIZE);
            }

            // If the block is already part of the chunk, its easy
            if (blockMap.Contains(blockType))
            {
                byte blockIndex = (byte)blockMap.IndexOf(blockType);

                // Calculate the position of the block in memory
                int index = x + (y * 16) + (z * 16 * 16);

                // Change the block to the new type.
                data[index] = blockIndex;
            }
            else
            {
                // Convert to a more complex model
                if (blockMap.Count() + 1 > 255)
                {
                    type = ChunkType.Complex;
                    short[] blockData = new short[SIMPLE_CHUNK_DATA_SIZE];

                    for (int i = 0; i < SIMPLE_CHUNK_DATA_SIZE; i++)
                    {
                        blockData[i] = blockMap[data[i]];
                    }

                    // Turn the short[] into a byte[]
                    ReadOnlySpan<byte> byteSpan = MemoryMarshal.Cast<short, byte>(blockData);

                    data = byteSpan.ToArray();

                    int index = (x + (y * 16) + (z * 16 * 16)) * 2;

                    byte[] mapping = BitConverter.GetBytes(blockType);

                    data[index] = mapping[0];
                    data[index + 1] = mapping[1];
                }
                else
                {
                    // Create the mapping
                    byte[] mapping = BitConverter.GetBytes(blockType);

                    // Add the value at the end of the data array
                    data = [.. data, mapping[0], mapping[1]];


                    int index = x + (y * 16) + (z * 16 * 16);

                    data[index] = (byte)blockMap.Length;
                }
            }

        }

        if (type == ChunkType.Complex)
        {
            int index = (x + (y * 16) + (z * 16 * 16)) * 2;

            byte[] mapping = BitConverter.GetBytes(blockType);

            data[index] = mapping[0];
            data[index + 1] = mapping[1];
        }
    }

    public short GetBlock(int x, int y, int z)
    {
        // A simple chunk only has one byte.
        if (type == ChunkType.Single)
        {
            return BitConverter.ToInt16(data, 0);
        }

        // A chunk with only a few blocks, and a map
        if (type == ChunkType.Simple)
        {

            // We first need to know how many different types of blocks we have in this chunk.
            // There is a max of 256 different blocks in a simple chunk.
            short[] blockMap = new short[(data.Length - SIMPLE_CHUNK_DATA_SIZE) / 2];

            // First we fill out our block map.
            for (int i = 0; i < blockMap.Length; i++)
            {
                blockMap[i] = BitConverter.ToInt16(data, (i * 2) + SIMPLE_CHUNK_DATA_SIZE);
            }

            // Calculate the position of the block in memory
            int index = x + (y * 16) + (z * 16 * 16);

            // Get the index to the block
            byte mapIndex = data[index];

            // Return the correct block
            return blockMap[mapIndex];
        }

        // A chunk with more then 256 blocks (rare!)
        if (type == ChunkType.Complex)
        {
            // Calculate the position of the block in memory
            int index = (x + (y * 16) + (z * 16 * 16)) * 2;

            // Return the value at that postition
            return BitConverter.ToInt16(data, index);
        }

        // If all else fails (should never happen)
        throw new Exception("Invalid chunk type '" + type + "' could not be decoded.");

    }

    public void Optimize()
    {
        AttemptSimplify();
    }

    private void AttemptSimplify()
    {
        // Attempt lowering the indicies of the simple chunk mapping system.
        // And remove useless mappings
        if (type == ChunkType.Simple)
        {
            short[] blockMap = new short[(data.Length - SIMPLE_CHUNK_DATA_SIZE) / 2];
            bool[] isUsed = new bool[blockMap.Length];

            // First we fill out our block map.
            for (int i = 0; i < blockMap.Length; i++)
            {
                blockMap[i] = BitConverter.ToInt16(data, (i * 2) + SIMPLE_CHUNK_DATA_SIZE);
            }

            // Find all the places where the block type is used.
            for (int i = 0; i < SIMPLE_CHUNK_DATA_SIZE; i++)
            {
                isUsed[data[i]] = true;
            }

            // Remove any where the block is not used.
            for (int i = blockMap.Length - 1; i >= 0; i--)
            {
                if (!isUsed[i])
                {
                    for (int j = 0; j < SIMPLE_CHUNK_DATA_SIZE; j++)
                    {
                        if (data[j] > i)
                        {
                            data[j]--;
                        }
                    }

                    int byteOffset = (i * 2) + SIMPLE_CHUNK_DATA_SIZE;

                    data = [.. data.AsSpan(0, byteOffset), .. data.AsSpan(byteOffset + 2)];
                }
            }
        }

        if (type == ChunkType.Complex)
        {
            // Create the mappings list
            List<short> blocks = new List<short>();

            // Loop over every block of data, to see if what blocks are used
            for (int i = 0; i < SIMPLE_CHUNK_DATA_SIZE * 2; i += 2)
            {
                short block = BitConverter.ToInt16(data, i);

                if (!blocks.Contains(block))
                {
                    blocks.Add(block);
                }
            }

            // If there is a low enough number to turn it into a Simple, we convert it.
            if (blocks.Count < 256)
            {
                type = ChunkType.Simple;
                byte[] newData = new byte[SIMPLE_CHUNK_DATA_SIZE + (blocks.Count * 2)];
                for (int i = 0; i < SIMPLE_CHUNK_DATA_SIZE; i++)
                {
                    short block = BitConverter.ToInt16(data, i * 2);
                    newData[i] = (byte)blocks.IndexOf(block);
                }

                for (int i = 0; i < blocks.Count; i++)
                {
                    byte[] a = BitConverter.GetBytes(blocks[i]);

                    newData[SIMPLE_CHUNK_DATA_SIZE + (i * 2)] = a[0];
                    newData[SIMPLE_CHUNK_DATA_SIZE + (i * 2) + 1] = a[1];
                }

                data = newData;
                isDirty = true;
            }
        }

        // Attempt to convert simple to single
        if (type == ChunkType.Simple)
        {
            int length = (data.Length - SIMPLE_CHUNK_DATA_SIZE) / 2;

            if (length == 1)
            {
                type = ChunkType.Single;
                byte a = data[SIMPLE_CHUNK_DATA_SIZE];
                byte b = data[SIMPLE_CHUNK_DATA_SIZE + 1];

                data = new byte[2]
                {
                    a, b
                };

                isDirty = true;
            }
        }
    }

    public enum ChunkType : byte
    {
        /// <summary>
        /// The chunk is a single block, like ground or air
        /// </summary>
        Single = 0,

        /// <summary>
        /// The chunk has less then 256 different blocks
        /// </summary>
        Simple = 1,

        /// <summary>
        /// The chunk has more then 256 different blocks
        /// </summary>
        Complex = 2
    }


    public bool isDirty = true;
}