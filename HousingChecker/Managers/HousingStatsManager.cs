using System;
using System.Collections.Generic;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
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

    private static readonly Dictionary<WardSnapshot, long> WardSnapshots = [];
    private static readonly Dictionary<LotterySnapshot, long> LotterySnapshots = [];

    internal void Init()
    {
        Service.Hook.InitializeFromAttributes(this);

        PlacardSaleInfoHook?.Enable();
        HousingWardInfoHook?.Enable();
        ExecuteCommandHook?.Enable();
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
            if (Environment.TickCount64 - lastTime > 300_000)
            {
                Service.OnlineStats.UploadWard(uploadEntry);
                WardSnapshots[uploadEntry] = Environment.TickCount64;
            }

            Service.Log.Debug("正在处于上传冷却期, 禁止上传");
            return;
        }

        Service.OnlineStats.UploadWard(uploadEntry);
        WardSnapshots[uploadEntry] = Environment.TickCount64;
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
            if (Environment.TickCount64 - lastTime > 300_000)
            {
                Service.OnlineStats.UploadLottery(uploadEntry);
                LotterySnapshots[uploadEntry] = Environment.TickCount64;
            }

            Service.Log.Debug("正在处于上传冷却期, 禁止上传");
            return;
        }

        Service.OnlineStats.UploadLottery(uploadEntry);
        LotterySnapshots[uploadEntry] = Environment.TickCount64;

        Service.OnlineStats.UploadLottery(uploadEntry);
    }

    public void ObtainResidentAreaInfo(HouseArea area)
    {
        if (area == HouseArea.未知) return;

        const int AreaCount = 30;
        for (var i = 0; i < AreaCount; i++)
            ExecuteCommandDetour(1107, (int)area, i, 0, 0);
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
        ExecuteCommandHook?.Dispose();
        ExecuteCommandHook = null;

        HousingWardInfoHook?.Dispose();
        HousingWardInfoHook = null;

        PlacardSaleInfoHook?.Dispose();
        PlacardSaleInfoHook = null;
    }
}
