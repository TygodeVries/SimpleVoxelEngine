using Shared.Worlds;

namespace Client;

public class LocalInventory
{
    private static ItemStack?[] items = new ItemStack[9];

    public static void SetItem(int slot, ItemStack? item)
    {
        items[slot] = item;
        OnLocalInventoryChange?.Invoke();
    }

    public static ItemStack? GetItem(int slot)
    {
        return items[slot];
    }

    public static Item? GetItemType(int slot)
    {
        ItemStack? itemStack = items[slot];
        if (itemStack == null)
            return null;

        return itemStack.Type;
    }

    public static void Clear()
    {
        items = new ItemStack[9];
        OnLocalInventoryChange?.Invoke();
    }

    public static void ForceUpdate()
    {
        OnLocalInventoryChange?.Invoke();
    }
    public static event Action? OnLocalInventoryChange;
}
