using OpenTK.Mathematics;
using Shared.Mathf;

namespace Shared.Worlds;

public abstract class Entity
{
    public int Id { get; private set; }

    /// <summary>
    /// A bad function to call if you don't know what you are doing!!!
    /// </summary>
    /// <param name="id"></param>
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
    /// <param name="id"></param>
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
    public Vector3 position;

    public Vector3 velocity;
    public Vector3 Size = new(0.6f, 1.8f, 0.6f);
    public void ApplyGravity()
    {
        // Subtract gravity.
        velocity -= Vector3.UnitY * Time.DeltaTime * 20.8f;
    }

    public bool IsGrounded { get; private set; }

    /// <summary>
    /// Taken from old project, did not want to write all this again.
    /// </summary>
    public void ApplyPhysics()
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
                if (BlockData.IsSolid(GetWorld().GetBlockAt(
                    (int)MathF.Floor(checkX),
                    y,
                    z)))
                {
                    hitX = true;
                    break;
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
                if (BlockData.IsSolid(world.GetBlockAt(
                    x,
                    (int)MathF.Floor(checkY),
                    z)))
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
                if (BlockData.IsSolid(world.GetBlockAt(
                    x,
                    y,
                    (int)MathF.Floor(checkZ))))
                {
                    hitZ = true;
                    break;
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
