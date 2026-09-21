namespace Shared.Worlds;

public class DefaultEntities
{
    public static EntityType Player { get; set; }

    public static void Register()
    {
        Player = Registry.CreateEntity("player");
    }
}
