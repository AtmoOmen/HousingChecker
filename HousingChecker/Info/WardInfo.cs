using System.IO;
using System.Text;
using System;

namespace HousingChecker.Info;

public class WardInfo
{
    public HouseInfoEntry[] HouseInfoEntries = null!;
    public LandInfo LandInfo = null!;
    public PurchaseType PurchaseType0;
    public PurchaseType PurchaseType1;
    public TenantType TenantType0;
    public TenantType TenantType1;

    public static unsafe WardInfo Read(IntPtr dataPtr)
    {
        var wardInfo = new WardInfo();
        using var stream = new UnmanagedMemoryStream((byte*)dataPtr.ToPointer(), 2664L);
        using var binaryReader = new BinaryReader(stream);
        wardInfo.LandInfo = LandInfo.ReadFromBinaryReader(binaryReader);
        wardInfo.HouseInfoEntries = new HouseInfoEntry[60];

        for (var i = 0; i < 60; i++)
        {
            var infoEntry = new HouseInfoEntry
            {
                HousePrice = binaryReader.ReadUInt32(),
                InfoFlags = (HousingFlags)binaryReader.ReadByte(),
                HouseAppeals = new sbyte[3]
            };
            for (var j = 0; j < 3; j++) infoEntry.HouseAppeals[j] = binaryReader.ReadSByte();
            infoEntry.EstateOwnerName = Encoding.UTF8.GetString(binaryReader.ReadBytes(32)).TrimEnd(new char[1]);
            wardInfo.HouseInfoEntries[i] = infoEntry;

            if ((infoEntry.InfoFlags & HousingFlags.PlotOwned) == 0)
                infoEntry.EstateOwnerName = "";
        }

        // 0x2440
        wardInfo.PurchaseType0 = (PurchaseType)binaryReader.ReadByte();
        // 0x2441 - padding byte?
        wardInfo.PurchaseType1 = (PurchaseType)binaryReader.ReadByte();
        // 0x2442
        wardInfo.TenantType0 = (TenantType)binaryReader.ReadByte();
        // 0x2443 - padding byte?
        wardInfo.TenantType1 = (TenantType)binaryReader.ReadByte();
        // 0x2444 - 0x2447 appear to be padding bytes

        return wardInfo;
    }
}
