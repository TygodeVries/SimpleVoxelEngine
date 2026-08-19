using OpenTK.Mathematics;

namespace Client.Rendering;

public abstract class Renderer
{
    /// <summary>
    /// Runs when the object should be rendered.
    /// </summary>
    /// <param name="isShadowPass"></param>
    public abstract void Render(bool isShadowPass);
    public bool enableDepth = true;
    public int sort = 0;

    /// <summary>
    /// The shader to use when the object is rendered.
    /// </summary>
    /// <returns></returns>
    public abstract ShaderProgram? GetShader();

    /// <summary>
    /// The Transform (Position, Scale, Rotation) of the object.
    /// </summary>
    /// <returns></returns>
    public abstract Matrix4 GetModelMatrix();
}
