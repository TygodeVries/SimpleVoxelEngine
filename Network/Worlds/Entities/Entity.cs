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
        OnEntityIdSet?.Invoke();
    }

    public event Action? OnEntityIdSet;

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
    public Vector3 Size { get; private set; } = new(0.6f, 1.8f, 0.6f);
    public void ApplyGravity()
    {
        // Subtract gravity.
        Velocity -= Vector3.Up * Time.DeltaTime * 26.8f;
    }

    public bool IsGrounded { get; private set; }

    public Vector3 Velocity { get; private set; } = Vector3.Zero;

    public event Action? OnSetVelocity;

    public void SetVelocity(Vector3 velocity, bool invokeEvent = true)
    {
        this.Velocity = velocity;

        if (invokeEvent)
            OnSetVelocity?.Invoke();
    }

    public void SetVelocity(float x, float y, float z, bool invokeEvent = true)
    {
        SetVelocity(new Vector3(x, y, z), invokeEvent);
    }


    public void SetVelocityX(float x, bool invokeEvent = true)
    {
        SetVelocity(new Vector3(
            x,
            Velocity.Y,
            Velocity.Z
        ), invokeEvent);
    }

    public void SetVelocityY(float y, bool invokeEvent = true)
    {
        SetVelocity(new Vector3(
            Velocity.X,
            y,
            Velocity.Z
        ), invokeEvent);
    }

    public void SetVelocityZ(float z, bool invokeEvent = true)
    {
        SetVelocity(new Vector3(
            Velocity.X,
            Velocity.Y,
            z
        ), invokeEvent);
    }


    public Vector3 Position { get; private set; } = Vector3.Zero;
    public event Action? OnTeleport;

    public void Teleport(float x, float y, float z)
    {
        Teleport(new Vector3(x, y, z));
    }
    public virtual void Teleport(Vector3 position)
    {
        this.Position = position;
        OnTeleport?.Invoke();
    }

    public void TeleportX(float x)
    {
        Teleport(new Vector3(
            x,
            this.Position.Y,
            this.Position.Z
        ));
    }

    public void TeleportY(float y)
    {
        Teleport(new Vector3(
            this.Position.X,
            y,
            this.Position.Z
        ));
    }

    public void TeleportZ(float z)
    {
        Teleport(new Vector3(
            this.Position.X,
            this.Position.Y,
            z
        ));
    }


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
        float newX = Position.X + (Velocity.X * dt);

        float checkX = Velocity.X > 0
            ? newX + half.X
            : newX - half.X;

        bool hitX = false;

        for (int y = (int)MathF.Floor(Position.Y); y <= (int)MathF.Floor(Position.Y + Size.Y - 0.001f); y++)
        {
            for (int z = (int)MathF.Floor(Position.Z - half.Z); z <= (int)MathF.Floor(Position.Z + half.Z); z++)
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

                    int minZ = (int)MathF.Floor(Position.Z - half.Z);
                    int maxZ = (int)MathF.Floor(Position.Z + half.Z - 0.001f);

                    for (int supportX = minX; supportX <= maxX; supportX++)
                    {
                        for (int supportZ = minZ; supportZ <= maxZ; supportZ++)
                        {
                            Block? blockB = world.GetBlockAt(
                                supportX,
                                (int)MathF.Floor(Position.Y) - 1,
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
            SetVelocityX(0, false);
        else
            TeleportX(newX);

        // Y
        float newY = Position.Y + (Velocity.Y * dt);

        float checkY = Velocity.Y > 0
            ? newY + Size.Y
            : newY;

        bool hitY = false;

        for (int x = (int)MathF.Floor(Position.X - half.X); x <= (int)MathF.Floor(Position.X + half.X); x++)
        {
            for (int z = (int)MathF.Floor(Position.Z - half.Z); z <= (int)MathF.Floor(Position.Z + half.Z); z++)
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
            SetVelocityY(0, false);
        }
        else
        {
            IsGrounded = false;
            TeleportY(newY);
        }
        // Z
        float newZ = Position.Z + (Velocity.Z * dt);

        float checkZ = Velocity.Z > 0
            ? newZ + half.Z
            : newZ - half.Z;

        bool hitZ = false;

        for (int y = (int)MathF.Floor(Position.Y); y <= (int)MathF.Floor(Position.Y + Size.Y - 0.001f); y++)
        {
            for (int x = (int)MathF.Floor(Position.X - half.X); x <= (int)MathF.Floor(Position.X + half.X); x++)
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

                    int minX = (int)MathF.Floor(Position.X - half.X);
                    int maxX = (int)MathF.Floor(Position.X + half.X - 0.001f);

                    int minZ = (int)MathF.Floor(newZ - half.Z);
                    int maxZ = (int)MathF.Floor(newZ + half.Z - 0.001f);

                    for (int supportX = minX; supportX <= maxX; supportX++)
                    {
                        for (int supportZ = minZ; supportZ <= maxZ; supportZ++)
                        {
                            Block? blockB = world.GetBlockAt(
                                supportX,
                                (int)MathF.Floor(Position.Y) - 1,
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
            SetVelocityZ(0, false);
        else
            TeleportZ(newZ);
    }

    public void Destroy()
    {
        World? world = GetWorld();
        if (world == null)
            return;

        world.DestroyEntity(this);
    }
}
