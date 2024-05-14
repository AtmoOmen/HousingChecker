using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Hooking;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Component.GUI;
using HousingChecker.Info;

namespace HousingChecker.Managers;

public class HousingStatsManager
{
    private delegate void HousingWardInfoDelegate(nint housingPortalAgent, nint housingWardInfo);

    [Signature("40 55 57 41 54 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? B8", DetourName = nameof(OnHousingWardInfo))]
    private Hook<HousingWardInfoDelegate>? HousingWardInfoHook;

    private delegate void PlacardSaleInfoDelegate(
        nint housingSignboardAgent, HousingType housingType, ushort territoryTypeId, byte wardId, byte plotId,
        short apartmentNumber, nint placardSaleInfo, long a8);

    [Signature("E8 ?? ?? ?? ?? 48 8B B4 24 ?? ?? ?? ?? 48 8B 6C 24 ?? E9", DetourName = nameof(PlacardSaleInfoDetour))]
    private Hook<PlacardSaleInfoDelegate>? PlacardSaleInfoHook;

    private delegate nint ExecuteCommandDelegate(int command, int a2, int a3, int a4, int a5);

    [Signature(
        "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 8B E9 41 8B D9 48 8B 0D ?? ?? ?? ?? 41 8B F8 8B F2",
        DetourName = nameof(ExecuteCommandDetour))]
    private Hook<ExecuteCommandDelegate>? ExecuteCommandHook;

    private static readonly Dictionary<WardSnapshot, DateTime> WardSnapshots = [];
    private static readonly Dictionary<LotterySnapshot, DateTime> LotterySnapshots = [];

    private static TimeSpan FiveMinutesSpan;

    internal void Init()
    {
        FiveMinutesSpan = TimeSpan.FromMinutes(5);

        Service.Hook.InitializeFromAttributes(this);

        PlacardSaleInfoHook?.Enable();
        HousingWardInfoHook?.Enable();
        ExecuteCommandHook?.Enable();

        Service.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "HousingSelectBlock", OnAddonSelectBlock);
    }

    private void OnAddonSelectBlock(AddonEvent type, AddonArgs args)
    {
        string? title;
        unsafe
        {
            var addon = (AtkUnitBase*)args.Addon;
            if (addon == null) return;

            title = Marshal.PtrToStringUTF8((nint)addon->AtkValues[2].String);
        }
        if (string.IsNullOrWhiteSpace(title)) return;

        var finalEnum = Enum.GetValues<HouseArea>().FirstOrDefault(x => title.Contains(x.ToString()));
        if (finalEnum is HouseArea.未知) return;

        Task.Run(async () =>
        {
            await Task.Delay(1000);
            await ObtainResidentAreaInfo(finalEnum);
        });
    }

    // 房区
    public void OnHousingWardInfo(nint housingPortalAgent, nint housingWardInfoPtr)
    {
        HousingWardInfoHook!.Original(housingPortalAgent, housingWardInfoPtr);

        var wardInfo = WardInfo.Read(housingWardInfoPtr);
        var uploadEntry = new WardSnapshot(wardInfo);

        if (WardSnapshots.TryGetValue(uploadEntry, out var lastTime))
        {
            // 大于 5 分钟
            if (DateTime.Now - lastTime > FiveMinutesSpan)
            {
                Service.OnlineStats.EnqueueWard(uploadEntry);
                WardSnapshots[uploadEntry] = DateTime.Now;
            }

            return;
        }

        Service.OnlineStats.EnqueueWard(uploadEntry);
        WardSnapshots[uploadEntry] = DateTime.Now;
    }

    // 门牌
    public void PlacardSaleInfoDetour(
        nint housingSignboardAgent, HousingType housingType, ushort territoryTypeId, byte wardId, byte plotId,
        short apartmentNumber, nint placardSaleInfo, long a8)
    {
        PlacardSaleInfoHook.Original(housingSignboardAgent, housingType, territoryTypeId, wardId, plotId,
                                     apartmentNumber,
                                     placardSaleInfo, a8);

        // 非空地, 直接返回
        if (housingType != HousingType.无主房屋) return;
        if (placardSaleInfo == nint.Zero) return;

        var placardInfo = LotteryInfo.Read(placardSaleInfo);
        var uploadEntry = new LotterySnapshot(placardInfo, territoryTypeId, wardId, plotId);

        if (LotterySnapshots.TryGetValue(uploadEntry, out var lastTime))
        {
            // 大于 5 分钟
            if (DateTime.Now - lastTime > FiveMinutesSpan)
            {
                Service.OnlineStats.EnqueueLottery(uploadEntry);
                LotterySnapshots[uploadEntry] = DateTime.Now;
            }

            return;
        }

        Service.OnlineStats.EnqueueLottery(uploadEntry);
        LotterySnapshots[uploadEntry] = DateTime.Now;

        Service.OnlineStats.EnqueueLottery(uploadEntry);
    }

    public async Task ObtainResidentAreaInfo(HouseArea area)
    {
        if (area == HouseArea.未知) return;

        unsafe
        {
            var addon = (AtkUnitBase*)Service.Gui.GetAddonByName("HousingSelectBlock");
            if (addon == null)
            {
                Service.DalamudNotice.AddNotification(new()
                {
                    Title = "HousingChecker",
                    Content = "禁止上传: 未获取到可用的 选择住宅区 界面",
                    InitialDuration = TimeSpan.FromSeconds(3),
                    ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
                    Type = NotificationType.Error
                });
                return;
            }

            var title = Marshal.PtrToStringUTF8((nint)addon->AtkValues[2].String);
            if (string.IsNullOrWhiteSpace(title) || !title.Contains(area.ToString()))
            {
                Service.DalamudNotice.AddNotification(new()
                {
                    Title = "HousingChecker",
                    Content = "禁止上传: 界面错误或未选择对应房区",
                    InitialDuration = TimeSpan.FromSeconds(3),
                    ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
                    Type = NotificationType.Error
                });
                return;
            }
        }

        const int AreaCount = 30;
        for (var i = 0; i < AreaCount; i++)
        {
            ExecuteCommandDetour(1107, (int)area, i, 0, 0);
            await Task.Delay(100);
        }
    }

    public void ObtainResidentAreaHousesInfo(HouseArea area, int wardID)
    {
        if (area == HouseArea.未知) return;

        const int HousesPerArea = 60;
        const int AreaOffset = 256;

        var districtOffset = wardID * AreaOffset;
        for (var houseNumber = 1; houseNumber <= HousesPerArea; houseNumber++)
        {
            var houseOffset = (houseNumber - 1);
            var position = districtOffset + houseOffset;
            ExecuteCommandDetour(1105, (int)area, position, 0, 0);
        }
    }

    private nint ExecuteCommandDetour(int command, int a2, int a3, int a4, int a5)
    {
        var original = ExecuteCommandHook.Original(command, a2, a3, a4, a5);
        return original;
    }

    internal void Uninit()
    {
        Service.AddonLifecycle.UnregisterListener(OnAddonSelectBlock);

        ExecuteCommandHook?.Dispose();
        ExecuteCommandHook = null;

        HousingWardInfoHook?.Dispose();
        HousingWardInfoHook = null;

        PlacardSaleInfoHook?.Dispose();
        PlacardSaleInfoHook = null;
    }
}
