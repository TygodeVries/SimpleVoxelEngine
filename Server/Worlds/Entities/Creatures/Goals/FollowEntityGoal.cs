using Shared.Worlds;

namespace Server.Worlds;

public class FollowEntityGoal : Goal
{
    private int priority;
    private EntityType entityType;
    public float Range;
    public FollowEntityGoal(EntityType type, float range, int priority)
    {
        this.Range = range;
        this.entityType = type;
        this.priority = priority;
    }

    public override int GetPriority()
    {
        return priority;
    }

    public override void Update()
    {

        if (!Creature.HasPath)
        {
            foreach (Entity entity in Creature.GetWorld().GetEntitiesNear(Creature.Position, Range))
            {
                if (entity.GetEntityType() == entityType)
                {
                    Creature.Navigate(entity.Position);
                    return;
                }
            }
        }

        // No entity of this type near.
    }
}
