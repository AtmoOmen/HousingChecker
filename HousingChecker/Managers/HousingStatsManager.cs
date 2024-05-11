using System.Collections.Generic;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Lumina.Excel.GeneratedSheets;
using HousingChecker.Info;

namespace HousingChecker.Managers;

public unsafe class HousingStatsManager
{
    private delegate void HousingWardInfoDelegate(nint housingPortalAgent, nint housingWardInfo);

    [Signature("40 55 57 41 54 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? B8", DetourName = nameof(OnHousingWardInfo))]
    private Hook<HousingWardInfoDelegate>? HousingWardInfoHook;

    private delegate void PlacardSaleInfoDelegate
        (nint agentBase, HousingType housingType, ushort territoryTypeId, byte wardId, byte plotId, short apartmentNumber, nint placardSaleInfo, long a8);
    [Signature("E8 ?? ?? ?? ?? 48 8B B4 24 ?? ?? ?? ?? 48 8B 6C 24 ?? E9", DetourName = nameof(PlacardSaleInfoDetour))]
    private Hook<PlacardSaleInfoDelegate>? PlacardSaleInfoHook;

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
        Service.OnlineStats.UploadWard(new List<WardSnapshot> { uploadEntry });
    }


    // 门牌
    public void PlacardSaleInfoDetour(
        nint agentBase, HousingType housingType, ushort territoryTypeId, byte wardId, byte plotId, short apartmentNumber,
        nint placardSaleInfo, long a8)
    {
        PlacardSaleInfoHook.Original(agentBase, housingType, territoryTypeId, wardId, plotId, apartmentNumber, placardSaleInfo, a8);

        if (housingType != HousingType.UnownedHouse) return;
        if (placardSaleInfo == nint.Zero) return;

        Service.Log.Debug(
            $"housingType={housingType}, territoryTypeId={territoryTypeId}, wardId={wardId}, plotId={plotId}, apartmentNumber={apartmentNumber}, placardSaleInfoPtr={placardSaleInfo}, a8={a8}");

        var world = Service.ClientState.LocalPlayer.CurrentWorld.GameData;
        if (world is null) return;

        var place = Service.Data.GetExcelSheet<TerritoryType>().GetRow(territoryTypeId)?.PlaceName.Value?.Name;
        var worldName = world.Name;
        Service.Log.Info($"Plot {place} {wardId + 1}-{plotId + 1} ({worldName}) has {1} lottery entries.");

        // plugin.PaissaClient.PostLotteryInfo(world.RowId, territoryTypeId, wardId, plotId, saleInfo);
    }

    internal void Uninit()
    {
        HousingWardInfoHook?.Dispose();
        HousingWardInfoHook = null;

        PlacardSaleInfoHook?.Dispose();
        PlacardSaleInfoHook = null;
    }
}
