using Shared.Mathf;
using Shared.Worlds;

namespace Server.Worlds;

public class AvoidEntityGoal : Goal
{
    private readonly int priority;
    private readonly EntityType entityType;
    private readonly float range;
    private readonly float fleeDistance;

    public AvoidEntityGoal(
        EntityType entityType,
        float range,
        float fleeDistance,
        int priority)
    {
        this.entityType = entityType;
        this.range = range;
        this.fleeDistance = fleeDistance;
        this.priority = priority;
    }

    public override int GetPriority()
    {
        return priority;
    }

    public override void Update()
    {
        Entity? closest = null;
        float closestDistance = float.MaxValue;

        foreach (Entity entity in Creature.GetWorld()
                     .GetEntitiesNear(Creature.Position, range))
        {
            if (entity.GetEntityType() != entityType)
                continue;

            float distance = Distance(
                Creature.Position,
                entity.Position
            );

            if (distance < closestDistance)
            {
                closest = entity;
                closestDistance = distance;
            }
        }

        if (closest == null)
            return;

        // Don't constantly replace an existing path.
        if (Creature.HasPath)
            return;

        Vector3 direction =
            Creature.Position - closest.Position;

        float length = MathF.Sqrt(
            (direction.X * direction.X) +
            (direction.Y * direction.Y) +
            (direction.Z * direction.Z)
        );

        if (length <= 0.001f)
            return;

        direction /= length;

        Vector3 target =
            Creature.Position + (direction * fleeDistance);

        Creature.Navigate(target);
    }

    private static float Distance(Vector3 a, Vector3 b)
    {
        float x = a.X - b.X;
        float y = a.Y - b.Y;
        float z = a.Z - b.Z;

        return MathF.Sqrt((x * x) + (y * y) + (z * z));
    }
}
