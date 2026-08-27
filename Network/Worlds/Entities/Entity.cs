using Shared.Mathf;

namespace Shared.Worlds;

public abstract class Entity
{
    public int Id { get; private set; }

    /// <summary>
    /// A bad function to call if you don't know what you are doing!!!
    /// </summary>
    /// <param Name="RegistryId"></param>
    public void SetId(int id)
    {
        this.Id = id;
    }
    private World? world;
    public World? GetWorld()
    {
        return world;
    }
    /// <summary>
    /// A bad function to call if you don't know what you are doing!!!
    /// </summary>
    /// <param Name="RegistryId"></param>
    public void SetWorld(World world)
    {
        this.world = world;
    }

    public Entity()
    {

    }

    public virtual void Tick() { }
    public virtual void OnSpawn() { }
    public virtual void OnDestroy() { }
    public Vector3 position = new Vector3();

    public Vector3 velocity = new Vector3();
    public Vector3 Size = new(0.6f, 1.8f, 0.6f);
    public void ApplyGravity()
    {
        // Subtract gravity.
        velocity -= Vector3.Up * Time.DeltaTime * 26.8f;
    }

    public bool IsGrounded { get; private set; }

    /// <summary>
    /// Taken from old project, did not want to write all this again.
    /// </summary>
    public void ApplyPhysics(bool canNotFallOffBlocks)
    {
        World? world = GetWorld();

        if (world == null)
        {
            Console.WriteLine("Can not run physics on an entity not assigned to a world!");
            return;
        }

        float dt = Time.DeltaTime;

        Vector3 half = new(Size.X * 0.5f, 0, Size.Z * 0.5f);

        // X
        float newX = position.X + (velocity.X * dt);

        float checkX = velocity.X > 0
            ? newX + half.X
            : newX - half.X;

        bool hitX = false;

        for (int y = (int)MathF.Floor(position.Y); y <= (int)MathF.Floor(position.Y + Size.Y - 0.001f); y++)
        {
            for (int z = (int)MathF.Floor(position.Z - half.Z); z <= (int)MathF.Floor(position.Z + half.Z); z++)
            {
                Block? block = GetWorld().GetBlockAt(
                    (int)MathF.Floor(checkX),
                    y,
                    z);

                if (block == null || block.Solid)
                {
                    hitX = true;
                    break;
                }

                if (canNotFallOffBlocks)
                {
                    bool hasSupport = false;

                    int minX = (int)MathF.Floor(newX - half.X);
                    int maxX = (int)MathF.Floor(newX + half.X - 0.001f);

                    int minZ = (int)MathF.Floor(position.Z - half.Z);
                    int maxZ = (int)MathF.Floor(position.Z + half.Z - 0.001f);

                    for (int supportX = minX; supportX <= maxX; supportX++)
                    {
                        for (int supportZ = minZ; supportZ <= maxZ; supportZ++)
                        {
                            Block? blockB = world.GetBlockAt(
                                supportX,
                                (int)MathF.Floor(position.Y) - 1,
                                supportZ);

                            if (blockB == null || blockB.Solid)
                            {
                                hasSupport = true;
                                break;
                            }
                        }

                        if (hasSupport)
                            break;
                    }

                    if (!hasSupport)
                    {
                        hitX = true;
                        break;
                    }
                }
            }

            if (hitX)
                break;
        }

        if (hitX)
            velocity.X = 0;
        else
            position.X = newX;

        // Y
        float newY = position.Y + (velocity.Y * dt);

        float checkY = velocity.Y > 0
            ? newY + Size.Y
            : newY;

        bool hitY = false;

        for (int x = (int)MathF.Floor(position.X - half.X); x <= (int)MathF.Floor(position.X + half.X); x++)
        {
            for (int z = (int)MathF.Floor(position.Z - half.Z); z <= (int)MathF.Floor(position.Z + half.Z); z++)
            {
                Block? block = world.GetBlockAt(
                    x,
                    (int)MathF.Floor(checkY),
                    z);

                if (block == null || block.Solid)
                {
                    hitY = true;
                    break;
                }
            }

            if (hitY)
                break;
        }

        if (hitY)
        {
            IsGrounded = true;
            velocity.Y = 0;
        }
        else
        {
            IsGrounded = false;
            position.Y = newY;
        }
        // Z
        float newZ = position.Z + (velocity.Z * dt);

        float checkZ = velocity.Z > 0
            ? newZ + half.Z
            : newZ - half.Z;

        bool hitZ = false;

        for (int y = (int)MathF.Floor(position.Y); y <= (int)MathF.Floor(position.Y + Size.Y - 0.001f); y++)
        {
            for (int x = (int)MathF.Floor(position.X - half.X); x <= (int)MathF.Floor(position.X + half.X); x++)
            {
                Block? block = world.GetBlockAt(
                    x,
                    y,
                    (int)MathF.Floor(checkZ));

                if (block == null || block.Solid)
                {
                    hitZ = true;
                    break;
                }

                if (canNotFallOffBlocks)
                {
                    bool hasSupport = false;

                    int minX = (int)MathF.Floor(position.X - half.X);
                    int maxX = (int)MathF.Floor(position.X + half.X - 0.001f);

                    int minZ = (int)MathF.Floor(newZ - half.Z);
                    int maxZ = (int)MathF.Floor(newZ + half.Z - 0.001f);

                    for (int supportX = minX; supportX <= maxX; supportX++)
                    {
                        for (int supportZ = minZ; supportZ <= maxZ; supportZ++)
                        {
                            Block? blockB = world.GetBlockAt(
                                supportX,
                                (int)MathF.Floor(position.Y) - 1,
                                supportZ);

                            if (blockB == null || blockB.Solid)
                            {
                                hasSupport = true;
                                break;
                            }
                        }

                        if (hasSupport)
                            break;
                    }

                    if (!hasSupport)
                    {
                        hitZ = true;
                        break;
                    }
                }
            }

            if (hitZ)
                break;
        }

        if (hitZ)
            velocity.Z = 0;
        else
            position.Z = newZ;
    }

    public void Destroy()
    {
        World? world = GetWorld();
        if (world == null)
            return;

        world.DestroyEntity(this);
    }
}
