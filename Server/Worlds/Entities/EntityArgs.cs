

using Shared.Mathf;

namespace Server.Worlds;

public class PlayerChangeSlotArgs
{
    public int lastSlot { get; private set; }
    public int Slot { get; private set; }

    public PlayerChangeSlotArgs(int lastSlot, int slot)
    {
        this.lastSlot = lastSlot;
        this.Slot = slot;
    }
}

public class PlayerClickBlocksArgs
{
    public Vector3 Block { get; set; }
    public Vector3 Normal { get; set; }

    public PlayerClickBlocksArgs(Vector3 block, Vector3 normal)
    {
        this.Block = block;
        this.Normal = normal;
    }
}