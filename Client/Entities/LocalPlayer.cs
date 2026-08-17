using Client.Input;
using Client.Networking;
using Client.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Shared.Mathf;
using Shared.Networking;
using Shared.Worlds;
namespace Client.Entities;

public class LocalPlayer : Entity
{
    public LocalPlayer()
    {
        position = new Vector3(0.5f, 30, 0.5f);
    }

    public override void Tick()
    {
        ApplyGravity();
        Movement();

        ApplyPhysics();
        Break();
        Place();

        Camera.Position = position + new Vector3(0, 1.7f, 0);

        if (Vector3.Distance(lastPacketPosition, position) > 0.1f)
        {
            PlayerMovePacket packet = new PlayerMovePacket();
            packet.X = position.X;
            packet.Y = position.Y;
            packet.Z = position.Z;
            lastPacketPosition = position;
            Network.SendPacket(packet.Write());
        }
    }

    private void Break()
    {
        if (Mouse.Current.LeftPressedThisFrame())
        {
            RaycastHit? hit = LocalWorld.World.Raycast(Camera.Position, Camera.Direction, 5);
            if (hit == null)
                return;

            LocalWorld.World.SetBlockAt(0, hit.WorldBlockPos.iX, hit.WorldBlockPos.iY, hit.WorldBlockPos.iZ);


            PlaceBlockPacket packet = new PlaceBlockPacket();
            packet.X = hit.WorldBlockPos.iX;
            packet.Y = hit.WorldBlockPos.iY;
            packet.Z = hit.WorldBlockPos.iZ;
            packet.Type = 0;

            Network.SendPacket(packet.Write());

        }
    }

    private void Place()
    {
        if (Mouse.Current.RightPressedThisFrame())
        {
            RaycastHit? hit = LocalWorld.World.Raycast(Camera.Position, Camera.Direction, 5);
            if (hit == null)
                return;

            Vector3 pos = hit.WorldBlockPos + hit.Normal;

            LocalWorld.World.SetBlockAt(1, (int)pos.X, (int)pos.Y, (int)pos.Z);

            PlaceBlockPacket packet = new PlaceBlockPacket();
            packet.X = (int)pos.X;
            packet.Y = (int)pos.Y;
            packet.Z = (int)pos.Z;
            packet.Type = 1;

            Network.SendPacket(packet.Write());
        }
    }

    private void Movement()
    {
        // Respawn thing #TODO move to server
        if (position.Y < -100)
        {
            position = new Vector3(0, 100, 0);
            velocity.Y = 0;
        }


        if (Keyboard.Current.IsPressedThisFrame(Keys.Escape))
        {
            GameCanvas.Unlock();
        }

        if (Mouse.Current.LeftPressedThisFrame())
        {
            GameCanvas.Lock();
        }

        Vector3 direction = Vector3.Zero;
        if (Keyboard.Current.IsPressed(Keys.W))
        {
            direction.Z += 1;
        }

        if (Keyboard.Current.IsPressed(Keys.S))
        {
            direction.Z -= 1;
        }

        if (Keyboard.Current.IsPressed(Keys.A))
        {
            direction.X -= 1;
        }

        if (Keyboard.Current.IsPressed(Keys.D))
        {
            direction.X += 1;
        }

        if (Keyboard.Current.IsPressed(Keys.Space) && IsGrounded)
        {
            velocity.Y += 8;
        }

        Vector3 tDirection = Camera.Translate(direction);

        float y = velocity.Y;
        velocity = tDirection * 4;
        velocity.Y = y;
    }

    private Vector3 lastPacketPosition = Vector3.Zero;
}