using ImGuiNET;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;

namespace ImGUIMod;

public class DebugWindow : IRenderer
{
    private readonly IGameLoop _gameLoop;

    public DebugWindow(IGameLoop gameLoop)
    {
        _gameLoop = gameLoop;
    }
    public void InitializeShaders()
    {
        
    }

    public void Render(double interpolationFactor)
    {
        ImGui.Begin("Debug Info", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoTitleBar);
        
        ImGui.Text($"FPS: {_gameLoop.CurrentRenderFrequency:F1}");
        ImGui.Text($"TPS: {_gameLoop.CurrentUpdateFrequency:F1}");
        
        ImGui.End();    
    }
}