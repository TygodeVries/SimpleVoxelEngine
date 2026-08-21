namespace Shared.Worlds;

public class DefaultBlocks
{
    public static Block AIR;

    public static void Register()
    {
        AIR = Registry.CreateBlock("air");
        AIR.isVisible = false;
        AIR.isSolid = false;
    }
}
