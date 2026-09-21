using Shared.Worlds;

namespace Server.Worlds;

public class FollowBlockGoal : Goal
{
    private int priority;
    private Block block;
    public int Range;
    public FollowBlockGoal(Block type, int range, int priority)
    {
        this.Range = range;
        this.block = type;
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
            for (int x = -Range; x < Range; x++)
                for (int y = -Range; y < Range; y++)
                    for (int z = -Range; z < Range; z++)
                    {
                        if (Creature.GetWorld().GetBlockAt(Creature.Position.iX + x, Creature.Position.iY + y, Creature.Position.iZ + z) == block)
                        {
                            Creature.Navigate(Creature.Position + new Shared.Mathf.Vector3(x, y, z));
                            return;
                        }
                    }
        }

        // No entity of this type near.
    }
}
