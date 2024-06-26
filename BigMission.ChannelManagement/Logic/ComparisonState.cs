﻿namespace BigMission.ChannelManagement.Logic;

/// <summary>
/// State information from a statement evaluation.
/// </summary>
public class ComparisonState
{
    public int ComparisonId { get; set; }
    
    /// <summary>
    /// Is the statement active.
    /// </summary>
    public bool IsTrue { get; set; }
    
    /// <summary>
    /// Channel value for the Updated logic.
    /// </summary>
    public string LastValue { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp at which the statement went true. 
    /// This is used for the 'on for' clause.
    /// </summary>
    public DateTime? ForTimestamp { get; set; }
}
