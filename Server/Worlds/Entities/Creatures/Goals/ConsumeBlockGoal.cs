using Shared.Worlds;

namespace Server.Worlds;

public class ConsumeBlockGoal : Goal
{
    private readonly int priority;
    private readonly Block block;

    public int Range;

    public ConsumeBlockGoal(Block type, int range, int priority)
    {
        Range = range;
        block = type;
        this.priority = priority;
    }

    public override int GetPriority()
    {
        return HasBlockNearby() ? priority : 0;
    }

    public override void Update()
    {
        if (!Creature.HasPath)
        {
            for (int x = -Range; x < Range; x++)
                for (int y = -Range; y < Range; y++)
                    for (int z = -Range; z < Range; z++)
                    {
                        int blockX = Creature.Position.iX + x;
                        int blockY = Creature.Position.iY + y;
                        int blockZ = Creature.Position.iZ + z;

                        if (Creature.GetWorld().GetBlockAt(
                                blockX,
                                blockY,
                                blockZ) == block)
                        {
                            Creature.GetWorld().SetBlockAt(
                                Defaults.AirBlock,
                                blockX,
                                blockY,
                                blockZ
                            );

                            return;
                        }
                    }
        }
    }

    private bool HasBlockNearby()
    {
        for (int x = -Range; x < Range; x++)
            for (int y = -Range; y < Range; y++)
                for (int z = -Range; z < Range; z++)
                {
                    int blockX = Creature.Position.iX + x;
                    int blockY = Creature.Position.iY + y;
                    int blockZ = Creature.Position.iZ + z;

                    if (Creature.GetWorld().GetBlockAt(
                            blockX,
                            blockY,
                            blockZ) == block)
                    {
                        return true;
                    }
                }

        return false;
    }
}
