namespace Shared.Worlds;

public class Defaults
{
    public static EntityType ItemEntity = EntityType.Unregisterd;
    public static EntityType PlayerEntity = EntityType.Unregisterd;
    public static Block AirBlock = Block.Unregistered;

    public static void Register()
    {
        AirBlock = Registry.CreateBlock("air");
        AirBlock.Visible = false;
        AirBlock.Solid = false;

        PlayerEntity = Registry.CreateEntity("player");

        ItemEntity = Registry.CreateEntity("item");
    }
}
