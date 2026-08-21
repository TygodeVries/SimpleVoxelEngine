namespace Shared.Worlds;

public class BlockTextureAtlas
{
    public static List<string> textureNames = new List<string>();

    public static int GetTextureId(string textureName)
    {
        int index = textureNames.IndexOf(textureName); ;
        if (index == -1)
        {
            Console.WriteLine($"The block atlas does not contain a texture with the name {textureName}.");
        }
        return index;
    }
}
