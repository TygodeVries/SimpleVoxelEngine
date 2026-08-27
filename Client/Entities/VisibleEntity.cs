using Client.Rendering;
using Shared.Mathf;
using Shared.Worlds;
using Matrix4 = OpenTK.Mathematics.Matrix4;
namespace SimpleVoxelEngine.Entities;

/// <summary>
/// An entity that has a visible mesh with interpolated rendering.
/// </summary>
public abstract class VisibleEntity : Entity
{
    private MeshRenderer renderer = new MeshRenderer(RenderData.DefaultChunkShader!);

    // Track the smoothed visual position separately from the physical entity position
    private Vector3 visualPosition;
    private bool isFirstFrame = true;

    /// <summary>
    /// The speed multiplier for the interpolation. 
    /// Higher values snap faster; lower values are smoother.
    /// </summary>
    public float SmoothSpeed { get; set; } = 5.0f;

    public VisibleEntity()
    {
    }

    public void SetMesh(Mesh mesh)
    {
        renderer.SetMesh(mesh);
        GameCanvas.AddRenderer(renderer);
    }

    public void SetTexture(Texture texture)
    {
        renderer.Texture = texture;
    }

    public override void OnDestroy()
    {
        GameCanvas.RemoveRenderer(renderer);
    }

    public void ApplyVisuals()
    {
        Vector3 targetPosition = new Vector3(position.X, position.Y, position.Z);

        if (isFirstFrame)
        {
            visualPosition = targetPosition;
            isFirstFrame = false;
        }
        else
        {
            float alpha = Clamp(Time.DeltaTime * SmoothSpeed, 0.0f, 1.0f);
            visualPosition = Vector3.Lerp(visualPosition, targetPosition, alpha);
        }

        renderer.SetModelMatrix(Matrix4.CreateTranslation(visualPosition.ToOpenTK()) * Matrix4.CreateScale(1));
    }

    private float Clamp(float a, float min, float max)
    {
        if (a < min)
            return min;
        if (a > max)
            return max;
        return a;
    }
}