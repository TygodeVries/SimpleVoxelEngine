using Shared.Worlds;
using SimpleVoxelEngine.Entities;

namespace Client.Entities;

public class GenericOnlineEntity : VisibleEntity
{
    private EntityType type;
    public GenericOnlineEntity(EntityType type)
    {
        this.type = type;
    }
    public override EntityType GetEntityType()
    {
        return type;
    }

    public override void Tick()
    {
        ApplyVisuals();
    }
}
