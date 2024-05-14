using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HousingChecker.Managers;

namespace HousingChecker;

public class Service
{
    public const string CommandName = "/housingchecker";

    public static void Init(DalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
        pluginInterface.Create<Service>();

        Config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Config.Init();

        WindowManager.Init();
        CommandHandler();
        OnlineStats.Init();
        HousingStats.Init();
    }

    public static void Uninit()
    {
        HousingStats.Uninit();
        OnlineStats.Uninit();
        WindowManager.Uninit();
        Config.Uninit();
    }

    private static void CommandHandler()
    {
        const string helpMessage = "打开设置界面";

        Command.RemoveHandler(CommandName);
        Command.AddHandler(CommandName, new CommandInfo(OnCommand) { HelpMessage = helpMessage });
    }

    private static void OnCommand(string command, string args)
    {
        WindowManager.ConfigWindow.IsOpen ^= true;
    }

    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static ICommandManager Command { get; set; } = null!;
    [PluginService] public static IDataManager Data { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IGameGui Gui { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider Hook { get; private set; } = null!;
    [PluginService] public static INotificationManager DalamudNotice { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;

    public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    public static SigScanner SigScanner { get; private set; } = new();
    public static Configuration Config { get; private set; } = null!;
    public static WindowManager WindowManager { get; private set; } = new();
    public static HousingStatsManager HousingStats { get; private set; } = new();
    public static OnlineStatsManager OnlineStats { get; private set; } = new();
}
