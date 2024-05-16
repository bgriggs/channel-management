namespace BigMission.ChannelManagement.Logic;

public class Comparison
{
    public int ComparisonId { get; set; }
    public int ChannelId { get; set; }
    public LogicType Logic { get; set; }
    public bool UseStaticComparison { get; set; }
    public string StaticValueComparison { get; set; } = string.Empty;
    public int ChannelComparisonId { get; set; }
    public int ForMs { get; set; }
    public bool ReverseResult { get; set; }
}
