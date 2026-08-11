namespace Client.Rendering;

using Client.Input;
using Client.Networking;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Shared.Mathf;
using System.Diagnostics;

public class GameCanvas : GameWindow
{
    public static bool DebugEnabled = false;
    public GameCanvas(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {

    }

    public static float Width;
    public static float Height;

    /// <summary>
    /// Runs right after Run() is called on the window.
    /// Aka at startup.
    /// </summary>
    protected override void OnLoad()
    {

        // ** Setup a bunch of stuff **
        // Enable depth drawing
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.DepthFunc(DepthFunction.Lequal);

        // Load the default data into memory, like shaders, and setup vertex meshes on the GPU.
        RenderData.SetupDefaults();

        // ** Graphic Settings **

        // Set the background color
        GL.ClearColor(0, 0, 1, 1);

        // Enable the shadows
        // Shadow.Enable();

        // Start the stopwatch for the FPS counter
        fpsCounterStopwatch.Start();

        LocalWorld.World.OnAddChunk += World_OnAddChunk;
        GameCanvas.Width = width;
        GameCanvas.Height = height;


        AddRenderer(new UIRenderer(RenderData.UIShader));
    }

    private void World_OnAddChunk(Shared.Worlds.Chunk obj)
    {
        ChunkRenderer chunkRenderer = new ChunkRenderer(obj);
        GameCanvas.AddRenderer(chunkRenderer);
    }

    private Stopwatch fpsCounterStopwatch = new Stopwatch();
    private int frameCount = 0;

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        // Keep track of the framerate
        frameCount++;
        if (fpsCounterStopwatch.Elapsed.TotalSeconds > 1)
        {
            // Print the FPS every second
            fpsCounterStopwatch.Restart();
            Console.WriteLine("FPS: " + frameCount);

            Title = $"Game --- FPS: {frameCount}";
            frameCount = 0;
        }

        // Clear the background
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Render the shadows if enabled
        if (Shadow.ShadowsEnabled)
            Shadow.RenderShadows();

        // Render at the correct resolution, gets reset if shadows are rendered.
        GL.Viewport(0, 0, width, height);

        Matrix4 viewMatrix = Camera.GetViewMatrix();
        Matrix4 projectionMatrix = Camera.GetProjectionMatrix();

        Matrix4 ortho = Matrix4.CreateOrthographicOffCenter(
    0,
    GameCanvas.Width,
    GameCanvas.Height,
    0,
    -1,
    1
);

        // Loop over every thing we are rendering
        foreach (Renderer renderer in renderers)
        {
            // Render that thing.

            ShaderProgram? shader = renderer.GetShader();

            if (shader == null)
                continue;

            bool isUI = shader.useOrthoProjection;

            if (isUI)
            {
                GL.Disable(EnableCap.DepthTest);
            }
            else
            {
                GL.Enable(EnableCap.DepthTest);
            }

            shader.SetMatrix4("u_Model", renderer.GetModelMatrix());
            shader.SetMatrix4("u_View", viewMatrix);

            if (shader.useOrthoProjection)
            {
                shader.SetMatrix4("u_Projection", ortho);
            }
            else
            {
                shader.SetMatrix4("u_Projection", projectionMatrix);
            }
            // Pass shadow data only if enabled
            if (Shadow.ShadowsEnabled)
            {
                shader.SetMatrix4("u_LightSpaceMatrix", Shadow.lightSpaceMatrix);

                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, Shadow.depthGl);
                shader.SetTextureId("shadowMap", 1);
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            renderer.Render(false);
        }

        // Say we are ready to show the frame!
        SwapBuffers();
    }


    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        Camera.aspectRatio = e.Width / (float)e.Height;

        width = e.Width;
        height = e.Height;

        GameCanvas.Width = width;
        GameCanvas.Height = height;
    }

    private int width;
    private int height;

    /// <summary>
    /// The list of things we are rendering.
    /// </summary>
    private static List<Renderer> renderers = new List<Renderer>();

    public static List<Renderer> GetRenderers()
    {
        return renderers;
    }

    /// <summary>
    /// Add a thing to renderer
    /// </summary>
    /// <param name="renderer"></param>
    public static void AddRenderer(Renderer renderer)
    {
        renderers.Add(renderer);

        renderers.Sort((a, b) =>
        {
            return a.sort - b.sort;
        });
    }

    /// <summary>
    /// Remove a thing to render
    /// </summary>
    /// <param name="renderer"></param>
    public static void RemoveRenderer(Renderer renderer)
    {
        renderers.Remove(renderer);
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        Keyboard.Current.SetKeysState(e.Key, true);
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        Keyboard.Current.SetKeysState(e.Key, false);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if ((float)args.Time < 0.1f)
            Time.DeltaTime = (float)args.Time;


        if (Keyboard.Current.IsPressedThisFrame(OpenTK.Windowing.GraphicsLibraryFramework.Keys.F7))
        {
            int currentId = Environment.ProcessId;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath,
                UseShellExecute = true,
                CreateNoWindow = false,
                Arguments = $"--depends {currentId}"
            };

            Process.Start(startInfo);
        }

        LocalWorld.World.Tick();

        Keyboard.Current.EndOfFrame();
        Mouse.Current.EndOfFrame();
        Network.Tick();

        if (isLocked)
        {
            CursorState = CursorState.Grabbed;
        }
        else
        {
            CursorState = CursorState.Normal;
        }

    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left)
            Mouse.Current.leftPressed = true;

        if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right)
            Mouse.Current.rightPressed = true;

        if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle)
            Mouse.Current.middlePressed = true;
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left)
            Mouse.Current.leftPressed = false;

        if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right)
            Mouse.Current.rightPressed = false;

        if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle)
            Mouse.Current.middlePressed = false;
    }

    protected override void OnMouseMove(OpenTK.Windowing.Common.MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);

        float yaw = MathF.Atan2(Camera.Direction.Z, Camera.Direction.X) * (180f / MathF.PI);
        float pitch = MathF.Asin(Clamp(Camera.Direction.Y, -1f, 1f)) * (180f / MathF.PI);

        yaw += e.DeltaX * 0.4f;
        pitch -= e.DeltaY * 0.4f;

        pitch = Clamp(pitch, -89f, 89f);

        float yawRad = MathHelper.DegreesToRadians(yaw);
        float pitchRad = MathHelper.DegreesToRadians(pitch);

        Vector3 newDirection;
        newDirection.X = MathF.Cos(pitchRad) * MathF.Cos(yawRad);
        newDirection.Y = MathF.Sin(pitchRad);
        newDirection.Z = MathF.Cos(pitchRad) * MathF.Sin(yawRad);

        Camera.Direction = Vector3.Normalize(newDirection);
    }

    private float Clamp(float a, float min, float max)
    {
        if (a < min) a = min;
        if (a > max) a = max;
        return a;
    }


    private static bool isLocked;
    public static void Lock()
    {
        isLocked = true;
    }

    public static void Unlock()
    {
        isLocked = false;
    }
}
