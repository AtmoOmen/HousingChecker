using System.IO;

namespace HousingChecker.Info;

public class LotteryInfo
{
    public PurchaseType PurchaseType; // 0x20
    public TenantType TenantType;     // 0x21
    public LotteryState LotteryState; // 0x22
    public byte Unknown1;             // 0x23
    public uint Unknown2;             // 0x24 - 0x27
    public uint PhaseEndsAt;          // 0x28 - 0x2B
    public uint Unknown3;             // 0x2C - 0x2F
    public uint EntryCount;           // 0x30 - 0x33
    public uint Winner;               // 0x34 - 0x37

    public static unsafe LotteryInfo Read(nint dataPtr)
    {
        using var stream = new UnmanagedMemoryStream((byte*)dataPtr.ToPointer(), 32);
        using var binaryReader = new BinaryReader(stream);

        var saleInfo = new LotteryInfo
        {
            PurchaseType = (PurchaseType)binaryReader.ReadByte(),
            TenantType = (TenantType)binaryReader.ReadByte(),
            LotteryState = (LotteryState)binaryReader.ReadByte(),
            Unknown1 = binaryReader.ReadByte(),
            Unknown2 = binaryReader.ReadUInt32(),
            PhaseEndsAt = binaryReader.ReadUInt32(),
            Unknown3 = binaryReader.ReadUInt32(),
            EntryCount = binaryReader.ReadUInt32(),
            Winner = binaryReader.ReadUInt32(),
        };

        return saleInfo;
    }
}
