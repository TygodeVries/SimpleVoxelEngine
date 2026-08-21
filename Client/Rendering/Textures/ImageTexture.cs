using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Client.Rendering;


/// <summary>
/// Image Texture taken from Dart game engine
/// </summary>
public class ImageTexture : Texture
{
    private byte[] pixels;
    public int width;
    public int height;
    public int Handle = 0;
    public bool isUploaded = false;
    public byte[] GetPixels()
    {
        return pixels;
    }
    public ImageTexture(int width, int height, byte[] pixels)
    {
        this.width = width;
        this.height = height;
        this.pixels = pixels;
    }


    private static Dictionary<string, ImageTexture> cache = new Dictionary<string, ImageTexture>();
    public static void RemoveFromCache(string path)
    {
        cache.Remove(path);
    }

    public static ImageTexture LoadFromBytes(byte[] bytes, int maxWidth = 8192, int maxHeight = 8192, bool upload = true, bool useCache = true, bool flip = false)
    {
        Image<Rgba32> image = Image.Load<Rgba32>(bytes);

        int newWidth = image.Width;
        int newHeight = image.Height;

        if (image.Width > maxWidth || image.Height > maxHeight)
        {
            float ratioX = (float)maxWidth / image.Width;
            float ratioY = (float)maxHeight / image.Height;
            float ratio = Math.Min(ratioX, ratioY);

            newWidth = (int)(image.Width * ratio);
            newHeight = (int)(image.Height * ratio);

            image.Mutate(x => x.Resize(newWidth, newHeight));
            Console.WriteLine($"Resized image from {image.Width}x{image.Height} to {newWidth}x{newHeight}");
        }

        if (flip)
        {
            image.Mutate(x => x.Flip(FlipMode.Vertical));
        }

        byte[] pixels = new byte[4 * image.Width * image.Height];
        image.CopyPixelDataTo(pixels);
        image.Dispose();

        ImageTexture texture = new ImageTexture(image.Width, image.Height, pixels);
        if (upload) texture.Upload();

        return texture;
    }

    public static ImageTexture LoadFromPng(string path, int maxWidth = 8192, int maxHeight = 8192, bool upload = true, bool useCache = true, bool flip = false)
    {
        if (cache.ContainsKey(path) && useCache)
        {
            return cache[path];
        }

        if (!File.Exists(path))
        {
            Console.WriteLine($"Failed to load image from path {path}. File does not exist!");
            return null;
        }

        byte[] fa = File.ReadAllBytes(path);

        return LoadFromBytes(fa, maxWidth, maxHeight, upload, useCache, flip);

    }

    public void Upload()
    {
        if (isUploaded)
            return;
        isUploaded = true;

        if (Handle == 0)
        {
            Handle = GL.GenTexture();
        }

        GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);

        GL.BindTexture(TextureTarget.Texture2D, Handle);

        GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.TexParameterf(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameterf(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);

        GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        if (GL.GetError() != ErrorCode.NoError)
            Console.WriteLine($"OpenGL has an error uploading a texture: {GL.GetError()}");

    }

    public override void Use(TextureUnit textureUnit)
    {
        if (!isUploaded)
            Upload();
        GL.ActiveTexture(textureUnit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }

    ~ImageTexture()
    {
        if (Handle != 0)
        {
            GL.DeleteTexture(Handle);
            Handle = 0;
        }
    }
}
