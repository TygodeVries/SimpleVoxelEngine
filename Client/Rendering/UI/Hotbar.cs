namespace Client.Rendering;

using Client.Input;
using Client.Networking;
using Client.UserInterface;
using Client.UserInterface.Elements;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Shared.Networking;
using Shared.Worlds;

public class Hotbar
{
    private static int Selected = -1;
    private static List<ItemSlot> InventorySlots = new List<ItemSlot>();
    private static UIImageRenderer SelectorIcon = new UIImageRenderer();
    public static void Create()
    {
        SelectorIcon = new UIImageRenderer();
        SelectorIcon.SetTexture(RenderData.UITexture);
        SelectorIcon.SetUvs(RenderData.UITextureMap.GetUV(1));
        SelectorIcon.sort = 19;
        SelectorIcon.scale = 0.035f;
        GameCanvas.AddRenderer(SelectorIcon);

        GameCanvas.OnUpdate += Update;

        for (int i = 0; i < 9; i++)
        {
            int slot = i;
            ItemSlot itemSlot = new ItemSlot();
            GameInterface.AddElement(itemSlot);

            itemSlot.SetPosition(new Shared.Mathf.Vector2((i * 0.04f) + 0.5f, 0.9f));

            LocalInventory.OnLocalInventoryChange += () =>
            {
                ItemStack? itemStack = LocalInventory.GetItem(slot);
                itemSlot.SetItemStack(itemStack);
            };

            InventorySlots.Add(itemSlot);
        }

        SetSlot(0);
    }

    private static void Update()
    {
        if (InventorySlots.Count == 0)
            return;

        Keys[] keys = new Keys[]
        {
            Keys.D1,
            Keys.D2,
            Keys.D3,
            Keys.D4,
            Keys.D5,
            Keys.D6,
            Keys.D7,
            Keys.D8,
            Keys.D9,
            Keys.D0,
        };

        for (int i = 0; i < keys.Length; i++)
        {
            if (Keyboard.Current.IsPressedThisFrame(keys[i]))
            {
                SetSlot(i);
            }
        }
    }

    public static void SetSlot(int slot)
    {
        if (slot == Selected)
            return;

        Selected = slot;
        SelectorIcon.position = InventorySlots[Selected].GetPosition();
        SelectSlotPacket selectSlotPacket = new SelectSlotPacket();
        selectSlotPacket.Slot = Selected;
        Network.SendPacket(selectSlotPacket.Write());
    }
}
