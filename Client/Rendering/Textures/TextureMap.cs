using Shared.Mathf;
using Shared.Worlds;

namespace Client.Rendering;


/// <summary>
/// Mapping a map to UV coordinates
/// </summary>
public class TextureMap
{
    private List<string> names = new List<string>();
    private int width = 0;
    private int height = 0;

    public int row;
    public int col;

    public TextureMap(List<string> ids, ImageTexture texture, int resolution)
    {
        this.names = ids;

        for (int i = 0; i < names.Count; i++)
        {
            names[i] = names[i].ToLower();
        }

        width = texture.width;
        height = texture.height;

        row = width / resolution;
        col = height / resolution;
    }

    public bool TextureExists(string texture)
    {
        return names.Contains(texture);
    }

    public TextureMap(ImageTexture texture, int resolution)
    {
        this.width = texture.width;
        this.height = texture.height;

        row = width / resolution;
        col = height / resolution;
    }

    public TextureMap(int width, int height, int resolution)
    {
        this.width = width;
        this.height = height;

        row = width / resolution;
        col = height / resolution;
    }

    private Dictionary<(short blockType, BlockFace face, bool normal), string> mappings = new Dictionary<(short blockType, BlockFace face, bool normal), string>();

    public void AddMapping(string textureId, short blockType, BlockFace face)
    {
        mappings.Add((blockType, face, textureId.EndsWith("_normal")), textureId);
    }

    public Vector2[] GetBlockTexture(short blockType, BlockFace face, bool normal, Vector2? size = null, Vector2? offset = null)
    {
        return GetUV(mappings[(blockType, face, normal)], size, offset);
    }

    public bool HasNormal(short blockType, BlockFace face)
    {
        return mappings.ContainsKey((blockType, face, true));
    }

    public Vector2[] GetUV(string textureName, Vector2? size = null, Vector2? offset = null)
    {
        int index = names.IndexOf(textureName.ToLower());
        if (index == -1)
        {
            throw new FileNotFoundException($"The texture {textureName} could not be found!");
        }

        return GetUV(index, size, offset);
    }

    public Vector2[] GetUV(int textureId, Vector2? size = null, Vector2? offset = null)
    {
        if (size == null)
            size = new Vector2(1, 1);

        if (offset == null)
            offset = new Vector2(0, 0);

        if (textureId < 0 || textureId >= row * col)
        {
            throw new ArgumentOutOfRangeException(
                nameof(textureId),
                "Texture ID is outside the texture atlas."
            );
        }

        int texX = textureId % row;
        int texY = textureId / row;

        float tileWidth = 1f / row;
        float tileHeight = 1f / col;

        float uvXmin = (texX * tileWidth) + (offset.Value.X * tileWidth);
        float uvYmin = (texY * tileHeight) + (offset.Value.Y * tileHeight);

        float uvXmax = uvXmin + (size.Value.X * tileWidth);
        float uvYmax = uvYmin + (size.Value.Y * tileHeight);

        return new[]
        {
        new Vector2(uvXmin, uvYmin),
        new Vector2(uvXmax, uvYmin),
        new Vector2(uvXmax, uvYmax),
        new Vector2(uvXmin, uvYmax)
     };
    }
}

