using Shared.ActionArguments;
using Shared.Mathf;
using Shared.Worlds;

namespace Server.Worlds.Items;

public class BlockBehaviour
{
    public static Action<BlockBrokenArgs> DropItem(Func<Item> item)
    {
        return (args) =>
        {
            Vector3 pos = args.Position;
            args.World.DropItem(pos, new ItemStack(item()), new Vector3(
                Random.Shared.Next(-3, 4),
                4,
                Random.Shared.Next(-3, 4)));
        };
    }
}
