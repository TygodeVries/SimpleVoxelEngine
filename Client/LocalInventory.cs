using Shared.Worlds;

namespace Client;

public class LocalInventory
{
    private static ItemStack?[] items = new ItemStack[9];

    public static void SetItem(int slot, ItemStack? item)
    {

        items[slot] = item;
        OnLocalInventoryChange?.Invoke();
        Console.WriteLine($"Local inventory changed, slot {slot} now has item {item.Type.Name}");
    }

    public static Item? GetItemType(int slot)
    {
        ItemStack? itemStack = items[slot];
        if (itemStack == null)
            return null;

        return itemStack.Type;
    }

    public static event Action? OnLocalInventoryChange;
}
