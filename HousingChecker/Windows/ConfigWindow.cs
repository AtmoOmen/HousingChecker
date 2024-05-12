using System;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;

namespace HousingChecker.Windows;

public class ConfigWindow() : Window("设置###HousingChecker",
                                     ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse |
                                     ImGuiWindowFlags.NoScrollbar |
                                     ImGuiWindowFlags.NoScrollWithMouse), IDisposable
{
    public override void Draw()
    {
        ImGui.SetWindowFontScale(2f);
        ImGui.Text("Housing Checker");
        ImGui.SetWindowFontScale(1f);

        if (ImGui.Button("艾欧泽亚售楼中心"))
            Util.OpenLink("https://househelper.ffxiv.cyou/");

        ImGui.SameLine();
        if (ImGui.Button("Github"))
            Util.OpenLink("https://github.com/AtmoOmen/HousingChecker");

        ImGui.Separator();

        var tokenInput = Service.Config.Token;
        ImGui.SetNextItemWidth(300f * ImGuiHelpers.GlobalScale);
        ImGui.InputText("上传 Token", ref tokenInput, 100);

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            Service.Config.Token = tokenInput;
            Service.Config.Save();

            Service.OnlineStats.Init();
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("在此输入从网站 用户管理 处获得的 Token\n留空则以匿名的方式上传数据");
    }

    public void Dispose() { }
}
