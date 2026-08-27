namespace Client.Rendering;

using Client.Input;
using Client.Networking;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Shared.Mathf;
using Shared.Worlds;
using System.Diagnostics;
using Matrix4 = OpenTK.Mathematics.Matrix4;

public class GameCanvas : GameWindow
{
    public static bool DebugEnabled = false;
    public GameCanvas(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        canvas = this;
    }
    private static GameCanvas canvas;
    public static void ForceClose()
    {
        canvas.Close();
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
        GL.Enable(EnableCap.CullFace);

        GL.Enable(EnableCap.LineSmooth);
        GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);


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
        LocalWorld.World.OnRemoveChunk += World_OnRemoveChunk;
        GameCanvas.Width = width;
        GameCanvas.Height = height;

        SwitchDedicated();

        InventoryUI.CreateUI();

        ImageTexture crossAir = ImageTexture.LoadFromPng("Textures/Crossair.png");

        UIRenderer uiCrossAir = new UIRenderer();
        uiCrossAir.SetTexture(crossAir);
        AddRenderer(uiCrossAir);

        // Skybox

        MeshRenderer skyboxRenderer = new MeshRenderer(RenderData.SkyboxShader);
        Mesh? mesh = Mesh.FromFileObj("Models/Skybox.obj");
        if (mesh == null)
        {
            Console.WriteLine("Could not load skybox model.");
        }

        skyboxRenderer.SetMesh(mesh!);
        skyboxRenderer.enableDepth = false;
        skyboxRenderer.sort = -10000;
        skyboxRenderer.Texture = ImageTexture.LoadFromPng("Textures/Skybox.png", flip: true);
        OnUpdate += () =>
        {
            skyboxRenderer.SetModelMatrix(Matrix4.CreateTranslation(Camera.Position.ToOpenTK()) * Matrix4.CreateScale(1));
        };

        AddRenderer(skyboxRenderer);

        LineRenderer lineRenderer = new LineRenderer(RenderData.SelectionShader);
        lineRenderer.LoadCubeWireframe();

