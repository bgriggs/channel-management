using System.Text;
using UnitsNet;

namespace BigMission.ChannelManagement.Shared;

public class ChannelValue
{
    /// <summary>
    /// ID of the channel, same as mapping and channel DTO.
    /// </summary>
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;

    public IQuantity? GetDisplayQuantity(ChannelMapping map)
    {
        var v = double.Parse(Value);
        // convert from base units to display units?
        if (map.DisplayUnitType is not null)
        {
            Quantity.TryFrom(v, map.DisplayUnitType, out IQuantity? quantity);
            return quantity;
        }
        return null;
    }

    public void SetBaseValue(double value, ChannelMapping map)
    {
        var zeros = GetZeros(map.Dto.BaseDecimalPlaces);
        Value = value.ToString("0." + zeros);
    }

    public void SetBaseValue(int value)
    {
        Value = value.ToString();
    }

    public int GetValueInt()
    {
        _ = int.TryParse(Value, out int r);
        return r;
    }

    public double GetValueDouble()
    {
        _ = double.TryParse(Value, out double r);
        return r;
    }

    private static string GetZeros(int count)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            _ = sb.Append('0');
        }
        return sb.ToString();
    }
}
