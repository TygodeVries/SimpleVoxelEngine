using Shared.Mathf;
using Shared.Worlds;

namespace Server.Worlds.Items;

public class ItemBehaviour
{
    public static Action<ItemClickBlockArgs> PlaceBlock(Func<Block> block)
    {
        Console.WriteLine("An item uses");

        return (args) =>
        {
            PlayerEntity player = (PlayerEntity)args.Entity;
            player.AddCountInMainHand(-1);

            Vector3 pos = args.Block + args.Normal;
            args.GetWorld().SetBlockAt(block(), pos);
        };
    }

    public static void BreakBlock(ItemClickBlockArgs args)
    {
        if (args.Entity is PlayerEntity player)
        {
            player.StartBreakingBlock(args.Block);
        }
    }
}
