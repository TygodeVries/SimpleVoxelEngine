namespace Shared.Worlds;

public class DefaultBlocks
{
    public static Block AIR;

    public static void Register()
    {
        AIR = Registry.CreateBlock("air");
        AIR.Visible = false;
        AIR.Solid = false;
    }
}
