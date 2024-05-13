using System;
using System.Reflection;
using Dalamud.Plugin;

namespace HousingChecker;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "HousingChecker";
    public static Version? Version { get; private set; }

    public Plugin(DalamudPluginInterface pluginInterface)
    {
        Version ??= Assembly.GetExecutingAssembly().GetName().Version;

        Service.Init(pluginInterface);
    }

    public void Dispose()
    {
        Service.Uninit();
    }
}
