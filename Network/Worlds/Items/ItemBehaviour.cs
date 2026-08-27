
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
}
