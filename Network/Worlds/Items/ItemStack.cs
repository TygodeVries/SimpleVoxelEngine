namespace Shared.Worlds;

public class ItemStack
{
    public Item Type;
    public int Count;

    public ItemStack(Item type, int count)
    {
        Type = type;
        Count = count;
    }

    public ItemStack(Item type)
    {
        Type = type;
        Count = 1;
    }
}
