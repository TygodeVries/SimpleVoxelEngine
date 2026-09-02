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

    public bool isCrouch = false;

    public override void Tick()
    {
        ApplyGravity();
        Movement();

        ApplyPhysics(isCrouch && IsGrounded);
        Interact();

        isCrouch = Keyboard.Current.IsPressed(Keys.LeftShift);

        float playerHeight = 1.7f;

        if (isCrouch)
            playerHeight = 1.4f;


        Camera.Position = position + new Vector3(0, playerHeight, 0);

        if (Vector3.Distance(lastPacketPosition, position) > 0.3f)
        {
            PlayerMovePacket packet = new PlayerMovePacket();
            packet.X = position.X;
            packet.Y = position.Y;
            packet.Z = position.Z;
            lastPacketPosition = position;
            Network.SendPacket(packet.Write());
        }
    }


    private bool enableAO = false;
    private void Interact()
    {
        if (Keyboard.Current.IsPressedThisFrame(Keys.F9))
        {
            enableAO = !enableAO;
            int status = enableAO ? 1 : 0;
            RenderData.DefaultChunkShader.SetInt("u_DisableAO", status);
        }

        if (Mouse.Current.LeftPressedThisFrame())
        {
            PlayerInteractPacket playerInteractPacket = new PlayerInteractPacket();

            RaycastHit? hit = LocalWorld.World.Raycast(Camera.Position, Camera.Direction, 5);
            if (hit != null)
            {
                playerInteractPacket.InteractionType = InteractionType.LeftClickBlock;
                playerInteractPacket.BlockPos = hit.WorldBlockPos;
                playerInteractPacket.BlockNormal = hit.Normal;
            }
            else
            {
                playerInteractPacket.InteractionType = InteractionType.LeftClickAir;
            }

            Network.SendPacket(playerInteractPacket.Write());
        }

        if (Mouse.Current.RightPressedThisFrame())
        {
            PlayerInteractPacket playerInteractPacket = new PlayerInteractPacket();

            RaycastHit? hit = LocalWorld.World.Raycast(Camera.Position, Camera.Direction, 5);
            if (hit != null)
            {
                playerInteractPacket.InteractionType = InteractionType.RightClickBlock;
                playerInteractPacket.BlockPos = hit.WorldBlockPos;
                playerInteractPacket.BlockNormal = hit.Normal;
            }
            else
            {
                playerInteractPacket.InteractionType = InteractionType.RightClickAir;
            }

            Network.SendPacket(playerInteractPacket.Write());
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

        float speed = 4;
        if (isCrouch)
            speed = 2;

        velocity = tDirection * speed;
        velocity.Y = y;
    }

    private Vector3 lastPacketPosition = Vector3.Zero;
}