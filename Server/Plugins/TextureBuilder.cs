using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Server.Plugins;

public class TextureBuilder
{
    private readonly List<string> textureIds = new List<string>();
    private readonly List<byte[]> textures = new();

    public int TextureResolution { get; private set; }

    public string AddTexture(string path)
    {
        lock (textures)
        {
            string name = Path.GetFileNameWithoutExtension(path);

            textureIds.Add(name);
            textures.Add(File.ReadAllBytes(path));

            return name;
        }
    }

    public List<string> GetNames()
    {
        return textureIds;
    }

    public byte[] GetTexture()
    {
        if (textures.Count == 0)
        {
            Console.WriteLine($"[WARN] Texture Builder is empty");
            Image img = new Image<Rgba32>(1, 1);
            MemoryStream stream = new MemoryStream();
            img.SaveAsPng(stream);
            return stream.ToArray();
        }

        using var firstImage = Image.Load<Rgba32>(textures[0]);

        int textureSize = firstImage.Width;

        if (firstImage.Height != textureSize)
            throw new InvalidOperationException(
                "Textures must be square.");

        TextureResolution = textureSize;

        var images = textures
            .Select(x => Image.Load<Rgba32>(x))
            .ToList();

        try
        {
            var invalidTexture = images.FirstOrDefault(x =>
                x.Width != textureSize ||
                x.Height != textureSize);

            if (invalidTexture is not null)
            {
                throw new InvalidOperationException(
                    $"All textures must be square and have the same dimensions. " +
                    $"Server texture size is {textureSize}x{textureSize}, but the texture is " +
                    $"{invalidTexture.Width}x{invalidTexture.Height}.");
            }

            int columns = (int)Math.Ceiling(Math.Sqrt(images.Count));
            int rows = (int)Math.Ceiling(
                (double)images.Count / columns);

            using var atlas = new Image<Rgba32>(
                textureSize * columns,
                textureSize * rows);

            for (int i = 0; i < images.Count; i++)
            {
                int x = i % columns * textureSize;
                int y = i / columns * textureSize;

                atlas.Mutate(ctx =>
                    ctx.DrawImage(
                        images[i],
                        new Point(x, y),
                        1f));
            }

            using var output = new MemoryStream();

            atlas.SaveAsPng(output);

            return output.ToArray();
        }
        finally
        {
            foreach (var image in images)
                image.Dispose();
        }
    }
}