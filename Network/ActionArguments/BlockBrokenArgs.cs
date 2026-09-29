using Shared.Mathf;
using Shared.Worlds;

namespace Shared.ActionArguments;

public class BlockBrokenArgs
{
    public Vector3 Position;
    public World World;

    public BlockBrokenArgs(Vector3 position, World world)
    {
        this.Position = position;
        this.World = world;
    }
}
