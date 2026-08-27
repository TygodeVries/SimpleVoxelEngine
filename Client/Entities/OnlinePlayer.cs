using Client.Rendering;
using SimpleVoxelEngine.Entities;

namespace Client.Entities;

public class OnlinePlayer : VisibleEntity
{
    public OnlinePlayer()
    {
        Mesh? mesh = Mesh.FromFileObj("Models/Player.obj");
        if (mesh == null)
        {
            Console.WriteLine("Could not load player model!");
        }

        SetMesh(mesh);
        SetTexture(RenderData.PlayerTexture);
    }

    public override void Tick()
    {
        ApplyVisuals();
    }
}
