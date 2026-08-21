namespace Shared.Worlds;

public abstract class WorldGenerator
{
    public abstract Block Generate(int x, int y, int z);
}
