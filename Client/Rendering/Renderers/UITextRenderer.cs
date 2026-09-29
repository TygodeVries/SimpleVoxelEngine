using Client.Rendering;
using Client.Rendering.UI;
using OpenTK.Graphics.OpenGL;
using Shared.Mathf;
using System.Globalization;

public class UITextRenderer : MeshRenderer
{
    private ImageTexture texture;
    private Font font;

    private string text = "";

    private struct TextCharacter
    {
        public char Character;
        public Vector3 Color;

        public TextCharacter(char character, Vector3 color)
        {
            Character = character;
            Color = color;
        }
    }

    public UITextRenderer(Font font) : base(RenderData.UITextShader)
    {
        texture = font.GetTexture();
        this.font = font;

        sort = 5;
        shader.useOrthoProjection = true;

        Mesh mesh = new Mesh(
            new Vector3[0],
            new uint[0],
            new Vector2[0],
            new Vector3[0]
        );

        SetMesh(mesh);
    }

    private Vector3 ParseColor(string name)
    {
        switch (name.ToLowerInvariant())
        {
            case "red":
                return new Vector3(1, 0, 0);

            case "green":
                return new Vector3(0, 1, 0);

            case "blue":
                return new Vector3(0, 0, 1);

            case "white":
                return new Vector3(1, 1, 1);

            case "black":
                return new Vector3(0, 0, 0);

            case "yellow":
                return new Vector3(1, 1, 0);

            case "cyan":
                return new Vector3(0, 1, 1);

            case "magenta":
                return new Vector3(1, 0, 1);

            case "gray":
            case "grey":
                return new Vector3(0.5f, 0.5f, 0.5f);

            // Add RRGGBB support
            default:
                if (name.StartsWith("#") && name.Length == 7)
                {
                    try
                    {
                        float r = int.Parse(name.Substring(1, 2), NumberStyles.HexNumber) / 255f;
                        float g = int.Parse(name.Substring(3, 2), NumberStyles.HexNumber) / 255f;
                        float b = int.Parse(name.Substring(5, 2), NumberStyles.HexNumber) / 255f;

                        return new Vector3(r, g, b);
                    }
                    catch
                    {
                        // No idea
                    }
                }

                return new Vector3(1, 1, 1);
        }
    }

    private List<TextCharacter> ParseText(string input)
    {
        List<TextCharacter> result = new();

        Vector3 currentColor = new Vector3(1, 1, 1);

        for (int i = 0; i < input.Length;)
        {
            if (input[i] == '<')
            {
                int end = input.IndexOf('>', i + 1);

                if (end != -1)
                {
                    string tag = input.Substring(
                        i + 1,
                        end - i - 1
                    );
                    currentColor = ParseColor(tag);

                    i = end + 1;
                    continue;
                }
            }

            result.Add(new TextCharacter(
                input[i],
                currentColor
            ));

            i++;
        }

        return result;
    }

    public float TextWidth = 10;

    public void SetText(string text)
    {
        this.text = text ?? "";

        if (this.text.Length == 0)
        {
            SetMesh(new Mesh(
                new Vector3[0],
                new uint[0],
                new Vector2[0],
                new Vector3[0]
            ));

            return;
        }

        List<TextCharacter> characters = ParseText(this.text);

        List<Vector3> vertices = new();
        List<Vector2> uvs = new();
        List<Vector3> colors = new();
        List<uint> indices = new();

        float characterWidth = font.GetCharacterWidth() / TextWidth;
        float characterHeight = font.GetCharacterHeight();

        int visibleCharacterIndex = 0;

        foreach (TextCharacter characterData in characters)
        {
            char character = characterData.Character;

            Vector2[] characterUv = font.GetCharacterUv(character);

            float x = visibleCharacterIndex * characterWidth / 1.3f;

            uint vertexStart = (uint)vertices.Count;

            // Top-left
            vertices.Add(new Vector3(
                x,
                0,
                0
            ));

            // Top-right
            vertices.Add(new Vector3(
                x + characterWidth,
                0,
                0
            ));

            // Bottom-right
            vertices.Add(new Vector3(
                x + characterWidth,
                characterHeight,
                0
            ));

            // Bottom-left
            vertices.Add(new Vector3(
                x,
                characterHeight,
                0
            ));

            uvs.Add(characterUv[0]);
            uvs.Add(characterUv[1]);
            uvs.Add(characterUv[2]);
            uvs.Add(characterUv[3]);

            // Add color as normals data
            colors.Add(characterData.Color);
            colors.Add(characterData.Color);
            colors.Add(characterData.Color);
            colors.Add(characterData.Color);

            // First triangle
            indices.Add(vertexStart + 0);
            indices.Add(vertexStart + 1);
            indices.Add(vertexStart + 2);

            // Second triangle
            indices.Add(vertexStart + 2);
            indices.Add(vertexStart + 3);
            indices.Add(vertexStart + 0);

            visibleCharacterIndex++;
        }

        Mesh mesh = new Mesh(
            vertices.ToArray(),
            indices.ToArray(),
            uvs.ToArray(),
            colors.ToArray()
        );

        SetMesh(mesh);
    }

    public override void Render(bool isShadowPass)
    {
        if (isShadowPass)
            return;

        if (texture == null)
            return;

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(
            BlendingFactor.SrcAlpha,
            BlendingFactor.OneMinusSrcAlpha
        );
        texture.Use(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
        GL.Disable(EnableCap.Blend);
        base.Render(isShadowPass);
    }

    public override ShaderProgram? GetShader()
    {
        return base.GetShader();
    }

    public Vector2 position = new Vector2(0.5f, 0.5f);
    public float scale = 0.0007f;

    public UIImageRenderer? Parent;

    public Vector2 GetSize()
    {
        if (Parent == null)
        {
            float size = GameCanvas.Width * scale;

            return new Vector2(
                size * Math.Max(text.Length, 1),
                size
            );
        }

        Vector2 parentSize = Parent.GetSize();

        return new Vector2(
            parentSize.X * scale * Math.Max(text.Length, 1),
            parentSize.Y * scale
        );
    }

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

    public Vector2 GetCenter()
    {
        Vector2 topLeft = GetTopLeft();
        Vector2 size = GetSize();

        return topLeft + (size / 2.0f);
    }

    public override OpenTK.Mathematics.Matrix4 GetModelMatrix()
    {
        Vector2 topLeft = GetTopLeft();
        Vector2 size = GetSize();

        return
            OpenTK.Mathematics.Matrix4.CreateScale(
                size.X,
                size.Y,
                1.0f
            )
            *
            OpenTK.Mathematics.Matrix4.CreateTranslation(
                topLeft.X,
                topLeft.Y,
                0.0f
            );
    }
}
