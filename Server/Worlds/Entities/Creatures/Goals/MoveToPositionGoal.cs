using Shared.Mathf;

namespace Server.Worlds;

public class MoveToPositionGoal : Goal
{
    private readonly int priority;
    private readonly Vector3 target;
    private readonly float stoppingDistance;

    public MoveToPositionGoal(
        Vector3 target,
        float stoppingDistance,
        int priority)
    {
        this.target = target;
        this.stoppingDistance = stoppingDistance;
        this.priority = priority;
    }

    public override int GetPriority()
    {
        return priority;
    }

    public override void Update()
    {
        if (Creature.HasPath)
            return;

        float distance = Distance(
            Creature.Position,
            target
        );

        if (distance <= stoppingDistance)
            return;

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
