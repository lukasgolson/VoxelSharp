using VoxelSharp.Modding.Structs;

namespace VoxelSharp.Modding.Interfaces;

public interface IMod
{
    ModInfo ModInfo { get; }

    bool PreInitialize();


    bool Initialize();


    bool Update(float deltaTime);
    bool Render();
}