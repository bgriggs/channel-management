using Newtonsoft.Json;

namespace BigMission.ChannelManagement;

public class ChannelMappingDto
{
    public int Id { get; set; }

    /// <summary>
    /// Is the channel user editable.
    /// </summary>
    public bool IsReserved { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;

    /// <summary>
    /// String value for enumerations or non-numeric Quantities.
    /// </summary>
    public bool IsStringValue { get; set; }
    public string DataType { get; set; } = string.Empty;
    public string BaseUnitType { get; set; } = string.Empty;
    public int BaseDecimalPlaces { get; set; }
    public string DisplayUnitType { get; set; } = string.Empty;
    public int DisplayDecimalPlaces { get; set; }

    /// <summary>
    /// Makes a deep copy of the model. 
    /// </summary>
    public ChannelMappingDto Copy()
    {
        var json = JsonConvert.SerializeObject(this);
        return JsonConvert.DeserializeObject<ChannelMappingDto>(json)!;
    }
}