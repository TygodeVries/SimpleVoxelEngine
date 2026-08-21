using Shared.Mathf;
using Matrix4 = OpenTK.Mathematics.Matrix4;
namespace Client.Rendering;

public class Camera
{
    public static float aspectRatio = 1.0f;
    public static float Fov = 80;

    public static Vector3 Position = Vector3.Zero;
    public static Vector3 Direction = Vector3.Forwards; // Default looking forward (-Z)

    public static bool IsBoxInView(Vector3 center, Vector3 size)
    {
        Vector3 halfSize = size * 0.5f;

        Vector3 forward = Direction.Normalized;

        Vector3 worldUp = Vector3.Up;

        if (MathF.Abs(Vector3.Dot(forward, worldUp)) > 0.999f)
            worldUp = Vector3.Right;

        Vector3 right = Vector3.Cross(worldUp, forward).Normalized;
        Vector3 up = Vector3.Cross(forward, right).Normalized;

        Vector3 relative = center - Position;

        float z = Vector3.Dot(relative, forward);
        float x = Vector3.Dot(relative, right);
        float y = Vector3.Dot(relative, up);

        float extentX =
            (MathF.Abs(right.X) * halfSize.X) +
            (MathF.Abs(right.Y) * halfSize.Y) +
            (MathF.Abs(right.Z) * halfSize.Z);

        float extentY =
            (MathF.Abs(up.X) * halfSize.X) +
            (MathF.Abs(up.Y) * halfSize.Y) +
            (MathF.Abs(up.Z) * halfSize.Z);

        float extentZ =
            (MathF.Abs(forward.X) * halfSize.X) +
            (MathF.Abs(forward.Y) * halfSize.Y) +
            (MathF.Abs(forward.Z) * halfSize.Z);

        float halfFovY = Fov * 0.5f * MathF.PI / 180.0f;

        float tanY = MathF.Tan(halfFovY);

        float tanX = tanY * aspectRatio;

        if (z + extentZ < 0)
            return false;

        if (x - extentX > z * tanX)
            return false;

        if (x + extentX < -z * tanX)
            return false;

        if (y - extentY > z * tanY)
            return false;

        if (y + extentY < -z * tanY)
            return false;

        return true;
    }

    public bool IsPointInView(Vector3 point)
    {
        Vector3 toPoint = point - Position;

        float forward = Vector3.Dot(Direction, toPoint);
        if (forward <= 0)
            return false;

        float verticalHalfFov = Fov * 0.5f;
        float horizontalHalfFov = MathF.Atan(
            MathF.Tan(verticalHalfFov * MathF.PI / 180.0f) * aspectRatio
        ) * 180.0f / MathF.PI;

        Vector3 directionToPoint = toPoint.Normalized;

        float angle = MathF.Acos(
            Math.Clamp(Vector3.Dot(Direction, directionToPoint), -1.0f, 1.0f)
        ) * 180.0f / MathF.PI;

        return angle <= horizontalHalfFov;
    }

    public static Matrix4 GetViewMatrix()
    {
        // Target is Current Position offset by the forward Direction
        return Matrix4.LookAt(Position.ToOpenTK(), Position.ToOpenTK() + Direction.ToOpenTK(), Vector3.Up.ToOpenTK());
    }

    public static Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(Fov),
            aspectRatio,
            0.1f,
            4000.0f
        );
    }

    public static Vector3 Right
    {
        get
        {
            return Vector3.Cross(Direction, Vector3.Up).Normalized;
        }
    }

    public static Vector3 Up
    {
        get
        {
            return Vector3.Cross(Right, Direction).Normalized;
        }
    }

    /// <summary>
    /// Move based on local camera orientation
    /// </summary>
    /// <param name="delta">X = Strafe Right, Y = Global Up, Z = Move Forward</param>
    public static Vector3 Translate(Vector3 delta)
    {
        Vector3 flatDirection = Direction;
        flatDirection.Y = 0;
        flatDirection.Normalize();

        Vector3 output = new Vector3(0, 0, 0);
        output += Right * delta.X;
        output += Vector3.Up * delta.Y;
        output += flatDirection * delta.Z;

        return output;
    }
}