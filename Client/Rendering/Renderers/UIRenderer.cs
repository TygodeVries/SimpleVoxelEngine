using Shared.Mathf;
using Matrix4 = OpenTK.Mathematics.Matrix4;

namespace Client.Rendering;

public class UIRenderer : MeshRenderer
{
    public void SetTexture(ImageTexture texture)
    {
        this.texture = texture;
    }
    private ImageTexture texture;
    public UIRenderer() : base(RenderData.UIShader)
    {
        sort = 5;
        shader.SetTextureId("u_Color", 0);
        shader.useOrthoProjection = true;
        Mesh mesh = new Mesh(
    new Vector3[]
    {
        new Vector3(-0.5f, -0.5f, 0),
        new Vector3( 0.5f, -0.5f, 0),
        new Vector3( 0.5f,  0.5f, 0),
        new Vector3(-0.5f,  0.5f, 0)
    },
    new uint[]
    {
        0, 1, 3,
        1, 3, 2
    },
    new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(1, 1),
        new Vector2(0, 1)
    }
);

        SetMesh(mesh);
    }

    public void SetUvs(Vector2[] uvs)
    {
        if (uvs.Length != 4)
            throw new ArgumentException(
                "UIRenderer requires exactly 4 UV coordinates.",
                nameof(uvs)
            );

        float[] final = new float[uvs.Length * 2];

        for (int i = 0; i < uvs.Length; i++)
        {
            final[i * 2] = uvs[i].X;
            final[(i * 2) + 1] = uvs[i].Y;
        }

        mesh.uvs = final;
        SetMesh(mesh);
    }

    public override void Render(bool isShadowPass)
    {
        if (isShadowPass)
            return;

        if (texture == null)
            return;
        texture.Use(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);


        base.Render(isShadowPass);
    }

    public override ShaderProgram? GetShader()
    {
        return base.GetShader();
    }

    public Vector2 position = new Vector2(0.5f, 0.5f);
    public float scale = 0.02f;

    public UIRenderer? Parent;

    /// <summary>
    /// Gets the size of this UI element in pixels.
    /// </summary>
    public Vector2 GetSize()
    {
        if (Parent == null)
        {
            float size = GameCanvas.Width * scale;
            return new Vector2(size, size);
        }

        Vector2 parentSize = Parent.GetSize();

        float width = parentSize.X * scale;
        float height = parentSize.Y * scale;

        return new Vector2(width, height);
    }

    /// <summary>
    /// Gets the top-left Position of this UI element in screen pixels.
    /// </summary>
    public Vector2 GetTopLeft()
    {
        Vector2 size = GetSize();

        Vector2 center;

        if (Parent == null)
        {
            center = new Vector2(
                GameCanvas.Width * position.X,
                GameCanvas.Height * position.Y
            );
        }
        else
        {
            Vector2 parentTopLeft = Parent.GetTopLeft();
            Vector2 parentSize = Parent.GetSize();

            center = new Vector2(
                parentTopLeft.X + (parentSize.X * position.X),
                parentTopLeft.Y + (parentSize.Y * position.Y)
            );
        }

        return center - (size / 2.0f);
    }

    /// <summary>
    /// Gets the center Position of this UI element in screen pixels.
    /// </summary>
    public Vector2 GetCenter()
    {
        Vector2 topLeft = GetTopLeft();
        Vector2 size = GetSize();

        return topLeft + (size / 2.0f);
    }

    public override Matrix4 GetModelMatrix()
    {
        Vector2 center = GetCenter();
        Vector2 size = GetSize();

        Matrix4 scaleMatrix = Matrix4.CreateScale(
            size.X,
            size.Y,
            1.0f
        );

        Matrix4 translation = Matrix4.CreateTranslation(
            center.X,
            center.Y,
            0.0f
        );

        return scaleMatrix * translation;
    }
}
