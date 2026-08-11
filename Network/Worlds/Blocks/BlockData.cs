namespace Shared.Worlds;

public static class BlockData
{
    public static bool IsInvisible(short block)
    {
        if (block == 0)
            return true;

        return false;
    }

    public static bool IsSolid(short block)
    {
        if (block == 0)
            return false;

        return true;
    }
}
