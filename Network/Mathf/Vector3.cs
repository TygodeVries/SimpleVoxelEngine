namespace Shared.Mathf;

public struct Vector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public int iX => (int)MathF.Floor(X);
    public int iY => (int)MathF.Floor(Y);
    public int iZ => (int)MathF.Floor(Z);

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3()
    {
        X = 0;
        Y = 0;
        Z = 0;
    }

    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        return new Vector3(
            a.X + b.X,
            a.Y + b.Y,
            a.Z + b.Z
        );
    }

    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3(
            a.X - b.X,
            a.Y - b.Y,
            a.Z - b.Z
        );
    }

    public static Vector3 operator *(Vector3 a, float scalar)
    {
        return new Vector3(
            a.X * scalar,
            a.Y * scalar,
            a.Z * scalar
        );
    }

    public static Vector3 operator -(Vector3 vector)
    {
        return new Vector3(
            -vector.X,
            -vector.Y,
            -vector.Z
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
                (Y * Y) +
                (Z * Z)
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
            return (X * X) + (Y * Y) + (Z * Z);
        }
    }

    public static Vector3 Up
    {
        get
        {
            return new Vector3(0, 1, 0);
        }
    }

    public static Vector3 Right
    {
        get
        {
            return new Vector3(1, 0, 0);
        }
    }

    public static Vector3 Zero
    {
        get
        {
            return new Vector3(0, 0, 0);
        }
    }

    public static Vector3 Forwards
    {
        get
        {
            return new Vector3(0, 0, -1);
        }
    }

    public OpenTK.Mathematics.Vector3 ToOpenTK()
    {
        return new OpenTK.Mathematics.Vector3(X, Y, Z);
    }


    public void Normalize()
    {
        float length = Length;

        if (length == 0)
            return;

        X /= length;
        Y /= length;
        Z /= length;
    }

    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        return a + ((b - a) * t);
    }

    /// <summary>
    /// Returns a normalized version of this vector.
    /// </summary>
    public Vector3 Normalized
    {
        get
        {
            float length = Length;

            if (length == 0)
                return new Vector3(0, 0, 0);

            return new Vector3(
                X / length,
                Y / length,
                Z / length
            );
        }
    }

    /// <summary>
    /// Calculates the dot product between two vectors.
    /// </summary>
    public static float Dot(Vector3 a, Vector3 b)
    {
        return
            (a.X * b.X) +
            (a.Y * b.Y) +
            (a.Z * b.Z);
    }

    /// <summary>
    /// Calculates the cross product between two vectors.
    /// </summary>
    public static Vector3 Cross(Vector3 a, Vector3 b)
    {
        return new Vector3(
            (a.Y * b.Z) - (a.Z * b.Y),
            (a.Z * b.X) - (a.X * b.Z),
            (a.X * b.Y) - (a.Y * b.X)
        );
    }

    /// <summary>
    /// Calculates the distance between two points.
    /// </summary>
    public static float Distance(Vector3 a, Vector3 b)
    {
        float x = a.X - b.X;
        float y = a.Y - b.Y;
        float z = a.Z - b.Z;

        return MathF.Sqrt(
            (x * x) +
            (y * y) +
            (z * z)
        );
    }

    public static Vector3 operator /(Vector3 vector, float scalar)
    {
        return new Vector3(
            vector.X / scalar,
            vector.Y / scalar,
            vector.Z / scalar
        );
    }

    public Vector3 Floor()
    {
        return new Vector3((float)Math.Floor(X), (float)Math.Floor(Y), (float)Math.Floor(Z));
    }

    public float EuclideanLengthSquared
    {
        get
        {
            return (X * X) + (Y * Y) + (Z * Z);
        }
    }
}
