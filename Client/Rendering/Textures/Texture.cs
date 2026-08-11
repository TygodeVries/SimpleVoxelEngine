using OpenTK.Graphics.OpenGL;

namespace Client.Rendering;

/// <summary>
/// Texture taken from Dart game engine
/// </summary>
public abstract class Texture
{
    public abstract void Use(TextureUnit textureUnit);
}
