using OpenTK.Mathematics;

namespace Client.Rendering;

public class Camera
{
    public static float aspectRatio = 1.0f;
    public static float Fov = 80;

    public static Vector3 Position = Vector3.Zero;
    public static Vector3 Direction = -Vector3.UnitZ; // Default looking forward (-Z)

    public static Matrix4 GetViewMatrix()
    {
        // Target is Current Position offset by the forward Direction
        return Matrix4.LookAt(Position, Position + Direction, Vector3.UnitY);
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
            return Vector3.Normalize(Vector3.Cross(Direction, Vector3.UnitY));
        }
    }

    public static Vector3 Up
    {
        get
        {
            return Vector3.Normalize(Vector3.Cross(Right, Direction));
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

        Vector3 output = new Vector3();
        output += Right * delta.X;
        output += Vector3.UnitY * delta.Y;
        output += flatDirection * delta.Z;

        return output;
    }
}