using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Client.Rendering;

/// <summary>
/// Shaderprogram taken from Dart game engine
/// </summary>
public class ShaderProgram
{
    public bool useOrthoProjection;
    private string vertexSource;
    private string fragmentSource;

    private bool isCompiled = false;

    private int shaderProgramId;
    public void Use()
    {
        if (!isCompiled)
            Compile();

        GL.UseProgram(shaderProgramId);
    }

    public ShaderProgram Copy()
    {
        return new ShaderProgram(vertexSource, fragmentSource);
    }
    public ShaderProgram(string vertexShader, string fragmentShader)
    {
        if (vertexShader.Length < 20)
        {
            Console.WriteLine($"VertexShader does not look like source code, please be aware. {vertexShader}");
        }

        if (fragmentShader.Length < 20)
        {
            Console.WriteLine($"FragmentShader does not look like source code, please be aware. {fragmentShader}");
        }


        this.vertexSource = vertexShader;
        this.fragmentSource = fragmentShader;
    }

    public void Compile()
    {
        int vertex = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertex, vertexSource);
        GL.CompileShader(vertex);

        GL.GetShaderi(vertex, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus == 0)
        {
            GL.GetShaderInfoLog(vertex, out string vLog);
            Console.WriteLine($"Vertex shader compile error:\n{vLog}");
            return;
        }

        int fragment = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragment, fragmentSource);
        GL.CompileShader(fragment);

        GL.GetShaderi(fragment, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus == 0)
        {
            GL.GetShaderInfoLog(fragment, out string fLog);
            Console.WriteLine($"Fragment shader compile error:\n{fLog}");
            return;
        }

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);
        GL.LinkProgram(program);

        GL.GetProgrami(program, ProgramProperty.LinkStatus, out int linkStatus);
        if (linkStatus == 0)
        {
            GL.GetProgramInfoLog(program, out string pLog);
            Console.WriteLine($"Program link error:\n{pLog}");
            return;
        }

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        shaderProgramId = program;
        isCompiled = true;
        uniformLocations.Clear();
    }


    public void SetFloat(string field, float f)
    {
        int mvpLocation = GetUniformLocation(field);
        if (mvpLocation == -1)
            return;
        GL.Uniform1f(mvpLocation, f);
    }

    public void SetVector3(string field, Vector3 vector3)
    {
        int mvpLocation = GetUniformLocation(field);
        if (mvpLocation == -1)
            return;
        GL.Uniform3f(mvpLocation, vector3.X, vector3.Y, vector3.Z);
    }

    public void SetVector4(string field, Vector4 vector)
    {
        int mvpLocation = GetUniformLocation(field);
        if (mvpLocation == -1)
            return;
        GL.Uniform4f(mvpLocation, vector.X, vector.Y, vector.Z, vector.W);
    }


    public void SetMatrix4(string field, Matrix4 matrix4)
    {
        int mvpLocation = GetUniformLocation(field);
        if (mvpLocation == -1)
            return;
        GL.UniformMatrix4f(mvpLocation, 1, false, ref matrix4);
    }

    public void SetInt(string field, int i)
    {
        int mvpLocation = GetUniformLocation(field);
        if (mvpLocation == -1)
            return;
        GL.Uniform1i(mvpLocation, i);
    }

    public void SetTextureId(string field, int id)
    {
        int mvpLocation = GetUniformLocation(field);
        if (mvpLocation == -1)
            return;
        GL.Uniform1i(mvpLocation, id);
    }

    private Dictionary<string, int> uniformLocations = new Dictionary<string, int>();

    private int GetUniformLocation(string name)
    {
        Use();
        if (uniformLocations.TryGetValue(name, out int location))
            return location;

        location = GL.GetUniformLocation(shaderProgramId, name);
        if (location == -1)
        {
            //  Console.WriteLine($"Value '{name}' not found in shader, but you are trying to access it anyways!");
        }

        uniformLocations[name] = location;
        return location;
    }

    public void SetVector3Array(string uniformName, Vector3[] vectors)
    {
        for (int i = 0; i < vectors.Length; i++)
        {
            string elementName = $"{uniformName}[{i}]";
            int location = GL.GetUniformLocation(shaderProgramId, elementName);
            if (location != -1)
            {
                GL.Uniform3f(location, vectors[i].X, vectors[i].Y, vectors[i].Z);
            }
        }
    }

    public void SetIntArray(string uniformName, int[] values)
    {
        int location = GL.GetUniformLocation(shaderProgramId, uniformName);
        if (location == -1) return;
        GL.Uniform1i(location, values.Length, values);
    }

    public void SetMatrix4Array(string uniformName, Matrix4[] matrices)
    {
        for (int i = 0; i < matrices.Length; i++)
        {
            string elementName = $"{uniformName}[{i}]";
            int location = GL.GetUniformLocation(shaderProgramId, elementName);
            if (location != -1)
            {
                OpenTK.Mathematics.Matrix4 matrix = matrices[i];
                GL.UniformMatrix4f(location, 1, false, ref matrix);
            }
        }
    }
}