        OnUpdate += () =>
        {
            RaycastHit? hit = LocalWorld.World.Raycast(Camera.Position, Camera.Direction, 5);
            if (hit != null)
            {
                lineRenderer.position = hit.WorldBlockPos;
            }
            else
            {
                lineRenderer.position = Camera.Position - new Vector3(0, 10000, 0);
            }
        };
        AddRenderer(lineRenderer);
    }
    private void World_OnRemoveChunk(Shared.Worlds.Chunk obj)
    {
        for (int i = 0; i < chunkRenderers.Count; i++)
        {

            if (chunkRenderers[i].Chunk == obj)
            {
                RemoveRenderer(chunkRenderers[i]);
                i--;
                break;
            }
        }
    }

    private static void SwitchDedicated()
    {
        if (OperatingSystem.IsLinux())
        {
            Environment.SetEnvironmentVariable("DRI_PRIME", "1");
        }
    }
    private void World_OnAddChunk(Shared.Worlds.Chunk obj)
    {
        ChunkRenderer chunkRenderer = new ChunkRenderer(obj);
        GameCanvas.AddRenderer(chunkRenderer);
    }

    private Stopwatch fpsCounterStopwatch = new Stopwatch();
    private int frameCount = 0;
    public static event Action? OnUpdate;
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

        Time.Frame++;

        // Clear the background
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Render at the correct resolution, gets reset if shadows are rendered.
        GL.Viewport(0, 0, width, height);

        Matrix4 viewMatrix = Camera.GetViewMatrix();
        Matrix4 projectionMatrix = Camera.GetProjectionMatrix();

        Matrix4 orthoProjection = Matrix4.CreateOrthographicOffCenter(
            0,
            GameCanvas.Width,
            GameCanvas.Height,
            0,
            -1,
            1
        );

        // Render background elements
        int leftOff = RenderRenderers(0, 0, viewMatrix, orthoProjection, projectionMatrix);

        ChunkRenderType currentChunkRenderType = ChunkRenderType.Empty;

        // Set the data once.
        RenderData.SingleChunkShader!.SetMatrix4("u_View", viewMatrix);
        RenderData.DefaultChunkShader!.SetMatrix4("u_View", viewMatrix);

        RenderData.SingleChunkShader!.SetMatrix4("u_Projection", projectionMatrix);
        RenderData.DefaultChunkShader!.SetMatrix4("u_Projection", projectionMatrix);


        if (RenderData.BlockTexture != null)
        {
            RenderData.BlockTexture!.Use(TextureUnit.Texture0);
        }
        else
        {
            // Panic?
        }

        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);

        // Render chunks
        foreach (ChunkRenderer chunkRenderer in chunkRenderers)
        {
            if (chunkRenderer.renderType == ChunkRenderType.Empty)
            {
                currentChunkRenderType = ChunkRenderType.Empty;
                continue;
            }

            // Use the correct shader
            if (chunkRenderer.renderType == ChunkRenderType.Solid && currentChunkRenderType != ChunkRenderType.Solid)
            {
                currentChunkRenderType = ChunkRenderType.Solid;
                RenderData.SingleChunkShader!.Use();
            }
            else if (chunkRenderer.renderType == ChunkRenderType.Normal && currentChunkRenderType != ChunkRenderType.Normal)
            {
                currentChunkRenderType = ChunkRenderType.Normal;
                RenderData.DefaultChunkShader!.Use();
            }

            // Set the model matrix 
            ShaderProgram.GlobalSetMatrix4("u_Model", chunkRenderer.GetModelMatrix());

            // Render the chunk

            chunkRenderer.Render(false);
        }

        // Render foreground elements
        RenderRenderers(leftOff, int.MaxValue, viewMatrix, orthoProjection, projectionMatrix);

        // Say we are ready to show the frame!
        SwapBuffers();
    }

    public int RenderRenderers(int startIndex, int untilExcludeDepth, Matrix4 viewMatrix, Matrix4 orthoProjection, Matrix4 projectionMatrix)
    {
        // Loop over every thing we are rendering
        for (int i = startIndex; i < renderers.Count; i++)
        {

            Renderer renderer = renderers[i];
            if (!renderer.visible)
                continue;


            // Render that thing.

            if (renderer.sort >= untilExcludeDepth)
            {
                return i;
            }

            ShaderProgram? shader = renderer.GetShader();

            if (shader == null)
                continue;

            bool isUI = shader.useOrthoProjection;

            if (isUI || !renderer.enableDepth)
            {
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);
            }
            else
            {
                GL.Enable(EnableCap.CullFace);
                GL.Enable(EnableCap.DepthTest);
            }

            shader.SetMatrix4("u_Model", renderer.GetModelMatrix());
            shader.SetMatrix4("u_View", viewMatrix);

            if (shader.useOrthoProjection)
            {
                shader.SetMatrix4("u_Projection", orthoProjection);
            }
            else
            {
                shader.SetMatrix4("u_Projection", projectionMatrix);
            }

            renderer.Render(false);
        }

        // We are done
        return renderers.Count + 1;
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
    private static List<ChunkRenderer> chunkRenderers = new List<ChunkRenderer>();

    public static List<Renderer> GetRenderers()
    {
        return renderers;
    }

    /// <summary>
    /// Add a thing to renderer
    /// </summary>
    /// <param Name="renderer"></param>
    public static void AddRenderer(Renderer renderer)
    {
        if (renderer is ChunkRenderer chunk)
        {
            chunkRenderers.Add(chunk);
        }
        else
        {
            renderers.Add(renderer);

            renderers.Sort((a, b) =>
            {
                return a.sort - b.sort;
            });
        }
    }

    /// <summary>
    /// Remove a thing to render
    /// </summary>
    /// <param Name="renderer"></param>
    public static void RemoveRenderer(Renderer renderer)
    {
        renderers.Remove(renderer);

        if (renderer is ChunkRenderer chunkRenderer)
            chunkRenderers.Remove(chunkRenderer);
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

        foreach (ChunkRenderer chunkRenderer in chunkRenderers)
        {
            chunkRenderer.Update();
        }

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

        if (Keyboard.Current.IsPressedThisFrame(OpenTK.Windowing.GraphicsLibraryFramework.Keys.F2))
        {
            if (!Keyboard.Current.IsPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift))
            {
                File.WriteAllBytes("TextureDump.png", ((ImageTexture)RenderData.BlockTexture).GetPngBytes());
            }
            else
            {
                File.WriteAllBytes("TextureDump.png", ((ImageTexture)RenderData.ItemTexture).GetPngBytes());
            }
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.GetFullPath("TextureDump.png"),
                UseShellExecute = true
            });
        }

        LocalWorld.World.Tick();

        OnUpdate?.Invoke();

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

        Vector3 newDirection = new Vector3();
        newDirection.X = MathF.Cos(pitchRad) * MathF.Cos(yawRad);
        newDirection.Y = MathF.Sin(pitchRad);
        newDirection.Z = MathF.Cos(pitchRad) * MathF.Sin(yawRad);

        Camera.Direction = newDirection.Normalized;
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        Mouse.Current.scroll = new Vector2(e.Offset);
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
