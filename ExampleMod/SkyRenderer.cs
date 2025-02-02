using OpenTK.Graphics.OpenGL4;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Core.Helpers;
using VoxelSharp.Renderer;
using VoxelSharp.Resources;
using VoxelSharp.Resources.Resources;

namespace ExampleMod;

public class SkyRenderer : IRenderer, IUpdatable
{
    private Shader _shader;

    private readonly ICameraMatrices _cameraMatrices;
    private readonly ResourceDictionary _resourceDictionary;


    private float[] _skyboxVertices =
    [
        -1.0f, -1.0f, 1.0f,
        1.0f, -1.0f, 1.0f,
        1.0f, 1.0f, 1.0f,
        -1.0f, 1.0f, 1.0f,

        -1.0f, -1.0f, -1.0f,
        -1.0f, 1.0f, -1.0f,
        1.0f, 1.0f, -1.0f,
        1.0f, -1.0f, -1.0f
    ];

    private uint[] _skyboxIndices =
    [
        0, 1, 2, 2, 3, 0, // Front
        4, 5, 6, 6, 7, 4, // Back
        4, 0, 3, 3, 5, 4, // Left
        1, 7, 6, 6, 2, 1, // Right
        3, 2, 6, 6, 5, 3, // Top
        4, 7, 1, 1, 0, 4 // Bottom
    ];


    private float _time;
    private int _vao;
    private int _vbo;
    private int _ebo;

    public SkyRenderer(ResourceDictionary resourceDictionary, ICameraMatrices cameraMatrices, IGameLoop gameLoop)
    {
        _cameraMatrices = cameraMatrices;
        _resourceDictionary = resourceDictionary;

        gameLoop.RegisterRenderAction(this, -10);
        gameLoop.RegisterUpdateAction(this);
    }


    public void InitializeShaders()
    {
        var skyVertResource = _resourceDictionary.GetResource<string>(new Address("ExampleMod:Shaders/Sky.vert"));
        var skyFragResource = _resourceDictionary.GetResource<string>(new Address("ExampleMod:Shaders/Sky.frag"));


        string skyVert = skyVertResource.Value;
        string skyFrag = skyFragResource.Value;


        _shader = new Shader(skyVert, skyFrag, true);

        SetupMesh();
    }

    private bool _initialized;

    public void Render(double interpolationFactor)
    {
        if (!_initialized)
        {
            InitializeShaders();
            _initialized = true;
        }

        GL.DepthMask(false); // Disable depth writing (but keep depth testing)
        GL.Disable(EnableCap.CullFace); // Optional: Prevents missing skybox faces

        GL.DepthFunc(DepthFunction.Lequal); // Ensures skybox doesn't clip the world


        _shader.Use();

        

        
        GL.BindVertexArray(_vao);

        // Set uniforms
        var view = _cameraMatrices.GetViewMatrix().ClearTranslation();
        var projection = _cameraMatrices.GetProjectionMatrix();

        _shader.SetUniform("view", view);
        _shader.SetUniform("projection", projection);
        _shader.SetUniform("time", _time % 60);

        // Draw skybox
        GL.DrawElements(PrimitiveType.Triangles, _skyboxIndices.Length, DrawElementsType.UnsignedInt, 0);
        
        GL.BindVertexArray(0);
        
        Shader.Unuse();

        GL.DepthMask(true); // Re-enable depth writing for other objects
        GL.Enable(EnableCap.CullFace); // Re-enable face culling if used
        GL.DepthFunc(DepthFunction.Less); // Reset depth function
        
        
    }

    public void SetupMesh()
    {
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        // Upload vertex data
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _skyboxVertices.Length * sizeof(float), _skyboxVertices,
            BufferUsageHint.StaticDraw);

        // Upload index data
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _skyboxIndices.Length * sizeof(uint), _skyboxIndices,
            BufferUsageHint.StaticDraw);

        // Enable vertex attribute
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    public void Update(double deltaTime)
    {
        _time += (float)deltaTime / 1000;
    }
    
    
}