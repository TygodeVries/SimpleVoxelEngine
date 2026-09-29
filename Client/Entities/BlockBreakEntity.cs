using Client.Rendering;
using Shared.Mathf;
using Shared.Worlds;
using SimpleVoxelEngine.Entities;

namespace Client.Entities
{
    public class BlockBreakEntity : VisibleEntity
    {
        public BlockBreakEntity()
        {
            SetTexture(RenderData.UITexture);
            SetMesh(Mesh.CreateCube(1.01f, RenderData.UITextureMap.GetUV(2)));
        }

        public override void Tick()
        {
            ApplyVisuals();
        }

        public override EntityType GetEntityType()
        {
            return EntityType.Unregisterd;
        }

        public override void OnMetadataChange(string key, string value)
        {
            SetMesh(Mesh.CreateCube(1.01f, RenderData.UITextureMap.GetUV(2 + int.Parse(value))));
        }
    }
}
