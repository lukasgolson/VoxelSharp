using ImGuiNET;
using VoxelSharp.Abstractions.Renderer;

namespace ImGUIMod;

public class DebugWindow : IRenderer
{
    public void InitializeShaders()
    {
        
    }

    public void Render(double interpolationFactor)
    {
        ImGui.ShowDemoWindow(); // Show the demo window

        ImGui.Begin("My ExampleMod Window");
        ImGui.Text("Hello from ExampleMod!");
        if (ImGui.Button("Click Me"))
        {
            // ...
        }
        ImGui.End();    }
}