using OpenTK.Mathematics;

namespace Client.Rendering;

public class UIRenderer : MeshRenderer
{
    private ImageTexture texture;
    public UIRenderer(ShaderProgram shader) : base(shader)
    {
        sort = 5;
        texture = ImageTexture.LoadFromPng("textures/ui.png");
        shader.SetTextureId("u_Color", 0);
        shader.useOrthoProjection = true;
        Mesh mesh = new Mesh(
    new Vector3[]
    {
        new Vector3(-0.5f, -0.5f, 0),
        new Vector3( 0.5f, -0.5f, 0),
        new Vector3(-0.5f,  0.5f, 0),
        new Vector3( 0.5f,  0.5f, 0)
    },
    new uint[]
    {
        0, 1, 2,
        1, 2, 3
    },
    new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1)
    }
);

        SetMesh(mesh);
    }

    public override void Render(bool isShadowPass)
    {
        if (isShadowPass)
            return;

        texture.Use(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);

        position = new Vector2(GameCanvas.Width / 2, GameCanvas.Height / 2);

        base.Render(isShadowPass);
    }

    public override ShaderProgram? GetShader()
    {
        return base.GetShader();
    }

    public Vector2 position;
    public float scale = 50;

    public override Matrix4 GetModelMatrix()
    {
        Matrix4 scale = Matrix4.CreateScale(this.scale);

        Matrix4 translation = Matrix4.CreateTranslation(
            GameCanvas.Width / 2.0f,
            GameCanvas.Height / 2.0f,
            0.0f
        );

        return scale * translation;
    }
}
