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
    private MeshRenderer renderer = new MeshRenderer(RenderData.EntityShader!);

    // Track smoothed visual position/rotation separately from the physical entity.
    private Vector3 visualPosition;
    private Vector3 visualRotation;
    private bool isFirstFrame = true;

    public Vector3 ModelOffset { get; protected set; } = new Vector3();

    /// <summary>
    /// The speed multiplier for position/rotation interpolation.
    /// Higher values snap faster; lower values are smoother.
    /// </summary>
    public float SmoothSpeed { get; set; } = 5.0f;

    public void SetRenderDoublesided(bool doubleSided)
    {
        renderer.doubleSided = doubleSided;
    }

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
        Vector3 targetPosition = new Vector3(
            Position.X,
            Position.Y,
            Position.Z
        );

        Vector3 targetRotation = new Vector3(
            Rotation.X,
            Rotation.Y,
            Rotation.Z
        );

        if (isFirstFrame)
        {
            visualPosition = targetPosition;
            visualRotation = targetRotation;
            isFirstFrame = false;
        }
        else
        {
            float alpha = Clamp(
                Time.DeltaTime * SmoothSpeed,
                0.0f,
                1.0f
            );

            visualPosition = Vector3.Lerp(
                visualPosition,
                targetPosition,
                alpha
            );

            visualRotation = Vector3.Lerp(
                visualRotation,
                targetRotation,
                alpha
            );
        }

        Matrix4 modelMatrix =
            Matrix4.CreateScale(1) *
            Matrix4.CreateRotationX(MathHelper.DegreesToRadians(visualRotation.X)) *
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians(visualRotation.Y)) *
            Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(visualRotation.Z)) *
            Matrix4.CreateTranslation(
                visualPosition.ToOpenTK() +
                ModelOffset.ToOpenTK()
            );

        renderer.SetModelMatrix(modelMatrix);
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