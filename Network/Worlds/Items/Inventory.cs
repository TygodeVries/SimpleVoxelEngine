namespace Shared.Worlds;

public class Inventory
{
    private ItemStack?[] contents;

    public Inventory(int size)
    {
        contents = new ItemStack[size];
    }

    public void SetSlot(int slot, ItemStack? stack)
    {
        OnSlotSet?.Invoke(new OnSlotSetArgs(slot, stack));
        contents[slot] = stack;
    }

    public ItemStack? GetItem(int slot)
    {
        return contents[slot];
    }

    public bool AddItem(ItemStack stack)
    {
        for (int i = 0; i < contents.Length; i++)
        {
            if (contents[i] == null)
            {
                SetSlot(i, stack);
                return true;
            }
        }

        return false;
    }

    public event Action<OnSlotSetArgs>? OnSlotSet;
}


public class OnSlotSetArgs
{
    public int slot { get; private set; }
    public ItemStack? stack { get; private set; }

    public OnSlotSetArgs(int slot, ItemStack? itemStack)
    {
        this.slot = slot;
        this.stack = itemStack;
    }
}