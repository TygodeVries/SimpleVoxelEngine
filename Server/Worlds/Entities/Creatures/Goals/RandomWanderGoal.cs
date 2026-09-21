namespace Server.Worlds;

public class RandomWanderGoal : Goal
{
    private int priority;
    public RandomWanderGoal(int priority)
    {
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
            Creature.Navigate(Creature.Position + new Shared.Mathf.Vector3(Random.Shared.Next(-5, 5), Random.Shared.Next(-5, 5), Random.Shared.Next(-5, 5)));
            return;
        }
    }
}
