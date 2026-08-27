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

    private int blockWidth = 16;
    private int blockHeight = 16;

    public int row;
    public int col;

    public TextureMap(List<string> ids, ImageTexture texture)
    {
        this.names = ids;

        for (int i = 0; i < names.Count; i++)
        {
            names[i] = names[i].ToLower();
        }

        width = texture.width;
        height = texture.height;

        row = width / blockWidth;
        col = height / blockHeight;
    }

    public TextureMap(int width, int height)
    {
        this.width = width;
        this.height = height;

        row = width / blockWidth;
        col = height / blockHeight;
    }

    private readonly Dictionary<(int textureId, BlockFace face), Vector2[]> _uvMappings = new();

    public void AddMapping(string textureId, short blockType, BlockFace face)
    {
        _uvMappings[(blockType, face)] = GetUV(textureId.ToLower());
    }

    public Vector2[] GetBlockTexture(int textureId, BlockFace face)
    {
        return _uvMappings.TryGetValue((textureId, face), out var uv)
            ? uv
            : new Vector2[0];
    }

    public Vector2[] GetUV(string textureName)
    {
        return GetUV(names.IndexOf(textureName.ToLower()));
    }

    public Vector2[] GetUV(int textureId)
    {
        if (textureId == -1)
        {
            throw new Exception("-1 is not a valid Texture RegistryId.");
        }
        int texX = textureId % row;
        int texY = textureId / row;

        float uvXmin = texX / (float)row;
        float uvYmin = texY / (float)col;

        float uvXmax = uvXmin + (blockWidth / (float)width);
        float uvYmax = uvYmin + (blockHeight / (float)height);

        return new[]
        {
        new Vector2(uvXmin, uvYmin),
        new Vector2(uvXmax, uvYmin),
        new Vector2(uvXmax, uvYmax),
        new Vector2(uvXmin, uvYmax)
    };
    }
}
