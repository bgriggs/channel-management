namespace BigMission.ChannelManagement.Shared
{
    public class ChannelMappingDto
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        /// <summary>
        /// String value for enumerations or non-numeric Quantities.
        /// </summary>
        public bool IsStringValue { get; set; }
        public string DataType { get; set; }
        public string BaseUnitType { get; set; }
        public int BaseDecimalPlaces { get; set; }
        public string DisplayUnitType { get; set; }
        public int DisplayDecimalPlaces { get; set; }
    }
}