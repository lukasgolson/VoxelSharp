using System.Reflection;
using HarmonyLib;
using Microsoft.Extensions.Logging;
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

    private readonly ILogger<ModLoader> _logger;

    public ModLoader(ILogger<ModLoader> logger)
    {
        _logger = logger;
    }

    public void LoadMods(string modsPath)
    {
        if (Loaded) throw new InvalidOperationException("Mods have already been loaded.");

        var path = Path.GetFullPath(modsPath);


        var assemblyPaths = DiscoverAssemblies(path);


        _logger.LogInformation("Found {AssemblyPathsCount} assemblies in {Path}", assemblyPaths.Count, path);


        var modTypes = LoadAssemblies(assemblyPaths);

        switch (assemblyPaths.Count)
        {
            case 0:
                _logger.LogWarning("No assemblies found in {Path}", path);
                return;
            case 1:
                _logger.LogInformation("Found {ModTypesCount} mod interface in {AssemblyPathsCount} assembly",
                    modTypes.Count, assemblyPaths.Count);
                break;
            default:
                _logger.LogInformation("Found {ModTypesCount} mod interfaces in {AssemblyPathsCount} assemblies",
                    modTypes.Count, assemblyPaths.Count);
                break;
        }


        var mods = CreateModInstances(modTypes);

        _logger.LogInformation("Created {ModsCount} mod instances", mods.Count);


        var orderedMods = ResolveLoadingOrder(mods);

        _logger.LogInformation("Resolved loading order for {OrderedModsCount} mods", orderedMods.Count);

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

            _logger.LogInformation("Pre-initialized mod {ModName} v{ModVersion}", mod.ModInfo.Name,
                mod.ModInfo.Version);
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

            _logger.LogInformation("Initialized mod {ModName} v{ModVersion}", mod.ModInfo.Name, mod.ModInfo.Version);
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

    private List<Type> LoadAssemblies(IEnumerable<string> assemblyPaths)
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
                _logger.LogError(ex, "Failed to load assembly {Path}", path);
            }

        return modTypes;
    }

    private List<IMod> CreateModInstances(IEnumerable<Type> modTypes)
    {
        var mods = new List<IMod>();

        foreach (var type in modTypes)
        {
            _logger.LogInformation("Creating mod instance {TypeName}", type.Name);

            try
            {
                if (Activator.CreateInstance(type) is not IMod mod) continue;

                mods.Add(mod);

                _logger.LogInformation("Created mod instance {ModName} v{ModVersion}", mod.ModInfo.Name,
                    mod.ModInfo.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create mod instance {TypeName}", type.Name);
            }
        }

        return mods;
    }

    public void InitializeShaders()
    {
        foreach (var mod in _mods) mod.InitializeShaders();
    }
}