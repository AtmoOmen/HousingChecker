using HousingChecker.Info;

namespace HousingChecker;

public class Utils
{
    public static string ToHouseSizeString(HouseSize size)
    {
        return size switch
        {
            HouseSize.S => "S",
            HouseSize.M => "M",
            HouseSize.L => "L",
            _ => "未知"
        };
    }

    public static string ToHouseAreaString(HouseArea area)
    {
        return area switch
        {
            HouseArea.Mist => "海雾村",
            HouseArea.LavenderBeds => "薰衣草苗圃",
            HouseArea.Goblet => "高脚孤丘",
            HouseArea.Shirogane => "白银乡",
            HouseArea.Empyreum => "穹顶皓天",
            _ => "未知"
        };
    }

    public static uint TerritoryTypeIdToLandSetId(uint territoryTypeId)
    {
        return territoryTypeId switch
        {
            641 => 3,                  // 白银乡
            979 => 4,                  // 穹顶皓天
            _ => territoryTypeId - 339 // 海雾村, 薰衣草苗圃, 高脚孤丘
        };
    }
}
