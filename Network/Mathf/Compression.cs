using System.IO.Compression;
namespace Shared.Mathf;

public class Compression
{

    public static byte[] Compress(byte[] bytes)
    {
        using var output = new MemoryStream();

        using (var deflate = new DeflateStream(
            output,
            CompressionLevel.Fastest,
            leaveOpen: true))
        {
            deflate.Write(bytes, 0, bytes.Length);
        }

        return output.ToArray();
    }

    public static byte[] Decompress(byte[] bytes)
    {
        using var input = new MemoryStream(bytes);
        using var deflate = new DeflateStream(
            input,
            CompressionMode.Decompress);

        using var output = new MemoryStream();

        deflate.CopyTo(output);

        return output.ToArray();
    }
}
