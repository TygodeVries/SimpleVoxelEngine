namespace Shared.Mathf;

public struct Vector2
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
        return new Vector2(
            a.X + b.X,
            a.Y + b.Y
        );
    }

    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
        return new Vector2(
            a.X - b.X,
            a.Y - b.Y
        );
    }

    public static Vector2 operator *(Vector2 a, float scalar)
    {
        return new Vector2(
            a.X * scalar,
            a.Y * scalar
        );
    }

    /// <summary>
    /// The length (magnitude) of this vector.
    /// </summary>
    public float Length
    {
        get
        {
            return MathF.Sqrt(
                (X * X) +
                (Y * Y)
            );
        }
    }

    /// <summary>
    /// The squared length of this vector.
    /// Faster than Length because it avoids the square root.
    /// </summary>
    public float LengthSquared
    {
        get
        {
            return (X * X) + (Y * Y);
        }
    }

    public static Vector2 Up
    {
        get
        {
            return new Vector2(0, 1);
        }
    }

    public static Vector2 Zero
    {
        get
        {
            return new Vector2(0, 0);
        }
    }

    public OpenTK.Mathematics.Vector2 ToOpenTK()
    {
        return new OpenTK.Mathematics.Vector2(X, Y);
    }


    public void Normalize()
    {
        float length = Length;

        if (length == 0)
            return;

        X /= length;
        Y /= length;
    }

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return a + ((b - a) * t);
    }

    /// <summary>
    /// Returns a normalized version of this vector.
    /// </summary>
    public Vector2 Normalized
    {
        get
        {
            float length = Length;

            if (length == 0)
                return new Vector2(0, 0);

            return new Vector2(
                X / length,
                Y / length
            );
        }
    }

    /// <summary>
    /// Calculates the dot product between two vectors.
    /// </summary>
    public static float Dot(Vector2 a, Vector2 b)
    {
        return
            (a.X * b.X) +
            (a.Y * b.Y);
    }

    /// <summary>
    /// Calculates the distance between two points.
    /// </summary>
    public static float Distance(Vector2 a, Vector2 b)
    {
        float x = a.X - b.X;
        float y = a.Y - b.Y;

        return MathF.Sqrt(
            (x * x) +
            (y * y)
        );
    }

    public static Vector2 operator /(Vector2 vector, float scalar)
    {
        return new Vector2(
            vector.X / scalar,
            vector.Y / scalar
        );
    }

    public Vector2 Floor()
    {
        return new Vector2((float)Math.Floor(X), (float)Math.Floor(Y));
    }

    public float EuclideanLengthSquared
    {
        get
        {
            return (X * X) + (Y * Y);
        }
    }
}
