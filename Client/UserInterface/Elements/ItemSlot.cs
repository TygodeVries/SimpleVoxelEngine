using Client.Rendering;
using Shared.Mathf;
using Shared.Worlds;

namespace Client.UserInterface.Elements
{
    internal class ItemSlot : InterfaceElement
    {
        private UIImageRenderer? backfaceRenderer;
        private UIImageRenderer? itemRenderer;
        private UITextRenderer? countRenderer;

        public ItemStack? ItemStack { get; private set; }
        public void SetItemStack(ItemStack? itemStack)
        {
            this.ItemStack = itemStack;

            itemRenderer!.visible = itemStack != null;
            countRenderer!.visible = itemStack != null;
            if (itemStack == null)
                return;

            itemRenderer.SetUvs(RenderData.ItemTexturesMap!.GetUV(itemStack.Type.Texture!));

            countRenderer.SetText($"{itemStack.Count}");
        }

        public void SetPosition(Vector2 pos)
        {
            backfaceRenderer?.position = pos;
        }

        public Vector2 GetPosition()
        {
            return backfaceRenderer!.position;
        }

        public override void Create()
        {

            backfaceRenderer = new UIImageRenderer();
            backfaceRenderer.scale = 0.035f;
            itemRenderer = new UIImageRenderer();
            itemRenderer.scale = 0.9f;
            countRenderer = new UITextRenderer(RenderData.DefaultFont);

            if (RenderData.ItemTexture != null)
                itemRenderer.SetTexture(RenderData.ItemTexture);

            if (RenderData.UITexture != null)
            {
                backfaceRenderer.SetTexture(RenderData.UITexture);
                backfaceRenderer.SetUvs(RenderData.UITextureMap!.GetUV(0));
            }

            RenderData.OnItemTextureUpdated += () =>
            {
                itemRenderer.SetTexture(RenderData.ItemTexture);
            };

            backfaceRenderer.sort = 10;
            itemRenderer.sort = 15;
            countRenderer.sort = 20;

            countRenderer.TextWidth = 2;

            countRenderer.Parent = backfaceRenderer;
            itemRenderer.Parent = backfaceRenderer;
            countRenderer.scale = 0.02f;
            countRenderer.position = new Shared.Mathf.Vector2(0.6f, 0.7f);

            GameCanvas.AddRenderer(backfaceRenderer);
            GameCanvas.AddRenderer(itemRenderer);
            GameCanvas.AddRenderer(countRenderer);
        }

        public override void Destroy()
        {
            if (backfaceRenderer != null)
                GameCanvas.RemoveRenderer(backfaceRenderer);

            if (itemRenderer != null)
                GameCanvas.RemoveRenderer(itemRenderer);

            if (countRenderer != null)
                GameCanvas.RemoveRenderer(countRenderer);
        }
    }
}
