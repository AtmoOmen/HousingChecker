using System;
using System.IO;

namespace HousingChecker.Info;

public class LandInfo
{
    public short LandId;
    public short TerritoryTypeId;
    public short WardNumber;
    public short WorldId;

    public static LandInfo ReadFromBinaryReader(BinaryReader binaryReader)
    {
        var info = new LandInfo
        {
            LandId = binaryReader.ReadInt16(),
            WardNumber = binaryReader.ReadInt16(),
            TerritoryTypeId = binaryReader.ReadInt16(),
            WorldId = binaryReader.ReadInt16()
        };
        return info;
    }
}

public class HouseInfoEntry
{
    public string EstateOwnerName = string.Empty;
    public sbyte[] HouseAppeals = null!;
    public uint HousePrice;
    public HousingFlags InfoFlags;
}

[Flags]
public enum HousingFlags : byte
{
    PlotOwned = 1 << 0,
    VisitorsAllowed = 1 << 1,
    HasSearchComment = 1 << 2,
    HouseBuilt = 1 << 3,
    OwnedByFC = 1 << 4
}

public enum TenantType : byte
{
    FreeCompany = 1,
    Personal = 2
}
