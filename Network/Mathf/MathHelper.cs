namespace Shared.Mathf;

public class MathHelper
{
    public static float DegreesToRadians(float degrees)
    {
        return degrees * (MathF.PI / 180f);
    }

    public static float RadiansToDegrees(float radians)
    {
        return radians * (180f / MathF.PI);
    }
}
