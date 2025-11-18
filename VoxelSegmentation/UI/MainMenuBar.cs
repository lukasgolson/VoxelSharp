using ImGuiNET;
using System.Numerics;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;

namespace VoxelSegmentation.UI;

public class MainMenuBar : IRenderer
{
    private readonly PointcloudImporter _importer;
    private readonly IGameLoop _gameLoop;
    
    // State for the file dialog
    private bool _isDialogOpen = false;
    private string _filePathInput = "test.txt"; 
    
    // Import Settings
    private Vector3 _importRotation = Vector3.Zero;
    private float _importVoxelSize = 1.0f;
    
    // Up Axis Selection
    // Default to 1 (+Z) as requested
    private int _selectedUpAxis = 1;
    private readonly string[] _upAxisOptions = { "+Y", "+Z (Default)", "-Z", "+X", "-X", "-Y" };

    public MainMenuBar(PointcloudImporter importer, IGameLoop gameLoop)
    {
        _importer = importer;
        _gameLoop = gameLoop;
    }

    public void InitializeShaders() { }

    public void Render(double interpolationFactor)
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
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
            ImGui.EndMainMenuBar();
        }
        
        // Render Import Progress if active
        if (_importer.IsProcessing)
        {
            DrawProgressWindow();
        }

        // Render the modal if open
        DrawOpenFileDialog();
    }

    private void DrawProgressWindow()
    {
        // Center the progress window
        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(250, 80));
        
        // Added NoMove to ensure it stays centered
        if (ImGui.Begin("Importing...", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove))
        {
            ImGui.Text(_importer.StatusMessage);
            ImGui.ProgressBar(_importer.ImportProgress, new Vector2(-1, 0), $"{_importer.ImportProgress:P0}");
            ImGui.End();
        }
    }

    private void DrawOpenFileDialog()
    {
        if (_isDialogOpen)
        {
            ImGui.OpenPopup("Load Point Cloud");
        }

        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        // Added NoMove here as well
        if (ImGui.BeginPopupModal("Load Point Cloud", ref _isDialogOpen, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            ImGui.Text("Import Settings");
            ImGui.Separator();
            
            ImGui.InputText("Path", ref _filePathInput, 256);

            ImGui.Dummy(new Vector2(0, 10)); 
            
            // --- Up Axis Control ---
            ImGui.Text("Source Orientation");
            ImGui.Combo("Up Axis", ref _selectedUpAxis, _upAxisOptions, _upAxisOptions.Length);
            
            // Rotation Controls
            ImGui.Text("Fine Tuning (Degrees)");
            ImGui.DragFloat("X", ref _importRotation.X, 1.0f, -360f, 360f);
            ImGui.DragFloat("Y", ref _importRotation.Y, 1.0f, -360f, 360f);
            ImGui.DragFloat("Z", ref _importRotation.Z, 1.0f, -360f, 360f);

            // Voxel Size Control
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Text("Voxelization");
            ImGui.InputFloat("Voxel Size", ref _importVoxelSize, 0.1f, 1.0f, "%.2f");
            if (_importVoxelSize < 0.01f) _importVoxelSize = 0.01f; 

            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 5));

            if (ImGui.Button("Load", new Vector2(120, 0)))
            {
                Vector3 axisOffset = GetRotationForAxis(_selectedUpAxis);
                _importer.QueueImport(_filePathInput, _importRotation + axisOffset, _importVoxelSize);
                
                ImGui.CloseCurrentPopup();
                _isDialogOpen = false; 
            }
            
            ImGui.SameLine();
            
            if (ImGui.Button("Cancel", new Vector2(120, 0)))
            {
                ImGui.CloseCurrentPopup();
                _isDialogOpen = false; 
            }

            ImGui.EndPopup();
        }
    }

    private Vector3 GetRotationForAxis(int axisIndex)
    {
        return axisIndex switch
        {
            0 => Vector3.Zero,           // +Y
            1 => new Vector3(-90, 0, 0), // +Z -> +Y (Rotate -90 X)
            2 => new Vector3(90, 0, 0),  // -Z -> +Y (Rotate +90 X)
            3 => new Vector3(0, 0, 90),  // +X -> +Y (Rotate +90 Z)
            4 => new Vector3(0, 0, -90), // -X -> +Y (Rotate -90 Z)
            5 => new Vector3(180, 0, 0), // -Y -> +Y (Flip upside down)
            _ => Vector3.Zero
        };
    }
}