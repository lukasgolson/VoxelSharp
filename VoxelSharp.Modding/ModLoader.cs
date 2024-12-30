using System.Reflection;
using HarmonyLib;
using VoxelSharp.Modding.Interfaces;
using VoxelSharp.Modding.Structs;

namespace VoxelSharp.Modding;

public class ModLoader
{
    private readonly DependencyResolver _resolver = new();

    private bool _initialized;
    private List<IMod> _mods = [];
    private readonly string _modsPath;

    public ModLoader(string modsPath)
    {
        // resolve the mods path to the absolute path
        _modsPath = Path.GetFullPath(modsPath);
        Console.WriteLine($"Resolved mods path to {_modsPath}");
        
    }


    public void LoadMods()
    {
        if (_initialized) throw new InvalidOperationException("ModLoader has already been initialized.");
        

        var assemblyPaths = DiscoverAssemblies();

        Console.WriteLine($"Found {assemblyPaths.Count} assemblies in {_modsPath}");

        var modTypes = LoadAssemblies(assemblyPaths);

        Console.WriteLine($"Found {modTypes.Count} mod interfaces in {assemblyPaths.Count} assemblies");

        var mods = CreateModInstances(modTypes);

        Console.WriteLine($"Created {mods.Count} mod instances");


        // create a dictionary of mod info to Mods
        var orderedMods = ResolveLoadingOrder(mods);

        Console.WriteLine($"Resolved loading order for {orderedMods.Count} mods");

        foreach (var mod in orderedMods)
        {
            mod.PreInitialize();
            Console.WriteLine($"Pre-initialized mod {mod.ModInfo.Name} v{mod.ModInfo.Version}");
        }

        foreach (var mod in orderedMods)
        {
            var harmony = new Harmony(mod.ModInfo.Id);
            mod.Initialize(harmony);
            Console.WriteLine($"Initialized mod {mod.ModInfo.Name} v{mod.ModInfo.Version}");
        }

        _mods = orderedMods;
        _initialized = true;
    }

    public void Update(double deltaTime)
    {
        foreach (var mod in _mods) mod.Update(deltaTime);
    }

    public void Render()
    {
        foreach (var mod in _mods) mod.Render();
    }

    private List<IMod> ResolveLoadingOrder(List<IMod> mods)
    {
        Dictionary<ModInfo, IMod> modDict = new();

        foreach (var mod in mods)
        {
            modDict.Add(mod.ModInfo, mod);
            _resolver.AddMod(mod.ModInfo);
        }

        // Resolve dependencies
        var loadOrder = _resolver.ResolveLoadOrder();
        var orderedMods = loadOrder.Select(mod => modDict[mod]).ToList();
        return orderedMods;
    }


    private List<string> DiscoverAssemblies()
    {
        if (!Directory.Exists(_modsPath)) throw new DirectoryNotFoundException($"Directory not found: {_modsPath}");

        return Directory.GetFiles(_modsPath, "*.dll").ToList();
    }

    private static List<Type> LoadAssemblies(IEnumerable<string> assemblyPaths)
    {
        var modTypes = new List<Type>();

        foreach (var path in assemblyPaths)
            try
            {
                var assembly = Assembly.LoadFrom(path);
                var typesImplementingIMod = assembly.GetTypes()
                    .Where(type =>
                        typeof(IMod).IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false })
                    .ToList();


                modTypes.AddRange(typesImplementingIMod);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load assembly {path}: {ex.Message}");
            }

        return modTypes;
    }

    private static List<IMod> CreateModInstances(IEnumerable<Type> modTypes)
    {
        var mods = new List<IMod>();

        foreach (var type in modTypes)
        {
            Console.WriteLine($"Creating mod instance {type.Name}");

            try
            {
                if (Activator.CreateInstance(type) is not IMod mod)
                {
                    continue;
                }
                
                mods.Add(mod);
                
                Console.WriteLine($"Created mod instance {mod.ModInfo.Name} v{mod.ModInfo.Version}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create mod instance {type.Name}: {ex.Message}");
            }
        }

        return mods;
    }

    public void InitializeShaders()
    {
        foreach (var mod in _mods) mod.InitializeShaders();
    }
}