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

    private const int MAX_STACK_COUNT = 10;

    public int AddItem(ItemStack stack)
    {

        int maxStackCount = stack.Type.MaxStackCount;
        int remaining = stack.Count;

        for (int i = 0; i < contents.Length && remaining > 0; i++)
        {
            ItemStack? content = contents[i];

            if (content != null && content.Type == stack.Type)
            {
                int space = maxStackCount - content.Count;

                if (space <= 0)
                    continue;

                int amount = Math.Min(remaining, space);

                content.Count += amount;
                remaining -= amount;

                SetSlot(i, content);
            }
        }

        for (int i = 0; i < contents.Length && remaining > 0; i++)
        {
            if (contents[i] == null)
            {
                int amount = Math.Min(remaining, maxStackCount);

                ItemStack newStack = new ItemStack(stack.Type)
                {
                    Count = amount
                };

                SetSlot(i, newStack);
                remaining -= amount;
            }
        }

        return remaining;
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