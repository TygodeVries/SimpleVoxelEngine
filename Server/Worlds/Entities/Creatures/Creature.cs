
using Shared.Mathf;

namespace Server.Worlds;

public abstract class Creature : ServerEntity
{
    public Creature()
    {
        OnWorldChange += WhenWorldChange;
    }

    private void WhenWorldChange()
    {
        if (GetWorld() == null)
            return;

        PathFinder = new PathFinder(GetWorld()!);
        goals = new List<Goal>();

        SetGoals();
    }

    private List<Goal> goals = new List<Goal>();
    public Memory Memory { get; set; } = new Memory();
    public PathFinder? PathFinder { get; private set; }
    public abstract void SetGoals();

    public void AddGoal(Goal goal)
    {
        goal.Creature = this;
        goals.Add(goal);
    }

    private Goal? currentGoal;

    public Goal? GetGoal()
    {
        return goals
            .Where(x => x.CanStart())
            .MaxBy(x => x.GetPriority());
    }

    public override void Tick()
    {
        Execute();
        FollowPath();
        ApplyGravity();
        ApplyPhysics(false);
    }

    public void Execute()
    {
        Goal? nextGoal = GetGoal();

        if (nextGoal == null)
            return;

        if (nextGoal != currentGoal)
        {
            currentGoal?.Stop();

            currentGoal = nextGoal;
            currentGoal.Start();
        }

        currentGoal.Update();
    }

    private List<Vector3>? currentPath;
    private int pathIndex;

    public void Navigate(Vector3 position)
    {
        if (PathFinder == null)
            return;

        currentPath = PathFinder.FindPath(Position, position);

        pathIndex = 0;
    }

    public bool HasPath =>
        currentPath != null &&
        pathIndex < currentPath.Count;


    private void FollowPath()
    {
        if (!HasPath)
            return;

        Vector3 target = currentPath![pathIndex];

        MoveTowards(target);

        if (Vector3.Distance(Position, target) < 0.1f)
        {
            pathIndex++;

            if (pathIndex >= currentPath.Count)
            {
                SetVelocity(0, 0, 0);
                currentPath = null;
            }
        }
    }
    public float Speed { get; set; } = 8;

    private void MoveTowards(Vector3 target)
    {
        Vector3 v = (target - Position).Normalized;
        v.Y *= 4;
        SetVelocity(v * Speed);
    }

}
