using System.Reflection;
using HarmonyLib;
using VoxelSharp.Modding.Interfaces;
using VoxelSharp.Modding.Structs;

namespace VoxelSharp.Modding;

public class ModLoader(string modsPath)
{
    private readonly DependencyResolver _resolver = new DependencyResolver();

    private bool _initialized = false;
    private List<IMod> _mods = [];


    public void LoadMods()
    {
        if (_initialized)
        {
            throw new InvalidOperationException("ModLoader has already been initialized.");
        }

        var assemblyPaths = DiscoverAssemblies();
        var modTypes = LoadAssemblies(assemblyPaths);
        var mods = CreateModInstances(modTypes);


        // create a dictionary of mod info to Mods
        var orderedMods = ResolveLoadingOrder(mods);

        foreach (var mod in orderedMods)
        {
            mod.PreInitialize();
        }

        foreach (var mod in orderedMods)
        {
            var harmony = new Harmony(mod.ModInfo.Id);
            mod.Initialize(harmony);
        }

        _mods = orderedMods;
        _initialized = true;
    }

    public void Update(float deltaTime)
    {
        foreach (var mod in _mods)
        {
            mod.Update(deltaTime);
        }
    }

    public void Render()
    {
        foreach (var mod in _mods)
        {
            mod.Render();
        }
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
        if (!Directory.Exists(modsPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {modsPath}");
        }

        return Directory.GetFiles(modsPath, "*.dll").ToList();
    }

    private List<Type> LoadAssemblies(IEnumerable<string> assemblyPaths)
    {
        var modTypes = new List<Type>();

        foreach (var path in assemblyPaths)
        {
            try
            {
                var assembly = Assembly.LoadFrom(path);
                var typesImplementingIMod = assembly.GetTypes()
                    .Where(type => typeof(IMod).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    .ToList();

                switch (typesImplementingIMod.Count)
                {
                    // Ensure only one implementation
                    case 1:
                        modTypes.Add(typesImplementingIMod.First());
                        break;
                    case > 1:
                        Console.WriteLine($"Assembly {path} has multiple IMod implementations. Skipping.");
                        break;
                    default:
                        Console.WriteLine(
                            $"Assembly {path} has no IMod implementations. Must be a library. Not initializing.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load assembly {path}: {ex.Message}");
            }
        }

        return modTypes;
    }

    private List<IMod> CreateModInstances(IEnumerable<Type> modTypes)
    {
        var mods = new List<IMod>();

        foreach (var type in modTypes)
        {
            try
            {
                var mod = Activator.CreateInstance(type);

                switch (mod)
                {
                    case null:
                        Console.WriteLine($"Failed to create mod instance {type.Name}: Activator returned null.");
                        continue;
                    case IMod:
                        continue;
                    default:
                        Console.WriteLine($"Failed to create mod instance {type.Name}: Does not implement IMod.");
                        break;
                }
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
        foreach (var mod in _mods)
        {
            mod.InitializeShaders();
        }
    }
}