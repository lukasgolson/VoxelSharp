using ImGuiNET;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;

namespace VoxelSegmentation.UI;

public class MainMenuBar : IRenderer
{
    private readonly PointcloudImporter _importer;
    
    // State for the file dialog
    private bool _isDialogOpen = false;
    private string _filePathInput = "test.txt"; // Default value

    private IGameLoop _gameLoop;
    
    public MainMenuBar(PointcloudImporter importer, IGameLoop gameLoop)
    {
        _importer = importer;
        _gameLoop = gameLoop;
    }

    public void InitializeShaders()
    {
    }

    public void Render(double interpolationFactor)
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New", "Ctrl+N")) { /* ... */ }
                
                // Trigger the popup
                if (ImGui.MenuItem("Open Point Cloud", "Ctrl+O"))
                {
                    _isDialogOpen = true; 
                }

                ImGui.Separator();
                if (ImGui.MenuItem("Exit"))
                {
                    _gameLoop.Stop();
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
        }
        
        // Render the modal if open
        DrawOpenFileDialog();
    }

    private void DrawOpenFileDialog()
    {
        if (_isDialogOpen)
        {
            ImGui.OpenPopup("Load Point Cloud");
            //_isDialogOpen = false; // Consume the flag so we don't re-open constantly
        }

        // Always center the modal
        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new System.Numerics.Vector2(0.5f, 0.5f));

        if (ImGui.BeginPopupModal("Load Point Cloud", ref _isDialogOpen, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("Enter the path to the .txt point cloud file:");
            ImGui.InputText("Path", ref _filePathInput, 256);

            ImGui.Separator();

            if (ImGui.Button("Load", new System.Numerics.Vector2(120, 0)))
            {
                _importer.QueueImport(_filePathInput);
                ImGui.CloseCurrentPopup();
                _isDialogOpen = false; 

            }
            
            ImGui.SameLine();
            
            if (ImGui.Button("Cancel", new System.Numerics.Vector2(120, 0)))
            {
                ImGui.CloseCurrentPopup();
                _isDialogOpen = false; 

            }

            ImGui.EndPopup();
        }
    }
}