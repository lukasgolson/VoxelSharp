using VoxelSharp.Modding.Structs;

namespace VoxelSharp.Modding;

public class DependencyResolver
{
    private readonly Dictionary<string, ModInfo> _mods = new();

    public void AddMod(ModInfo mod)
    {
        if (!_mods.TryAdd(mod.Id, mod))
            throw new Exception($"Duplicate mod ID detected: {mod.Id}");
    }

    public List<ModInfo> ResolveLoadOrder()
    {
        var resolved = new List<ModInfo>();
        var seen = new HashSet<string>();

        foreach (var mod in _mods.Values) Resolve(mod, resolved, seen, []);

        return resolved;
    }

    private void Resolve(ModInfo mod, List<ModInfo> resolved, HashSet<string> seen, HashSet<string> visiting)
    {
        if (resolved.Any(m => m.Id == mod.Id))
            return; // Already resolved

        if (!visiting.Add(mod.Id))
            throw new Exception($"Circular dependency detected for mod: {mod.Id}-{mod.Name}");


        foreach (var dependency in mod.Dependencies)
        {
            if (!_mods.TryGetValue(dependency.Id, out var dependencyMod))
                throw new Exception($"Missing dependency: {dependency.Id} >= {dependency.Version} for mod: {mod.Name}");

            // Compare versions for compatibility
            var requiredVersion = dependency.Version;
            var availableVersion = dependencyMod.Version;

            if (availableVersion < requiredVersion)
                throw new Exception(
                    $"Dependency {dependency.Id} requires version {requiredVersion} or higher, but found {availableVersion}.");

            Resolve(dependencyMod, resolved, seen, visiting);
        }


        visiting.Remove(mod.Id);
        seen.Add(mod.Id);
        resolved.Add(mod);
    }
}