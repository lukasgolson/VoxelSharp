using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Core.Helpers;
using VoxelSharp.Renderer;
using VoxelSharp.Resources;
using VoxelSharp.Resources.Resources;

namespace ExampleMod;

public class SkyRenderer : IRenderer, IUpdatable, ILightSource
{
    private Shader _shader;

    private readonly ICameraMatrices _cameraMatrices;
    private readonly ResourceDictionary _resourceDictionary;


    private const float DayLength = 120.0f;

    public float NormalizedTime { get; private set; }
    private Vector3 CurrentLightDirection { get; set; }

    private Vector3 CurrentLightColor { get; set; }

    public bool UpdateTime { get; set; } = true;


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


    private int _vao;
    private int _vbo;
    private int _ebo;

    public SkyRenderer(ResourceDictionary resourceDictionary, ICameraMatrices cameraMatrices)
    {
        _cameraMatrices = cameraMatrices;
        _resourceDictionary = resourceDictionary;
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

        GL.Disable(EnableCap.CullFace);
        // Keep the depth function change so the skybox draws "behind" everything
        GL.DepthFunc(DepthFunction.Lequal);

        _shader.Use();

        GL.BindVertexArray(_vao);

        // Set uniforms
        var view = _cameraMatrices.GetViewMatrix().ClearTranslation();
        var projection = _cameraMatrices.GetProjectionMatrix();

        _shader.SetUniform("view", view);
        _shader.SetUniform("projection", projection);
        _shader.SetUniform("time", NormalizedTime);

        // Draw skybox
        GL.DrawElements(PrimitiveType.Triangles, _skyboxIndices.Length, DrawElementsType.UnsignedInt, 0);

        GL.BindVertexArray(0);

        Shader.Unuse();

        GL.DepthFunc(DepthFunction.Less); // Reset depth function

        GL.Enable(EnableCap.CullFace);


        // IMGUI ui
        RenderImGui();
    }

    private float GetCurrentTimeForSlider()
    {
        return NormalizedTime * DayLength;
    }

    private static bool _isWindowOpen = true;

    private void RenderImGui()
    {
        float timeValue = GetCurrentTimeForSlider();


        if (ImGui.Begin("Time of Day Control", ref _isWindowOpen))
        {
            bool isFlowing = UpdateTime;
            if (ImGui.Checkbox("Time Flow", ref isFlowing))
            {
                UpdateTime = isFlowing;
            }

            ImGui.SameLine();
            ImGui.Text($"Current Cycle: {NormalizedTime:F3}");

            // --- Time Slider ---
            ImGui.Text("Time of Day (Manual)");
            const string label = "Time";

            // Format time value to display time (e.g., 0.5 = Noon)
            var displayedTime = timeValue / DayLength;
            var timeString = TimeToClock(displayedTime);

            ImGui.Text($"Current Time: {timeString}");


            if (ImGui.SliderFloat(label, ref timeValue, 0.0f, DayLength, ""))
            {
                // User interacted with the slider, so we stop the automatic flow
                UpdateTime = false;

                // Ensure time stays within bounds
                timeValue = Math.Clamp(timeValue, 0.0f, DayLength);

                // Update the static time field in SkyRenderer
                _time = timeValue;
            }


            ImGui.End();
        }
    }

    private string TimeToClock(float normalizedTime)
    {
        // Assume 0.0 is 00:00 (midnight) and 0.5 is 12:00 (noon).
        int totalMinutes = (int)(normalizedTime * 24.0f * 60.0f);
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        return $"{hours:D2}:{minutes:D2}";
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

    private float _time;

    public void Update(double deltaTime)
    {
        if (UpdateTime)
        {
            _time += (float)deltaTime * 1.0f;
        }

        NormalizedTime = _time % DayLength / DayLength;


        float rotationAngle = NormalizedTime * 2.0f * MathF.PI;
        float x = MathF.Cos(rotationAngle);
        float z = MathF.Sin(rotationAngle);
        float y = -0.5f;
        CurrentLightDirection = new Vector3(x, y, z).Normalize();


        Vector3 nightColor = new(0.05f, 0.05f, 0.15f);
        Vector3 dayColor = new(1.0f, 1.0f, 0.9f);
        Vector3 sunsetColor = new(1.0f, 0.5f, 0.1f);


        float dayFactorRaw = -MathF.Cos(rotationAngle) * 0.5f + 0.5f;

        // Use gamma correction to smooth the transition to darkness,
        // making the dark periods longer and the light change more gentle.
        float dayIntensity = MathF.Pow(dayFactorRaw, 1.5f);


        float distanceToNoon = MathF.Abs(dayFactorRaw - 0.5f);
        float warmFactor = (0.5f - distanceToNoon) * 2.0f; // Goes 0 (midnight) -> 1 (noon) -> 0 (midnight)

        float horizonFactor = MathF.Pow(1.0f - warmFactor, 8.0f) * 5.0f; // Power 8 creates sharp, strong peak


        Vector3 baseColor = Vector3.Lerp(nightColor, dayColor, dayIntensity);


        CurrentLightColor = Vector3.Lerp(baseColor, sunsetColor, MathF.Min(horizonFactor, 1.0f));

        if (dayIntensity < 0.2f)
        {
            CurrentLightColor *= dayIntensity / 0.2f * 0.8f + 0.2f;
        }
    }


    public Vector3 GetCurrentLightDirection()
    {
        return CurrentLightDirection;
    }

    public Vector3 GetCurrentLightColor()
    {
        return CurrentLightColor;
    }
}