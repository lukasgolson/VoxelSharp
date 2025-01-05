using System.Reflection;
using HarmonyLib;
using SimpleInjector;
using VoxelSharp.Modding.Interfaces;
using VoxelSharp.Modding.Structs;

namespace VoxelSharp.Modding;

public class ModLoader
{
    private readonly DependencyResolver _resolver = new();

    private List<IMod> _mods = [];
    private readonly Dictionary<string, Harmony> _harmonyInstances = new();

    public bool Loaded { get; private set; }

    public bool Preinitialized { get; private set; }

    public bool Initialized { get; private set; }

    public void LoadMods(string modsPath)
    {
        if (Loaded) throw new InvalidOperationException("Mods have already been loaded.");

        var path = Path.GetFullPath(modsPath);


        var assemblyPaths = DiscoverAssemblies(path);

        Console.WriteLine($"Found {assemblyPaths.Count} assemblies in {path}");

        var modTypes = LoadAssemblies(assemblyPaths);

        Console.WriteLine($"Found {modTypes.Count} mod interfaces in {assemblyPaths.Count} assemblies");

        var mods = CreateModInstances(modTypes);

        Console.WriteLine($"Created {mods.Count} mod instances");


        // create a dictionary of mod info to Mods
        var orderedMods = ResolveLoadingOrder(mods);

        Console.WriteLine($"Resolved loading order for {orderedMods.Count} mods");


        _mods = orderedMods;
        Loaded = true;
    }


    public void PreInitializeMods(Container container)
    {
        if (!Loaded)
            throw new InvalidOperationException("Mods have not been loaded yet. Call " + nameof(LoadMods) + " first.");

        if (Preinitialized) throw new InvalidOperationException("Mods have already been pre-initialized.");

        foreach (var mod in _mods)
        {
            var harmony = GetHarmony(mod.ModInfo.Id);
            mod.PreInitialize(harmony, container);

            Console.WriteLine($"Pre-initialized mod {mod.ModInfo.Name} v{mod.ModInfo.Version}");
        }

        Preinitialized = true;
    }


    public void InitializeMods(Container container)
    {
        if (!Loaded)
            throw new InvalidOperationException("Mods have not been loaded yet. Call " + nameof(LoadMods) + " first.");

        if (!Preinitialized)
            throw new InvalidOperationException("Mods have not been pre-initialized yet. Call " +
                                                nameof(PreInitializeMods) + " first.");

        if (Initialized) throw new InvalidOperationException("Mods have already been initialized.");


        foreach (var mod in _mods)
        {
            var harmony = GetHarmony(mod.ModInfo.Id);
            mod.Initialize(harmony, container);

            Console.WriteLine($"Initialized mod {mod.ModInfo.Name} v{mod.ModInfo.Version}");
        }

        Initialized = true;
    }

    private Harmony GetHarmony(string modId)
    {
        if (_harmonyInstances.TryGetValue(modId, out var harmony)) return harmony;
        harmony = new Harmony(modId);
        _harmonyInstances.Add(modId, harmony);

        return harmony;
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


    private static List<string> DiscoverAssemblies(string modsPath)
    {
        if (!Directory.Exists(modsPath)) throw new DirectoryNotFoundException($"Directory not found: {modsPath}");

        return Directory.GetFiles(modsPath, "*.dll").ToList();
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
                if (Activator.CreateInstance(type) is not IMod mod) continue;

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