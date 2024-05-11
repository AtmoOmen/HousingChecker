using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Lumina.Excel.GeneratedSheets;
using System.Linq;

namespace HousingChecker.Info;

public class WardSnapshot
{
    public long time;
    public int server;
    public string area = string.Empty;
    public int slot;
    public int purchase_main;
    public int purchase_sub;
    public int region_main;
    public int region_sub;

    public HousingItem[] houses = null!;

    public WardSnapshot() { }

    public WardSnapshot(WardInfo info)
    {
        time = Framework.GetServerTime();
        server = info.LandInfo.WorldId;
        area = Utils.ToHouseAreaString((HouseArea)info.LandInfo.TerritoryTypeId);
        slot = info.LandInfo.WardNumber;
        purchase_main = (int)info.PurchaseType0;
        purchase_sub = (int)info.PurchaseType1;
        region_main = (int)info.TenantType0;
        region_sub = (int)info.TenantType1;
        houses = new HousingItem[info.HouseInfoEntries.Length];

        var landSet = Service.Data.GetExcelSheet<HousingLandSet>().GetRow
            (Utils.TerritoryTypeIdToLandSetId((uint)info.LandInfo.TerritoryTypeId));

        for (var i = 0; i < info.HouseInfoEntries.Length; i++)
        {
            var houseInfo = info.HouseInfoEntries[i];
            houses[i] = new HousingItem
            {
                id = i + 1,
                owner = houseInfo.EstateOwnerName,
                price = (int)houseInfo.HousePrice,
                size = Utils.ToHouseSizeString((HouseSize)landSet?.PlotSize[i]!),
                tags = houseInfo.HouseAppeals.Select(x => (int)x).Take(3).ToArray(),
                isPersonal = houseInfo.InfoFlags.HasFlag(HousingFlags.PlotOwned) && !houseInfo.InfoFlags.HasFlag(HousingFlags.OwnedByFC),
                isEmpty = !houseInfo.InfoFlags.HasFlag(HousingFlags.PlotOwned),
                isPublic = houseInfo.InfoFlags.HasFlag(HousingFlags.VisitorsAllowed),
                hasGreeting = houseInfo.InfoFlags.HasFlag(HousingFlags.HasSearchComment)
            };
        }
    }
}

public class HousingItem
{
    public int id;
    public string owner = string.Empty;
    public int price;
    public string size = string.Empty;
    public int[] tags = null!;
    public bool isPersonal;
    public bool isEmpty;
    public bool isPublic;
    public bool hasGreeting;

    public HousingItem() { }
}
