using Client.Rendering;
using Shared.Mathf;
using Shared.Worlds;
using SimpleVoxelEngine.Entities;

namespace Client.Entities
{
    internal class LocalItemEntity : VisibleEntity
    {
        public LocalItemEntity()
        {
            Mesh? mesh = Mesh.CreateCube(0.2f);
            if (mesh == null)
            {
                Console.WriteLine("Could not load item model!");
            }

            SetMesh(mesh);
            SetTexture(RenderData.ItemTexture!);

            SetRenderDoublesided(true);
        }

        public override EntityType GetEntityType()
        {
            return Defaults.ItemEntity;
        }

        private float time = 0;

        public override void Tick()
        {
            base.Tick();
            Rotation.Y += Time.DeltaTime * 40f;
            time += Time.DeltaTime;
            ModelOffset = new Vector3(0, (MathF.Cos(time) / 10) + 0.3f, 0);
            ApplyVisuals();
        }

        public override void OnMetadataChange(string key, string value)
        {
            Mesh? mesh = Mesh.CreateQuad(0.4f, 0.4f, uvs: RenderData.ItemTexturesMap!.GetUV(Registry.GetItem(value)!.Texture!));
            if (mesh == null)
            {
                Console.WriteLine("Could not load item model!");
            }

            SetMesh(mesh);
        }
    }
}
