using Client.Rendering;
using Shared.Worlds;

namespace Client.Entities;

public class EntityFactory
{
    public static Entity CreateEntity(EntityType entityType)
    {
        if (entityType == DefaultEntities.Player)
        {
            return new OnlinePlayer();
        }

        Console.WriteLine($"EntityFactory: {entityType.Name}");

        GenericOnlineEntity visibleEntity = new GenericOnlineEntity(entityType);
        Console.WriteLine("Mesh is " + entityType.Mesh);
        if (entityType.Mesh == "?")
            return visibleEntity;
        visibleEntity.SetMesh(RenderData.models[entityType.Mesh]);
        return visibleEntity;
    }
}
