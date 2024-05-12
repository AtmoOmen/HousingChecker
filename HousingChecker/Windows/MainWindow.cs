using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace HousingChecker.Windows;

public class MainWindow() : Window("设置###HousingChecker",
                                   ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse |
                                   ImGuiWindowFlags.NoScrollbar |
                                   ImGuiWindowFlags.NoScrollWithMouse), IDisposable
{
    public override void Draw()
    {
        
    }

    public void Dispose()
    {

    }
}
