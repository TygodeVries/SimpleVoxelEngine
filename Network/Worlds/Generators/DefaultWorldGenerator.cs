namespace Shared.Worlds;

public class DefaultWorldGenerator : WorldGenerator
{
    public override short Generate(int x, int y, int z)
    {
        if (y < 0)
            return 1;
        return 0;
    }
}
