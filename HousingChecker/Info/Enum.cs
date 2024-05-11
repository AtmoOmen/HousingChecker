namespace HousingChecker.Info;

public enum HousingType : byte
{
    OwnedHouse = 0,
    UnownedHouse = 1,
    FreeCompanyApartment = 2,
    Apartment = 3
}

public enum RegionType
{
    FreeCompany, // 仅限部队购买
    Personal     // 仅限个人购买
}

public enum LotteryState
{
    Unknown,
    Purchaseable,
    ResultPeriod,
    Preparing
}

public enum PurchaseType
{
    Unavailable,
    FirstComeFirstServer,
    Lottery
}

public enum HouseSize
{
    S,
    M,
    L
}

public enum HouseArea
{
    Unknown,
    Mist = 339,
    LavenderBeds = 340,
    Goblet = 341,
    Shirogane = 641,
    Empyreum = 979
}
