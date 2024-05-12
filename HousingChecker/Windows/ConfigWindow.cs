using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using HousingChecker.Helpers;
using ImGuiNET;

namespace HousingChecker.Windows;

public class ConfigWindow() : Window("设置###HousingChecker",
                                     ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse |
                                     ImGuiWindowFlags.NoScrollbar |
                                     ImGuiWindowFlags.NoScrollWithMouse), IDisposable
{
    private static readonly HashSet<uint> ValidZones = [339, 339, 340, 641, 979];

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

        var addon = Service.Gui.GetAddonByName("HousingSelectBlock");
        ImGui.BeginDisabled(addon == nint.Zero);
        if (ImGui.Button("一键上传当前房区数据"))
        {
            Task.Run(async () =>
            {
                for (var i = 0; i < 30; i++)
                {
                    unsafe
                    {
                        AgentHelper.SendEvent(AgentId.HousingPortal, 1, 1, i);
                    }

                    await Task.Delay(100);
                }
            });
        }
        ImGui.EndDisabled();

        if (addon == nint.Zero)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
            ImGui.PushStyleColor(ImGuiCol.TextDisabled, ImGuiColors.DalamudYellow);
            ImGuiComponents.HelpMarker("未找到房区传送列表, 请先打开你想要上传数据的房区的传送列表");
            ImGui.PopStyleColor(2);
        }
    }

    public void Dispose() { }
}
