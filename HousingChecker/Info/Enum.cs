namespace HousingChecker.Info;

public enum HousingType : byte
{
    有主房屋 = 0,
    无主房屋 = 1,
    部队房间 = 2,
    公寓 = 3
}

public enum RegionType
{
    仅限部队, // 仅限部队购买
    仅限个人  // 仅限个人购买
}

public enum LotteryState
{
    可抽选 = 1,
    结果公示 = 2,
    不可抽选 = 3
}

public enum PurchaseType
{
    不可购买,
    先到先得,
    抽选
}

public enum HouseSize
{
    S,
    M,
    L
}

public enum HouseArea
{
    未知,
    海雾村 = 339,
    薰衣草苗圃 = 340,
    高脚孤丘 = 341,
    白银乡 = 641,
    穹顶皓天 = 979
}
