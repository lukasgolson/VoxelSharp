using ImGuiNET;
using VoxelSharp.Abstractions.Renderer;

namespace VoxelSegmentation.UI;

public class MainMenuBar : IRenderer
{
    public void InitializeShaders()
    {
    }

    public void Render(double interpolationFactor)
    {
    
        if (ImGui.BeginMainMenuBar())
        {
            // 2. Create a "File" menu
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New", "Ctrl+N"))
                {
                }
                if (ImGui.MenuItem("Open", "Ctrl+O"))
                {
                  

                }
                ImGui.Separator();
                if (ImGui.MenuItem("Exit"))
                {
                }
            
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                if (ImGui.MenuItem("Undo", "Ctrl+Z")) { /* ... */ }
                if (ImGui.MenuItem("Redo", "Ctrl+Y")) { /* ... */ }
            
                ImGui.EndMenu();
            }
        
            ImGui.EndMainMenuBar();
        }    }
}