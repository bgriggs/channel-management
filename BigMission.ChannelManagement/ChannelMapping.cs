using UnitsNet;

namespace BigMission.ChannelManagement;

public class ChannelMapping
{
    public int Id { get => Dto?.Id ?? 0; }
    public QuantityInfo? DateTypeInfo { get; }
    public Enum? BaseUnitType { get; }
    public Enum? DisplayUnitType { get; }

    public ChannelMappingDto Dto { get; }

    public ChannelMapping(ChannelMappingDto dto)
    {
        Dto = dto;
        if (!dto.IsStringValue)
        {
            DateTypeInfo = Quantity.ByName[dto.DataType];
            BaseUnitType = UnitsNetSetup.Default.UnitParser.Parse(dto.BaseUnitType, DateTypeInfo.UnitType);
            DisplayUnitType = UnitsNetSetup.Default.UnitParser.Parse(dto.DisplayUnitType, DateTypeInfo.UnitType);
        }
    }

}
