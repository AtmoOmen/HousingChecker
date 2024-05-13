using System;
using Dalamud.Configuration;

namespace HousingChecker;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public string Token { get; set; } = string.Empty;


    public void Init() { }

    public void Save()
    {
        Service.PluginInterface.SavePluginConfig(this);
    }

    public void Uninit() { }
}
