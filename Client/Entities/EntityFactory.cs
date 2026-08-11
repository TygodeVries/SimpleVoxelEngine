using Shared.Worlds;

namespace Client.Entities;

public class EntityFactory
{
    public static Entity CreateEntity(EntityType entityType)
    {
        if (entityType == EntityType.Player)
            return new OnlinePlayer();


        throw new Exception("No entity locally of this type.");
    }
}
