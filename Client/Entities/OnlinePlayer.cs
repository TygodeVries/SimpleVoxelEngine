using Client.Rendering;
using SimpleVoxelEngine.Entities;

namespace Client.Entities;

public class OnlinePlayer : VisibleEntity
{
    public OnlinePlayer()
    {
        Mesh? mesh = Mesh.FromFileObj("Models/Monkey.obj");
        if (mesh == null)
        {
            Console.WriteLine("Could not load player model!");
        }

        SetMesh(mesh);
    }

    public override void Tick()
    {
        ApplyVisuals();
    }
}
