namespace Client.Rendering;

using Client.Input;
using Client.Networking;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Shared.Mathf;
using Shared.Networking;
using Shared.Worlds;

public class InventoryUI
{
    public static int SelectedSlot = 0;

    private static UIRenderer hotbarSelector;
    private static List<UIRenderer> itemRenderers = new List<UIRenderer>();

    public static void CreateUI()
    {
        ImageTexture imageTexture = ImageTexture.LoadFromPng("Textures/Hotbar.png");
        UIRenderer hotbarRenderer = new UIRenderer();
        hotbarRenderer.SetTexture(imageTexture);

        hotbarRenderer.scale = 0.4f;
        hotbarRenderer.position = new Vector2(0.5f, 0.9f);

        GameCanvas.AddRenderer(hotbarRenderer);

        ImageTexture selector = ImageTexture.LoadFromPng("Textures/Selector.png");
        hotbarSelector = new UIRenderer();
        hotbarSelector.SetTexture(selector);
        hotbarSelector.scale = 1f / 9f;
        hotbarSelector.Parent = hotbarRenderer;
        hotbarSelector.position = new Vector2(0.5f, 0.5f);
        GameCanvas.AddRenderer(hotbarSelector);

        ImageTexture sheepTexture = ImageTexture.LoadFromPng("Textures/Sheep.png");

        for (int i = 0; i < 9; i++)
        {
            UIRenderer itemRenderer = new UIRenderer();
            itemRenderer.SetTexture(sheepTexture);
            itemRenderer.scale = 1f / 13f;
            itemRenderer.Parent = hotbarRenderer;
            itemRenderer.visible = false;
            float x = (i / 9f) + (1f / 18f);
            itemRenderer.position = new Vector2(x, 0.5f);

            itemRenderers.Add(itemRenderer);
            GameCanvas.AddRenderer(itemRenderer);
        }

        GameCanvas.OnUpdate += GameCanvas_OnUpdate;

        RenderData.OnItemTextureUpdated += RenderData_OnItemTextureUpdated;
        LocalInventory.OnLocalInventoryChange += LocalInventory_OnInventoryChange;
    }

    private static void LocalInventory_OnInventoryChange()
    {
        // 9 is the size of the hotbar.
        for (int i = 0; i < 9; i++)
        {
            Item? type = LocalInventory.GetItemType(i);

            // If the item type is null, we do not render ANYTHING
            if (type == null)
                SetHotbarTexture(i, null);
            else
            {
                // There is an item here, so we need to get the Texture to display that.
                SetHotbarTexture(i, type.texture);
            }
        }
    }

    private static bool isTextureLoaded = false;
    private static void RenderData_OnItemTextureUpdated()
    {
        isTextureLoaded = true;
        foreach (UIRenderer renderer in itemRenderers)
        {
            renderer.SetTexture((ImageTexture)RenderData.ItemTexture);
        }
    }

    public static void SetHotbarTexture(int slot, string? textureId)
    {
        if (!isTextureLoaded)
        {
            throw new Exception("Can not set Texture for hotbar slot before the items Texture atlas is loaded!");
        }

        if (textureId == null)
        {
            itemRenderers[slot].visible = false;
        }
        else
        {
            itemRenderers[slot].visible = true;
            itemRenderers[slot].SetUvs(RenderData.ItemTexturesMap.GetUV(textureId));
        }
    }

    private static void GameCanvas_OnUpdate()
    {
        int oldSlot = SelectedSlot;

        if (Mouse.Current.scroll.Y < 0)
            SelectedSlot++;

        if (Mouse.Current.scroll.Y > 0)
            SelectedSlot--;

        if (SelectedSlot < 0)
            SelectedSlot = 8;
        if (SelectedSlot > 8)
            SelectedSlot = 0;


        float x = (SelectedSlot / 9f) + (1f / 18f);
        hotbarSelector.position = new Vector2(x, 0.5f);

        KeyboardShortcut();


        if (oldSlot == SelectedSlot)
        {
            SelectSlotPacket selectSlotPacket = new SelectSlotPacket();
            selectSlotPacket.Slot = SelectedSlot;
            Network.SendPacket(selectSlotPacket.Write());
        }
    }

    private static void KeyboardShortcut()
    {
        Keys[] hotbarSlotShortcuts = new Keys[]
        {
            Keys.D1,
            Keys.D2,
            Keys.D3,
            Keys.D4,
            Keys.D5,
            Keys.D6,
            Keys.D7,
            Keys.D8,
            Keys.D9
        };

        for (int i = 0; i < 9; i++)
        {
            // See if the shortcut button is pressed.
            if (Keyboard.Current.IsPressedThisFrame(hotbarSlotShortcuts[i]))
                SelectedSlot = i;
        }
    }

}
