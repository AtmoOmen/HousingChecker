using System;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using HousingChecker.Helpers;

namespace HousingChecker.Info;

public class WardSnapshot : IEquatable<WardSnapshot>
{
    /// <summary>
    /// 服务器 Unix 时间
    /// </summary>
    public long time;

    /// <summary>
    /// 服务器 ID
    /// </summary>
    public int server;

    /// <summary>
    /// 房区中文名称
    /// </summary>
    public string area = string.Empty;

    /// <summary>
    /// 房区 ID
    /// </summary>
    public int slot;

    /// <summary>
    /// 主要购买方式
    /// </summary>
    public int purchase_main;

    /// <summary>
    /// 次要购买方式
    /// </summary>
    public int purchase_sub;

    /// <summary>
    /// 主要区域类型
    /// </summary>
    public int region_main;

    /// <summary>
    /// 次要区域类型
    /// </summary>
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
                isPersonal = houseInfo.InfoFlags.HasFlag(HousingFlags.PlotOwned) &&
                             !houseInfo.InfoFlags.HasFlag(HousingFlags.OwnedByFC),
                isEmpty = !houseInfo.InfoFlags.HasFlag(HousingFlags.PlotOwned),
                isPublic = houseInfo.InfoFlags.HasFlag(HousingFlags.VisitorsAllowed),
                hasGreeting = houseInfo.InfoFlags.HasFlag(HousingFlags.HasSearchComment)
            };
        }
    }

    public bool Equals(WardSnapshot? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return server == other.server && area == other.area && slot == other.slot;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((WardSnapshot)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(server, area, slot);
    }

    public static bool operator ==(WardSnapshot? lhs, WardSnapshot? rhs)
    {
        if (lhs is null) return rhs is null;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(WardSnapshot lhs, WardSnapshot rhs)
    {
        return !(lhs == rhs);
    }
}

public class HousingItem : IEquatable<HousingItem>
{
    /// <summary>
    /// 房屋 ID, 从 1 开始, 60 结束
    /// </summary>
    public int id;

    /// <summary>
    /// 所有人 ID
    /// </summary>
    public string owner = string.Empty;

    /// <summary>
    /// 价格
    /// </summary>
    public int price;

    /// <summary>
    /// 土地尺寸中文名称
    /// </summary>
    public string size = string.Empty;

    /// <summary>
    /// 房屋标签
    /// </summary>
    public int[] tags = null!;

    /// <summary>
    /// 是否为个人房屋
    /// </summary>
    public bool isPersonal;

    /// <summary>
    /// 是否为空地
    /// </summary>
    public bool isEmpty;

    /// <summary>
    /// 是否开放
    /// </summary>
    public bool isPublic;

    /// <summary>
    /// 是否有问候语
    /// </summary>
    public bool hasGreeting;

    public HousingItem() { }

    public bool Equals(HousingItem? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return id == other.id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((HousingItem)obj);
    }

    public override int GetHashCode()
    {
        return id;
    }

    public static bool operator ==(HousingItem? lhs, HousingItem? rhs)
    {
        if (lhs is null) return rhs is null;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(HousingItem lhs, HousingItem rhs)
    {
        return !(lhs == rhs);
    }
}

public class LotterySnapshot : IEquatable<LotterySnapshot>
{
    /// <summary>
    /// 服务器 ID
    /// </summary>
    public int ServerId { get; }

    /// <summary>
    /// 房区序号
    /// </summary>
    public int Area { get; }

    /// <summary>
    /// 房区ID, 从0开始, 29 结束
    /// </summary>
    public int Slot { get; }

    /// <summary>
    /// 房屋ID, 从1开始, 60 结束
    /// </summary>
    public int LandID { get; }

    /// <summary>
    /// 服务器 Unix 时间
    /// </summary>
    public long Time { get; }

    /// <summary>
    /// 结束时间, Unix 时间
    /// </summary>
    public uint EndTime { get; }

    /// <summary>
    /// 抽选阶段状态
    /// </summary>
    public int State { get; }

    /// <summary>
    /// 参与人数
    /// </summary>
    public uint Participate { get; }

    /// <summary>
    /// 胜选编号
    /// </summary>
    public uint Winner { get; }

    public LotterySnapshot(LotteryInfo info, uint territoryID, int wardID, int plotID)
    {
        Time = Framework.GetServerTime();
        ServerId = (int)Service.ClientState.LocalPlayer.CurrentWorld.Id;
        Area = Utils.ToHouseAreaNumber((HouseArea)territoryID);
        Slot = wardID;
        LandID = plotID + 1;
        State = (int)info.LotteryState;
        Participate = info.EntryCount;
        Winner = info.Winner;
        EndTime = info.PhaseEndsAt;
    }

    public bool Equals(LotterySnapshot? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ServerId == other.ServerId && Area == other.Area && Slot == other.Slot && LandID == other.LandID;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((LotterySnapshot)obj);
    }

    public override int GetHashCode() => HashCode.Combine(ServerId, Area, Slot, LandID);

    public static bool operator ==(LotterySnapshot? lhs, LotterySnapshot? rhs)
    {
        if (lhs is null) return rhs is null;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(LotterySnapshot lhs, LotterySnapshot rhs) => !(lhs == rhs);
}

public class HousingSellInfo : IEquatable<HousingSellInfo>
{
    /// <summary>
    /// 服务器 ID
    /// </summary>
    public int Server { get; set; }

    /// <summary>
    /// 区域 ID
    /// </summary>
    public int Area { get; set; }

    /// <summary>
    /// 房区 ID, 由 0 开始
    /// </summary>
    public int Slot { get; set; }

    /// <summary>
    /// 房屋 ID, 由 1 开始
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// 价格
    /// </summary>
    public int Pirce { get; set; }

    /// <summary>
    /// 房屋尺寸
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// 首次发现时间, Unix 时间戳
    /// </summary>
    public int FirstSeen { get; set; }

    /// <summary>
    /// 上次发现时间, Unix 时间戳
    /// </summary>
    public int LastSeen { get; set; }

    /// <summary>
    /// 抽签状态
    /// </summary>
    public int State { get; set; }

    /// <summary>
    /// 参与人数
    /// </summary>
    public int Participate { get; set; }

    /// <summary>
    /// 胜选编号
    /// </summary>
    public int Winner { get; set; }

    /// <summary>
    /// 当前阶段结束时间, Unix 时间戳
    /// </summary>
    public int EndTime { get; set; }

    /// <summary>
    /// 上次抽签信息更新时间
    /// </summary>
    public int UpdateTime { get; set; }

    /// <summary>
    /// 购买方式
    /// </summary>
    public int PurchaseType { get; set; }

    /// <summary>
    /// 房屋用途限制
    /// </summary>
    public int RegionType { get; set; }

    public HousingSellInfo() { }

    public bool Equals(HousingSellInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Server == other.Server && Area == other.Area && Slot == other.Slot && ID == other.ID;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((HousingSellInfo)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Server, Area, Slot, ID);

    public static bool operator ==(HousingSellInfo? lhs, HousingSellInfo? rhs)
    {
        if (lhs is null) return rhs is null;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(HousingSellInfo lhs, HousingSellInfo rhs) => !(lhs == rhs);
}
