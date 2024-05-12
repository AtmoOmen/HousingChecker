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

    private delegate void PlacardSaleInfoDelegate(nint agentBase, HousingType housingType, ushort territoryTypeId, byte wardId,
                                                  byte plotId, short apartmentNumber, nint placardSaleInfo, long a8);
    [Signature("E8 ?? ?? ?? ?? 48 8B B4 24 ?? ?? ?? ?? 48 8B 6C 24 ?? E9", DetourName = nameof(PlacardSaleInfoDetour))]
    private Hook<PlacardSaleInfoDelegate>? PlacardSaleInfoHook;

    private static readonly Dictionary<WardSnapshot, long> WardSnapshots = [];
    private static readonly Dictionary<LotterySnapshot, long> LotterySnapshots = [];

    internal void Init()
    {
        Service.Hook.InitializeFromAttributes(this);

        PlacardSaleInfoHook?.Enable();
        HousingWardInfoHook?.Enable();
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
        nint agentBase, HousingType housingType, ushort territoryTypeId, byte wardId, byte plotId,
        short apartmentNumber, nint placardSaleInfo, long a8)
    {
        PlacardSaleInfoHook.Original(agentBase, housingType, territoryTypeId, wardId, plotId, apartmentNumber,
                                     placardSaleInfo, a8);

        // 非空地, 直接返回
        if (housingType != HousingType.UnownedHouse) return;
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

    internal void Uninit()
    {
        HousingWardInfoHook?.Dispose();
        HousingWardInfoHook = null;

        PlacardSaleInfoHook?.Dispose();
        PlacardSaleInfoHook = null;
    }
}
