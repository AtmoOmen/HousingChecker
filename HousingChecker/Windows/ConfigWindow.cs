using System;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using HousingChecker.Info;
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
            Util.OpenLink("https://house.ffxiv.cyou/");

        ImGui.SameLine();
        if (ImGui.Button("Github"))
            Util.OpenLink("https://github.com/AtmoOmen/HousingChecker");

        ImGui.Spacing();

        ImGui.Separator();

        ImGui.Spacing();

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

        ImGui.Spacing();

        ImGui.Separator();

        ImGui.Spacing();

        ImGui.TextColored(ImGuiColors.DalamudOrange, "快捷上传:");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("房区信息:");

        foreach (var area in Enum.GetValues<HouseArea>())
        {
            if (area is HouseArea.未知) continue;

            ImGui.PushID($"{area}-AreaInfo");
            ImGui.SameLine();
            if (ImGui.Button($"{area}###AreaInfo"))
                Service.HousingStats.ObtainResidentAreaInfo(area);
            ImGui.PopID();
        }
    }

    public void Dispose() { }
}
