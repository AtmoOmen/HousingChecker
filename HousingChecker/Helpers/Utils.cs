using HousingChecker.Info;

namespace HousingChecker.Helpers;

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

    public static int ToHouseAreaNumber(HouseArea area)
    {
        return area switch
        {
            HouseArea.Unknown => -1,
            HouseArea.Mist => 0,
            HouseArea.LavenderBeds => 1,
            HouseArea.Goblet => 2,
            HouseArea.Shirogane => 3,
            HouseArea.Empyreum => 4,
            _ => -1
        };
    }

    public static string HouseAreaNumberToString(int area)
    {
        return area switch
        {
            0 => "海雾村",
            1 => "薰衣草苗圃",
            2 => "高脚孤丘",
            3 => "白银乡",
            4 => "穹顶皓天",
            _ => "未知"
        };
    }

    public static int HouseAreaToTerritory(int area)
    {
        return area switch
        {
            0 => 339,
            1 => 340,
            2 => 341,
            3 => 641,
            4 => 979,
            _ => 0
        };
    }
}
