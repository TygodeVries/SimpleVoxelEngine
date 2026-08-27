

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

public class EntityMoveArgs
{
    public Vector3 lastPosition { get; private set; }
    public Vector3 newPosition { get; private set; }

    public EntityMoveArgs(Vector3 lastPosition, Vector3 newPosition)
    {
        this.lastPosition = lastPosition;
        this.newPosition = newPosition;
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