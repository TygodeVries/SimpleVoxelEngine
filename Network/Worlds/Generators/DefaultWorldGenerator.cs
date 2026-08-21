namespace Shared.Worlds;

public class DefaultWorldGenerator : WorldGenerator
{
    public override Block Generate(int x, int y, int z)
    {
        return DefaultBlocks.AIR;
    }
}
