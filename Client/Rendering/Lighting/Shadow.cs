using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Client.Rendering;

public class Shadow
{
    private const int shadowWidth = 2000;
    private const int shadowHeight = 2000;
    public static bool ShadowsEnabled { get; private set; } = false;

    public static int depthGl;
    private static int depthMapFBO;

    public static void Enable()
    {
        ShadowsEnabled = true;
        depthGl = GL.GenTexture();

        GL.BindTexture(TextureTarget.Texture2D, depthGl);
        GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.DepthComponent,
             shadowWidth, shadowHeight, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

        GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        depthMapFBO = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthGl, 0);

        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public static Matrix4 lightSpaceMatrix;

    public static void RenderShadows()
    {
        if (!ShadowsEnabled)
            return;

        Vector3 sunDirection = new Vector3(-0.4f, -1, -0.4f).Normalized();

        Vector3 sunPosition = Camera.Position + (-sunDirection * 100.0f);

        float orthoSize = 30.0f;
        Matrix4 lightProjection = Matrix4.CreateOrthographicOffCenter(-orthoSize, orthoSize, -orthoSize, orthoSize, 0.1f, 200.0f);

        Vector3 lightRight = Vector3.Cross(sunDirection, Vector3.UnitY).Normalized();

        if (lightRight.LengthSquared < 0.001f)
        {
            lightRight = Vector3.UnitX;
        }

        Vector3 lightUp = Vector3.Cross(lightRight, sunDirection).Normalized();

        Matrix4 lightView = Matrix4.LookAt(sunPosition, Camera.Position, lightUp);

        lightSpaceMatrix = lightView * lightProjection;

        GL.Viewport(0, 0, shadowWidth, shadowHeight);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
        GL.Clear(ClearBufferMask.DepthBufferBit);

        ShaderProgram depthShader = RenderData.DepthShader;
        depthShader.Use();

        depthShader.SetMatrix4("u_LightSpaceMatrix", lightSpaceMatrix);

        foreach (Renderer renderer in GameCanvas.GetRenderers())
        {
            depthShader.SetMatrix4("u_Model", renderer.GetModelMatrix());
            renderer.Render(true);
        }
        GL.UseProgram(0);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
}
