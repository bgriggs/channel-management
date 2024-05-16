﻿namespace BigMission.ChannelManagement.Timer;

public class TimerParameters
{
    public int Id { get; set; }
    public int OutputChId { get; set; }
    public int StartStatementId { get; set; }
    public int StopStatementId { get; set; }
    public bool CountDown { get; set; }
    public bool EnableRollover { get; set; }
    public int RolloverSeconds { get; set; }
    public bool EnableStartSeconds { get; set; }
    public int StartSeconds { get; set; }
    public bool EnableStopSeconds { get; set; }
    public int StopSeconds { get; set; }
}
