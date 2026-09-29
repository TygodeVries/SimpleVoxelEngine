using Shared.Worlds;

namespace Server.Worlds.Entities
{
    public class ItemEntity : ServerEntity
    {
        private ItemStack itemStack;
        public ItemEntity(ItemStack itemStack)
        {
            this.itemStack = itemStack;
            SetItemStack(itemStack);
            this.used = false;
        }

        public void SetItemStack(ItemStack itemStack)
        {
            this.itemStack = itemStack;

            SetMetadata("item", itemStack.Type.Identifier);
        }

        public ItemStack GetItemStack()
        {
            return itemStack;
        }

        public override EntityType GetEntityType()
        {
            return Defaults.ItemEntity;
        }

        private bool used = false;
        public override void Tick()
        {
            base.Tick();
            if (used)
                return;

            ApplyGravity();
            ApplyPhysics(false);
            ApplyDrag();

            World? world = GetWorld();
            if (world == null)
                return;

            List<Entity> entities = world.GetEntitiesNear(Position, 1);
            foreach (Entity entity in entities)
            {
                if (entity is PlayerEntity player)
                {
                    int remaining = player.Inventory.AddItem(itemStack);
                    if (remaining != 0)
                    {
                        itemStack.Count = remaining;
                        return;
                    }

                    used = true;
                    Destroy();
                    return;
                }
            }
        }
    }
}
