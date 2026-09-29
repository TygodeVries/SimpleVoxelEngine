using Client.Input;
using Client.Networking;
using Client.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Shared.Mathf;
using Shared.Networking;
using Shared.Networking.Packets.Writers;
using Shared.Worlds;
namespace Client.Entities;

public class LocalPlayer : Entity
{
    public const bool IS_FLYMODE = true;

    public bool isCrouch = false;
    public override void Tick()
    {
        if (!GetWorld().IsChunkLoaded(Position / 16) && !dontSendPackets)
        {
            return;
        }

        ApplyGravity();
        Movement();

        if (!dontSendPackets)
        {
            ApplyPhysics(isCrouch && IsGrounded);
        }
        else
        {
            Teleport(Position + (Velocity * Time.DeltaTime));
        }
        Interact();

        isCrouch = Keyboard.Current.IsPressed(Keys.LeftShift);

        float playerHeight = 1.7f;

        if (isCrouch)
            playerHeight = 1.4f;

        if (Keyboard.Current.IsPressedThisFrame(Keys.F5))
        {
            CommandPacket packet = new CommandPacket();
            packet.Command = "reload";
            Network.SendPacket(packet.Write());
        }


        Camera.Position = Position + new Vector3(0, playerHeight, 0);
        if (Vector3.Distance(lastPacketPosition, Position) > 0.3f && !dontSendPackets)
        {
            PlayerMovePacket packet = new PlayerMovePacket();
            packet.X = Position.X;
            packet.Y = Position.Y;
            packet.Z = Position.Z;
            lastPacketPosition = Position;
            Network.SendPacket(packet.Write());
        }

        if (Keyboard.Current.IsPressedThisFrame(Keys.K))
        {
            dontSendPackets = !dontSendPackets;


            if (dontSendPackets)
                NormalSpeed = 30;
            else
                NormalSpeed = 4;
        }
    }

    private bool dontSendPackets = false;


    private int renderDebugMode = 0;
    private void Interact()
    {
        if (Keyboard.Current.IsPressedThisFrame(Keys.Q))
        {
            Network.SendPacket(new DropItemPacket().Write());
        }

        if (Keyboard.Current.IsPressedThisFrame(Keys.F9))
        {
            renderDebugMode++;
            if (renderDebugMode == 7)
                renderDebugMode = 0;
            RenderData.DefaultChunkShader.SetInt("u_Debug", renderDebugMode);
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

        if (Mouse.Current.LeftReleasedThisFrame())
        {
            PlayerInteractPacket playerInteractPacket = new PlayerInteractPacket();
            playerInteractPacket.InteractionType = InteractionType.ReleaseLeftMouse;
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

        if (Keyboard.Current.IsPressed(Keys.Space) && (IsGrounded || IS_FLYMODE))
        {
            SetVelocityY(8);
        }

        Vector3 tDirection = Camera.Translate(direction);

        float y = Velocity.Y;

        float speed = NormalSpeed;
        if (isCrouch)
            speed = CrouchSpeed;

        SetVelocity(tDirection * speed);
        SetVelocityY(y);
    }

    public float NormalSpeed = 4;
    public float CrouchSpeed = 2;

    private Vector3 lastPacketPosition = Vector3.Zero;

    public override EntityType GetEntityType()
    {
        return EntityType.Unregisterd;
    }
}