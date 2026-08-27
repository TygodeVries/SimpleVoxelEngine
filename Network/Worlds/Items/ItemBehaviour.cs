
using Shared.Mathf;

namespace Shared.Worlds;

public class ItemBehaviour
{
    public static Action<ItemClickBlockArgs> PlaceBlock(Block block)
    {
        return (args) =>
        {
            Vector3 pos = args.Block + args.Normal;
            args.GetWorld().SetBlockAt(block, pos);
        };
    }

    public static Action<ItemClickBlockArgs> BreakBlock()
    {
        return (args) =>
        {
            args.GetWorld().BreakBlock(args.Block);
        };
    }
}
